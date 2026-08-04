// src/app/features/admin/motorcycles/admin-motorcycles.component.ts
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CurrencyPipe } from '@angular/common';
import { forkJoin, Observable } from 'rxjs';
import { MotorcycleApi } from '../../../core/services/motorcycle.api';
import { BrandApi } from '../../../core/services/brand.api';
import { CategoryApi } from '../../../core/services/category.api';
import { Motorcycle, MotorcycleCreate } from '../../../core/models/motorcycle.model';
import { Brand } from '../../../core/models/brand.model';
import { Category } from '../../../core/models/category.model';

@Component({
    selector: 'app-admin-motorcycles',
    imports: [ReactiveFormsModule, CurrencyPipe],
    templateUrl: './admin-motorcycles.component.html',
    styleUrl: './admin-motorcycles.component.scss',
})
export class AdminMotorcyclesComponent {
    private api = inject(MotorcycleApi);
    private brandApi = inject(BrandApi);
    private categoryApi = inject(CategoryApi);
    private fb = inject(FormBuilder);

    motorcycles = signal<Motorcycle[]>([]);
    brands = signal<Brand[]>([]);
    categories = signal<Category[]>([]);
    isLoading = signal(true);

    editingId = signal<number | null>(null);
    isEditing = computed(() => this.editingId() !== null);
    isCreating = computed(() => this.editingId() === 0);

    isSubmitting = signal(false);
    errorMessage = signal<string | null>(null);
    successMessage = signal<string | null>(null);

    form = this.fb.nonNullable.group({
        name: ['', [Validators.required, Validators.minLength(2)]],
        description: [''],
        price: [0, [Validators.required, Validators.min(0.01)]],
        year: [new Date().getFullYear(), [Validators.required, Validators.min(1900), Validators.max(2100)]],
        stock: [0, [Validators.required, Validators.min(0)]],
        engineCc: [0, [Validators.required, Validators.min(50)]],
        brandId: [0, [Validators.required, Validators.min(1)]],
        categoryId: [0, [Validators.required, Validators.min(1)]],
    });

    constructor() {
        this.load();
        // Load brands and categories once (dropdown options)
        forkJoin({
            brands: this.brandApi.getAll(),
            categories: this.categoryApi.getAll(),
        }).subscribe(({ brands, categories }) => {
            this.brands.set(brands);
            this.categories.set(categories);
        });
    }

    load() {
        this.isLoading.set(true);
        this.api.getAll({ pageSize: 100 }).subscribe({
            next: page => {
                this.motorcycles.set(page.items);
                this.isLoading.set(false);
            },
            error: () => {
                this.errorMessage.set('Could not load motorcycles.');
                this.isLoading.set(false);
            },
        });
    }

    openCreate() {
        this.form.reset({
            name: '', description: '', price: 0,
            year: new Date().getFullYear(), stock: 0, engineCc: 0,
            brandId: 0, categoryId: 0,
        });
        this.editingId.set(0);
        this.errorMessage.set(null);
    }

    openEdit(bike: Motorcycle) {
        // The list DTO uses brandName/categoryName, not ids. Look up ids from the loaded lists.
        const brand = this.brands().find(b => b.name === bike.brandName);
        const category = this.categories().find(c => c.name === bike.categoryName);

        this.form.setValue({
            name: bike.name,
            description: bike.description ?? '',
            price: bike.price,
            year: bike.year,
            stock: bike.stock,
            engineCc: bike.engineCc,
            brandId: brand?.id ?? 0,
            categoryId: category?.id ?? 0,
        });
        this.editingId.set(bike.id);
        this.errorMessage.set(null);
    }

    closeDialog() {
        this.editingId.set(null);
        this.errorMessage.set(null);
    }

    save() {
        if (this.form.invalid || this.isSubmitting()) return;

        const values = this.form.getRawValue();
        const dto: MotorcycleCreate = {
            name: values.name,
            description: values.description || null,
            price: Number(values.price),
            year: Number(values.year),
            stock: Number(values.stock),
            engineCc: Number(values.engineCc),
            brandId: Number(values.brandId),
            categoryId: Number(values.categoryId),
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
                this.successMessage.set(id ? 'Motorcycle updated.' : 'Motorcycle created.');
                this.closeDialog();
                this.load();
            },
            error: (err: any) => {
                this.isSubmitting.set(false);
                this.errorMessage.set(err.error?.detail ?? 'Could not save motorcycle.');
            },
        });
    }

    remove(bike: Motorcycle) {
        if (!confirm(`Delete "${bike.name}"?`)) return;
        this.api.delete(bike.id).subscribe({
            next: () => { this.successMessage.set('Motorcycle deleted.'); this.load(); },
            error: (err: any) => {
                this.errorMessage.set(
                    err.error?.detail ?? 'Could not delete. It may still be referenced by orders.'
                );
            },
        });
    }
}