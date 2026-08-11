import {
    AfterViewChecked, Component, ElementRef, ViewChild,
    computed, effect, inject, signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { ChatService } from '../services/chat.service';

@Component({
    selector: 'app-chat',
    standalone: true,
    imports: [FormsModule, DatePipe],
    templateUrl: './chat.component.html',
    styleUrl: './chat.component.scss',
})
export class ChatComponent implements AfterViewChecked {
    readonly chat = inject(ChatService);
    readonly joined = signal(false);
    readonly dmTarget = signal<string | null>(null);
    user = '';
    text = '';

    @ViewChild('scroller') scroller?: ElementRef<HTMLElement>;
    private shouldScroll = false;

    readonly visibleMessages = computed(() => {
        const target = this.dmTarget();
        if (target === null) return this.chat.publicMessages();
        return this.chat.privateMessages().filter(m => m.privateWith === target);
    });

    readonly typingLabel = computed(() => {
        const target = this.dmTarget();
        const all = [...this.chat.typingUsers()];
        const relevant = target ? all.filter(u => u === target) : all;
        if (relevant.length === 0) return null;
        if (relevant.length === 1) return `${relevant[0]} is writing`;
        if (relevant.length === 2) return `${relevant[0]} and ${relevant[1]} are writing`;
        return `${relevant.length} people are writing`;
    });

    constructor() {
        // Autoscroll whenever the active thread gets new messages
        effect(() => {
            this.visibleMessages();
            this.shouldScroll = true;
        });

        // Clear unread on the thread we're currently viewing
        effect(() => {
            const target = this.dmTarget();
            const thread = target ?? 'public';
            // touch unread so the effect re-runs when counts change
            this.chat.unread();
            this.chat.markRead(thread);
        });
    }

    ngAfterViewChecked() {
        if (this.shouldScroll && this.scroller) {
            this.scroller.nativeElement.scrollTop = this.scroller.nativeElement.scrollHeight;
            this.shouldScroll = false;
        }
    }

    unreadFor(thread: string): number {
        return this.chat.unread()[thread] ?? 0;
    }

    async join(e: Event) {
        e.preventDefault();
        if (!this.user.trim()) return;
        await this.chat.connect(this.user.trim());
        this.joined.set(true);
    }

    async send(e: Event) {
        e.preventDefault();
        const msg = this.text.trim();
        if (!msg) return;

        const target = this.dmTarget();
        this.text = '';              // ← clear FIRST so UI feels instant
        this.chat.flushTyping();     // also stop "typing" indicator immediately

        try {
            if (target) await this.chat.sendPrivate(target, msg);
            else await this.chat.sendPublic(msg);
        } catch (err) {
            console.error('Send failed:', err);
            this.text = msg;           // restore so the user can retry
        }
    }

    onBlur() { this.chat.flushTyping(); }

    onInput() {
        if (this.text.trim()) {
            this.chat.notifyTyping(this.dmTarget());
        } else {
            this.chat.flushTyping();
        }
    }
}