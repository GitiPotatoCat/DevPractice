// src/app/core/services/auth.service.ts
import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    AuthResponse,
    LoginRequest,
    RegisterRequest,
    ChangePasswordRequest,
    ForgotPasswordRequest,
    ResetPasswordRequest
} from '../models/auth.model';

const TOKEN_KEY = 'motorsshop.token';
const USER_KEY = 'motorsshop.user';

@Injectable({ providedIn: 'root' })
export class AuthService {
    private http = inject(HttpClient);
    private router = inject(Router);

    // The auth state as a signal
    private _user = signal<AuthResponse | null>(this.loadFromStorage());

    // Read-only signals for templates and guards
    user = this._user.asReadonly();
    isAuthenticated = computed(() => this._user() !== null);
    isAdmin = computed(() => this._user()?.roles.includes('Admin') ?? false);

    login(req: LoginRequest): Observable<AuthResponse> {
        return this.http
            .post<AuthResponse>(`${environment.apiBaseUrl}/auth/login`, req)
            .pipe(tap(res => this.setSession(res)));
    }

    register(req: RegisterRequest): Observable<AuthResponse> {
        return this.http
            .post<AuthResponse>(`${environment.apiBaseUrl}/auth/register`, req)
            .pipe(tap(res => this.setSession(res)));
    }

    logout(): void {
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(USER_KEY);
        this._user.set(null);
        this.router.navigate(['/login']);
    }

    changePassword(req: ChangePasswordRequest): Observable<void> {
        return this.http.patch<void>(`${environment.apiBaseUrl}/auth/password`, req);
    }

    forgotPassword(req: ForgotPasswordRequest): Observable<void> {
        return this.http.post<void>(
            `${environment.apiBaseUrl}/auth/forgot-password`, req);
    }

    resetPassword(req: ResetPasswordRequest): Observable<void> {
        return this.http.post<void>(
            `${environment.apiBaseUrl}/auth/reset-password`, req);
    }

    getToken(): string | null {
        return localStorage.getItem(TOKEN_KEY);
    }

    private setSession(res: AuthResponse): void {
        localStorage.setItem(TOKEN_KEY, res.token);
        localStorage.setItem(USER_KEY, JSON.stringify(res));
        this._user.set(res);
    }

    private loadFromStorage(): AuthResponse | null {
        if (typeof window === 'undefined' || !window.localStorage) return null; // SSR safety
        const raw = localStorage.getItem(USER_KEY);
        return raw ? (JSON.parse(raw) as AuthResponse) : null;
    }
}