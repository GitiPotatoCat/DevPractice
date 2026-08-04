// src/app/features/admin/customers/admin-customers.component.ts
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { AdminCustomerApi } from '../../../core/services/admin-customer.api';
import { Customer, CustomerCreate } from '../../../core/models/customer.model';

@Component({
    selector: 'app-admin-customers',
    imports: [ReactiveFormsModule],
    templateUrl: './admin-customers.component.html',
    styleUrl: './admin-customers.component.scss',
})
export class AdminCustomersComponent {
    private api = inject(AdminCustomerApi);
    private fb = inject(FormBuilder);

    customers = signal<Customer[]>([]);
    isLoading = signal(true);

    editingId = signal<number | null>(null);
    isEditing = computed(() => this.editingId() !== null);
    isCreating = computed(() => this.editingId() === 0);

    isSubmitting = signal(false);
    errorMessage = signal<string | null>(null);
    successMessage = signal<string | null>(null);

    form = this.fb.nonNullable.group({
        fullName: ['', [Validators.required, Validators.minLength(2)]],
        email: ['', [Validators.required, Validators.email]],
        phone: [''],
        address: [''],
    });

    constructor() { this.load(); }

    load() {
        this.isLoading.set(true);
        this.api.getAll().subscribe({
            next: items => { this.customers.set(items); this.isLoading.set(false); },
            error: () => { this.errorMessage.set('Could not load customers.'); this.isLoading.set(false); },
        });
    }

    openCreate() {
        this.form.reset({ fullName: '', email: '', phone: '', address: '' });
        this.editingId.set(0);
        this.errorMessage.set(null);
    }

    openEdit(customer: Customer) {
        this.form.setValue({
            fullName: customer.fullName,
            email: customer.email,
            phone: customer.phone ?? '',
            address: customer.address ?? '',
        });
        this.editingId.set(customer.id);
        this.errorMessage.set(null);
    }

    closeDialog() {
        this.editingId.set(null);
        this.errorMessage.set(null);
    }

    save() {
        if (this.form.invalid || this.isSubmitting()) return;

        const values = this.form.getRawValue();
        const dto: CustomerCreate = {
            fullName: values.fullName,
            email: values.email,
            phone: values.phone || null,
            address: values.address || null,
        };

        this.isSubmitting.set(true);
        this.errorMessage.set(null);

        const id = this.editingId();
        const request: Observable<unknown> = id && id > 0
            ? this.api.update(id, dto)
            : this.api.create(dto);

        request.subscribe({
            next: () => {
                this.isSubmitting.set(false);
                this.successMessage.set(id ? 'Customer updated.' : 'Walk-in customer created.');
                this.closeDialog();
                this.load();
            },
            error: (err: any) => {
                this.isSubmitting.set(false);
                this.errorMessage.set(err.error?.detail ?? 'Could not save customer.');
            },
        });
    }

    remove(customer: Customer) {
        if (!confirm(`Delete customer "${customer.fullName}"?`)) return;
        this.api.delete(customer.id).subscribe({
            next: () => { this.successMessage.set('Customer deleted.'); this.load(); },
            error: (err: any) => {
                this.errorMessage.set(
                    err.error?.detail ?? 'Could not delete customer. They may have orders.'
                );
            },
        });
    }
}