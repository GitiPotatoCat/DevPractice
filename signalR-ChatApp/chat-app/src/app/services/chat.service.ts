import { computed, Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';


export interface ChatMessage {
    user: string;
    message: string;
    timestamp: Date;
    privateWith?: string;
    mine: boolean;
}

@Injectable({ providedIn: 'root' })
export class ChatService {
    private hub!: signalR.HubConnection;
    private currentUser = '';


    // ===== typing state =====
    private isTyping = false;
    private typingTarget: string | null = null;
    private typingHeartbeat: ReturnType<typeof setInterval> | null = null;
    private typingStopTimer: ReturnType<typeof setTimeout> | null = null;

    // Per-sender auto-expire timers on the RECEIVER side
    private typingExpireTimers = new Map<string, ReturnType<typeof setTimeout>>();



    readonly connected = signal(false);
    readonly users = signal<string[]>([]);
    readonly publicMessages = signal<ChatMessage[]>([]);
    readonly privateMessages = signal<ChatMessage[]>([]);
    readonly typingUsers = signal<Set<string>>(new Set());


    /** Unread counts keyed by thread ('public' or a username). */
    readonly unread = signal<Record<string, number>>({});

    readonly otherUsers = computed(() =>
        this.users().filter(u => u !== this.currentUser)
    );

    get me() { return this.currentUser; }



    async connect(user: string): Promise<void> {
        this.currentUser = user;

        this.hub = new signalR.HubConnectionBuilder()
            .withUrl('https://localhost:7297/hubs/chat')
            .withAutomaticReconnect()
            .build();

        this.hub.on('ReceiveMessage', (from, msg, ts) => {
            const mine = from === this.currentUser;
            this.publicMessages.update(list => [
                ...list,
                { user: from, message: msg, timestamp: new Date(ts), mine },
            ]);
            if (!mine) this.bumpUnread('public');
        });

        // IMPORTANT: server should NOT echo this back to caller anymore.
        // We optimistically append in sendPrivate(). Only handle incoming DMs here.
        this.hub.on('ReceivePrivate', (from, msg, ts) => {
            if (from === this.currentUser) return; // safety net if server still echoes
            this.privateMessages.update(list => [
                ...list,
                {
                    user: from,
                    message: msg,
                    timestamp: new Date(ts),
                    privateWith: from,
                    mine: false,
                },
            ]);
            this.bumpUnread(from);
        });

        this.hub.on('UserList', (users: string[]) => this.users.set([...users]));

        this.hub.on('UserTyping', (who: string) => {
            if (who === this.currentUser) return;
            this.typingUsers.update(s => new Set(s).add(who));

            // Failsafe: clear after 3s if no StoppedTyping arrives
            const existing = this.typingExpireTimers.get(who);
            if (existing) clearTimeout(existing);
            this.typingExpireTimers.set(who, setTimeout(() => {
                this.typingUsers.update(s => {
                    const next = new Set(s);
                    next.delete(who);
                    return next;
                });
                this.typingExpireTimers.delete(who);
            }, 3000));
        });

        this.hub.on('UserStoppedTyping', (who: string) => {
            this.typingUsers.update(s => {
                const next = new Set(s);
                next.delete(who);
                return next;
            });
            const t = this.typingExpireTimers.get(who);
            if (t) { clearTimeout(t); this.typingExpireTimers.delete(who); }
        });

        this.hub.onreconnecting(() => this.connected.set(false));
        this.hub.onreconnected(async () => {
            await this.hub.invoke('Register', this.currentUser);
            this.connected.set(true);
        });
        this.hub.onclose(() => this.connected.set(false));

        await this.hub.start();
        await this.hub.invoke('Register', user);
        this.connected.set(true);
    }



    sendPublic(message: string) {
        this.flushTyping(); // we're sending, so we're no longer typing
        this.publicMessages.update(list => [
            ...list,
            { user: this.currentUser, message, timestamp: new Date(), mine: true },
        ]);
        return this.hub.invoke('SendMessage', this.currentUser, message);
    }

    sendPrivate(toUser: string, message: string) {
        this.flushTyping();
        this.privateMessages.update(list => [
            ...list,
            {
                user: this.currentUser,
                message,
                timestamp: new Date(),
                privateWith: toUser,
                mine: true,
            },
        ]);
        return this.hub.invoke('SendPrivate', this.currentUser, toUser, message);
    }

    notifyTyping(toUser: string | null) {
        // If switching threads while typing, stop the old one first.
        if (this.isTyping && this.typingTarget !== toUser) {
            this.hub.invoke('StoppedTyping', this.currentUser, this.typingTarget);
            this.stopHeartbeat();
            this.isTyping = false;
        }

        if (!this.isTyping) {
            this.hub.invoke('Typing', this.currentUser, toUser);
            this.isTyping = true;
            this.typingTarget = toUser;

            // Heartbeat every 2s so a dropped packet can't strand the indicator
            this.typingHeartbeat = setInterval(() => {
                this.hub.invoke('Typing', this.currentUser, this.typingTarget);
            }, 2000);
        }

        // Reset the stop timer on every keystroke
        if (this.typingStopTimer) clearTimeout(this.typingStopTimer);
        this.typingStopTimer = setTimeout(() => this.flushTyping(), 2500);
    }

    flushTyping() {
        this.stopHeartbeat();
        if (this.typingStopTimer) {
            clearTimeout(this.typingStopTimer);
            this.typingStopTimer = null;
        }
        if (this.isTyping) {
            this.hub.invoke('StoppedTyping', this.currentUser, this.typingTarget);
            this.isTyping = false;
            this.typingTarget = null;
        }
    }

    private stopHeartbeat() {
        if (this.typingHeartbeat) {
            clearInterval(this.typingHeartbeat);
            this.typingHeartbeat = null;
        }
    }

    markRead(thread: string) {
        this.unread.update(u => {
            if (!u[thread]) return u;
            const { [thread]: _, ...rest } = u;
            return rest;
        });
    }

    private bumpUnread(thread: string) {
        this.unread.update(u =>
        ({
            ...u,
            [thread]: (u[thread] ?? 0) + 1
        }));
    }
}