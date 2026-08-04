import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { catchError, of, tap } from 'rxjs';
import { CustomerApi } from '../../../core/services/customer.api';
import { AuthService } from '../../../core/services/auth.service';
import { Customer, CustomerPatch } from '../../../core/models/customer.model';
import { KeyValuePipe } from '@angular/common';

@Component({
    selector: 'app-profile',
    imports: [ReactiveFormsModule, KeyValuePipe],
    templateUrl: './profile.component.html',
    styleUrl: './profile.component.scss',
})
export class ProfileComponent {
    private api = inject(CustomerApi);
    private auth = inject(AuthService);
    private fb = inject(FormBuilder);

    // ===== Existing profile state (unchanged) =====
    customer = toSignal(
        this.api.getMine().pipe(
            tap(c => this.populateForm(c)),
            catchError(() => of(null))
        ),
        { initialValue: null as Customer | null }
    );

    isEditing = signal(false);
    isSubmitting = signal(false);
    successMessage = signal<string | null>(null);
    errorMessage = signal<string | null>(null);

    form = this.fb.nonNullable.group({
        fullName: ['', [Validators.required, Validators.minLength(2)]],
        email: ['', [Validators.required, Validators.email]],
        phone: [''],
        address: [''],
    });

    // ===== NEW: change-password state =====
    passwordForm = this.fb.nonNullable.group({
        currentPassword: ['', [Validators.required]],
        newPassword: ['', [Validators.required, Validators.minLength(8)]],
    });

    isChangingPassword = signal(false);
    passwordSuccess = signal<string | null>(null);
    passwordError = signal<string | null>(null);
    passwordFieldErrors = signal<Record<string, string[]>>({});

    // ===== Existing profile methods (unchanged) =====
    startEdit() {
        this.isEditing.set(true);
        this.successMessage.set(null);
        this.errorMessage.set(null);
    }

    cancelEdit() {
        this.isEditing.set(false);
        const current = this.customer();
        if (current) this.populateForm(current);
    }

    save() {
        if (this.form.invalid || this.isSubmitting()) return;

        const original = this.customer();
        if (!original) return;

        const values = this.form.getRawValue();

        const patch: CustomerPatch = {};
        if (values.fullName !== original.fullName) patch.fullName = values.fullName;
        if (values.email !== original.email) patch.email = values.email;
        if (values.phone !== (original.phone ?? '')) patch.phone = values.phone || null;
        if (values.address !== (original.address ?? '')) patch.address = values.address || null;

        if (Object.keys(patch).length === 0) {
            this.isEditing.set(false);
            return;
        }

        this.isSubmitting.set(true);
        this.errorMessage.set(null);

        this.api.patchMine(patch).subscribe({
            next: () => {
                this.isSubmitting.set(false);
                this.isEditing.set(false);
                this.successMessage.set('Profile updated.');
                Object.assign(original, patch);
            },
            error: err => {
                this.isSubmitting.set(false);
                this.errorMessage.set(err.error?.detail ?? 'Could not update profile.');
            },
        });
    }

    // ===== NEW: change-password handler =====
    changePassword() {
        if (this.passwordForm.invalid || this.isChangingPassword()) return;

        this.isChangingPassword.set(true);
        this.passwordError.set(null);
        this.passwordSuccess.set(null);
        this.passwordFieldErrors.set({});

        this.auth.changePassword(this.passwordForm.getRawValue()).subscribe({
            next: () => {
                this.isChangingPassword.set(false);
                this.passwordSuccess.set('Password changed successfully.');
                this.passwordForm.reset({ currentPassword: '', newPassword: '' });
            },
            error: err => {
                this.isChangingPassword.set(false);
                if (err.error?.errors) {
                    this.passwordFieldErrors.set(err.error.errors);
                } else {
                    this.passwordError.set(err.error?.detail ?? 'Could not change password.');
                }
            },
        });
    }

    private populateForm(c: Customer) {
        this.form.setValue({
            fullName: c.fullName,
            email: c.email,
            phone: c.phone ?? '',
            address: c.address ?? '',
        });
    }
}