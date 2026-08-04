// src/app/features/auth/register/register.component.ts
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { KeyValuePipe } from '@angular/common';

@Component({
    selector: 'app-register',
    imports: [ReactiveFormsModule, RouterLink, KeyValuePipe],
    templateUrl: './register.component.html',
    styleUrl: './register.component.scss',
})
export class RegisterComponent {
    private fb = inject(FormBuilder);
    private auth = inject(AuthService);
    private router = inject(Router);

    form = this.fb.nonNullable.group({
        fullName: ['', [Validators.required, Validators.minLength(2)]],
        email: ['', [Validators.required, Validators.email]],
        password: ['', [Validators.required, Validators.minLength(8)]],
    });

    isSubmitting = signal(false);
    errorMessage = signal<string | null>(null);
    fieldErrors = signal<Record<string, string[]>>({});

    onSubmit() {
        if (this.form.invalid || this.isSubmitting()) return;

        this.isSubmitting.set(true);
        this.errorMessage.set(null);
        this.fieldErrors.set({});

        this.auth.register(this.form.getRawValue()).subscribe({
            next: () => this.router.navigateByUrl('/'),
            error: err => {
                this.isSubmitting.set(false);
                if (err.error?.errors) {
                    this.fieldErrors.set(err.error.errors);
                } else {
                    this.errorMessage.set(err.error?.detail ?? 'Registration failed.');
                }
            },
        });
    }
}