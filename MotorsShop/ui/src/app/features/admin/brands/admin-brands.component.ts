// src/app/features/admin/brands/admin-brands.component.ts
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { BrandApi } from '../../../core/services/brand.api';
import { Brand, BrandCreate } from '../../../core/models/brand.model';
import { Observable } from 'rxjs';

@Component({
    selector: 'app-admin-brands',
    imports: [ReactiveFormsModule],
    templateUrl: './admin-brands.component.html',
    styleUrl: './admin-brands.component.scss',
})
export class AdminBrandsComponent {
    private api = inject(BrandApi);
    private fb = inject(FormBuilder);

    brands = signal<Brand[]>([]);
    isLoading = signal(true);

    // Dialog state — null means closed, number means editing, 0 means creating
    editingId = signal<number | null>(null);
    isEditing = computed(() => this.editingId() !== null);
    isCreating = computed(() => this.editingId() === 0);

    isSubmitting = signal(false);
    errorMessage = signal<string | null>(null);
    successMessage = signal<string | null>(null);

    form = this.fb.nonNullable.group({
        name: ['', [Validators.required, Validators.minLength(2)]],
        country: ['', [Validators.required, Validators.minLength(2)]],
    });

    constructor() {
        this.load();
    }

    load() {
        this.isLoading.set(true);
        this.api.getAll().subscribe({
            next: items => {
                this.brands.set(items);
                this.isLoading.set(false);
            },
            error: () => {
                this.errorMessage.set('Could not load brands.');
                this.isLoading.set(false);
            },
        });
    }

    openCreate() {
        this.form.reset({ name: '', country: '' });
        this.editingId.set(0);
        this.errorMessage.set(null);
    }

    openEdit(brand: Brand) {
        this.form.setValue({ name: brand.name, country: brand.country });
        this.editingId.set(brand.id);
        this.errorMessage.set(null);
    }

    closeDialog() {
        this.editingId.set(null);
        this.errorMessage.set(null);
    }

    save() {
        if (this.form.invalid || this.isSubmitting()) return;
        const dto: BrandCreate = this.form.getRawValue();
        this.isSubmitting.set(true);
        this.errorMessage.set(null);

        const id = this.editingId();
        const request: Observable<unknown> = id && id > 0
            ? this.api.update(id, dto)
            : this.api.create(dto);

        request.subscribe({
            next: () => {
                this.isSubmitting.set(false);
                this.successMessage.set(id ? 'Brand updated.' : 'Brand created.');
                this.closeDialog();
                this.load();
            },
            error: (err: any) => {
                this.isSubmitting.set(false);
                this.errorMessage.set(err.error?.detail ?? 'Could not save brand.');
            },
        });

    }

    remove(brand: Brand) {
        if (!confirm(`Delete brand "${brand.name}"?`)) return;

        this.api.delete(brand.id).subscribe({
            next: () => {
                this.successMessage.set('Brand deleted.');
                this.load();
            },
            error: err => {
                this.errorMessage.set(
                    err.error?.detail ?? 'Could not delete brand. It may still be in use by motorcycles.'
                );
            },
        });
    }
}