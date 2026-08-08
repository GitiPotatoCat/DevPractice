import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { catchError, of, tap } from 'rxjs';
import { CustomerApi } from '../../../core/services/customer.api';
import { OrderApi } from '../../../core/services/order.api';
import { AuthService } from '../../../core/services/auth.service';
import { Customer, CustomerPatch } from '../../../core/models/customer.model';
import { Order } from '../../../core/models/order.model';
import { CurrencyPipe, DatePipe, KeyValuePipe } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
    selector: 'app-profile',
    imports: [ReactiveFormsModule, KeyValuePipe, CurrencyPipe, DatePipe, RouterLink],
    templateUrl: './profile.component.html',
    styleUrl: './profile.component.scss',
})
export class ProfileComponent {
    private api = inject(CustomerApi);
    private orders = inject(OrderApi);
    private auth = inject(AuthService);
    private fb = inject(FormBuilder);

    // ===== Profile state =====
    customer = toSignal(
        this.api.getMine().pipe(
            tap(c => this.populateForm(c)),
            catchError(() => of(null))
        ),
        { initialValue: null as Customer | null }
    );

    // ===== Recent orders (for the stats + preview card) =====
    private orderList = toSignal(
        this.orders.getMine().pipe(catchError(() => of([] as Order[]))),
        { initialValue: [] as Order[] }
    );

    orderCount = computed(() => this.orderList().length);

    totalSpend = computed(() =>
        this.orderList()
            .filter(o => o.status !== 'Cancelled')
            .reduce((sum, o) => sum + o.total, 0)
    );

    lastOrder = computed(() => {
        const list = this.orderList();
        if (!list.length) return null;
        // Most-recent by orderDate; API may return any order.
        return [...list].sort(
            (a, b) => new Date(b.orderDate).getTime() - new Date(a.orderDate).getTime()
        )[0];
    });

    recentOrders = computed(() => {
        const list = this.orderList();
        return [...list]
            .sort((a, b) => new Date(b.orderDate).getTime() - new Date(a.orderDate).getTime())
            .slice(0, 3);
    });

    isEditing = signal(false);
    isSubmitting = signal(false);
    successMessage = signal<string | null>(null);
    errorMessage = signal<string | null>(null);

    /** First letter of the customer's full name, uppercased. */
    initial = computed(() => {
        const name = this.customer()?.fullName?.trim() ?? '';
        return name ? name.charAt(0).toUpperCase() : '?';
    });

    /** Roles from the auth session, for the hero badges. */
    roles = computed(() => this.auth.user()?.roles ?? []);

    /** Was the email copied to the clipboard just now? Auto-resets. */
    emailCopied = signal(false);

    form = this.fb.nonNullable.group({
        fullName: ['', [Validators.required, Validators.minLength(2)]],
        email: ['', [Validators.required, Validators.email]],
        phone: [''],
        address: [''],
    });

    // ===== Change-password state =====
    passwordForm = this.fb.nonNullable.group({
        currentPassword: ['', [Validators.required]],
        newPassword: ['', [Validators.required, Validators.minLength(8)]],
    });

    isChangingPassword = signal(false);
    passwordSuccess = signal<string | null>(null);
    passwordError = signal<string | null>(null);
    passwordFieldErrors = signal<Record<string, string[]>>({});

    /**
     * Rough password-strength score (0–4) based on length and character variety.
     * Purely a UX hint — the server owns the real policy.
     */
    passwordStrength = computed(() => {
        const pw = this.passwordForm.controls.newPassword.value ?? '';
        if (!pw) return { score: 0, label: '', className: '' };

        let score = 0;
        if (pw.length >= 8) score++;
        if (pw.length >= 12) score++;
        if (/[A-Z]/.test(pw) && /[a-z]/.test(pw)) score++;
        if (/\d/.test(pw)) score++;
        if (/[^A-Za-z0-9]/.test(pw)) score++;
        score = Math.min(score, 4);

        const labels = ['Very weak', 'Weak', 'Okay', 'Strong', 'Very strong'];
        const classes = ['weak', 'weak', 'okay', 'strong', 'strong'];
        return { score, label: labels[score], className: classes[score] };
    });

    // ===== Profile methods =====
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

    // ===== Change-password handler =====
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

    /** Copy the email to the clipboard; falls back to a no-op on unsupported envs. */
    copyEmail(): void {
        const email = this.customer()?.email;
        if (!email || typeof navigator === 'undefined' || !navigator.clipboard) return;

        navigator.clipboard.writeText(email).then(() => {
            this.emailCopied.set(true);
            setTimeout(() => this.emailCopied.set(false), 1800);
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
