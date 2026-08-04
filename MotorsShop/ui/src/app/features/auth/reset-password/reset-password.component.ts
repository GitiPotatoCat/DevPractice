// src/app/features/auth/reset-password/reset-password.component.ts
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { KeyValuePipe } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';

@Component({
    selector: 'app-reset-password',
    imports: [ReactiveFormsModule, RouterLink, KeyValuePipe],
    templateUrl: './reset-password.component.html',
    styleUrl: './reset-password.component.scss',
})
export class ResetPasswordComponent implements OnInit {
    private fb = inject(FormBuilder);
    private auth = inject(AuthService);
    private route = inject(ActivatedRoute);
    private router = inject(Router);

    form = this.fb.nonNullable.group({
        email: ['', [Validators.required, Validators.email]],
        token: ['', [Validators.required]],
        newPassword: ['', [Validators.required, Validators.minLength(8)]],
    });

    isSubmitting = signal(false);
    errorMessage = signal<string | null>(null);
    fieldErrors = signal<Record<string, string[]>>({});

    ngOnInit() {
        // Prefill from query params if the user arrived via the email link
        const params = this.route.snapshot.queryParamMap;
        const email = params.get('email');
        const token = params.get('token');

        if (email || token) {
            this.form.patchValue({
                email: email ?? '',
                token: token ?? '',
            });
        }
    }

    submit() {
        if (this.form.invalid || this.isSubmitting()) return;

        this.isSubmitting.set(true);
        this.errorMessage.set(null);
        this.fieldErrors.set({});

        this.auth.resetPassword(this.form.getRawValue()).subscribe({
            next: () => {
                this.router.navigate(['/login'], {
                    queryParams: { resetSuccess: '1' },
                });
            },
            error: err => {
                this.isSubmitting.set(false);
                if (err.error?.errors) {
                    this.fieldErrors.set(err.error.errors);
                } else {
                    this.errorMessage.set(err.error?.detail ?? 'Could not reset password.');
                }
            },
        });
    }
}