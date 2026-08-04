// src/app/features/auth/login/login.component.ts
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
    selector: 'app-login',
    imports: [ReactiveFormsModule, RouterLink],
    templateUrl: './login.component.html',
    styleUrl: './login.component.scss',
})
export class LoginComponent {
    private fb = inject(FormBuilder);
    private auth = inject(AuthService);
    private router = inject(Router);
    private route = inject(ActivatedRoute);

    form = this.fb.nonNullable.group({
        email: ['', [Validators.required, Validators.email]],
        password: ['', [Validators.required]],
    });

    isSubmitting = signal(false);
    errorMessage = signal<string | null>(null);
    resetSuccess = signal(false);

    constructor() {
        if (this.route.snapshot.queryParamMap.get('resetSuccess') === '1') {
            this.resetSuccess.set(true);
        }
    }

    onSubmit() {
        if (this.form.invalid || this.isSubmitting()) return;

        this.isSubmitting.set(true);
        this.errorMessage.set(null);

        this.auth.login(this.form.getRawValue()).subscribe({
            next: () => {
                const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/';
                this.router.navigateByUrl(returnUrl);
            },
            error: err => {
                this.isSubmitting.set(false);
                this.errorMessage.set(
                    err.error?.detail ?? 'Invalid email or password.'
                );
            },
        });
    }
}