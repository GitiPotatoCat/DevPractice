// src/app/features/auth/forgot-password/forgot-password.component.ts
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
    selector: 'app-forgot-password',
    imports: [ReactiveFormsModule, RouterLink],
    templateUrl: './forgot-password.component.html',
    styleUrl: './forgot-password.component.scss',
})
export class ForgotPasswordComponent {
    private fb = inject(FormBuilder);
    private auth = inject(AuthService);

    form = this.fb.nonNullable.group({
        email: ['', [Validators.required, Validators.email]],
    });

    isSubmitting = signal(false);
    successMessage = signal<string | null>(null);
    errorMessage = signal<string | null>(null);

    submit() {
        if (this.form.invalid || this.isSubmitting()) return;

        this.isSubmitting.set(true);
        this.errorMessage.set(null);
        this.successMessage.set(null);

        this.auth.forgotPassword(this.form.getRawValue()).subscribe({
            next: () => {
                this.isSubmitting.set(false);
                this.successMessage.set(
                    'If an account exists with this email, a reset link has been sent. Check your inbox.'
                );
                this.form.reset();
            },
            error: () => {
                this.isSubmitting.set(false);
                this.errorMessage.set('Could not send reset email. Please try again.');
            },
        });
    }
}