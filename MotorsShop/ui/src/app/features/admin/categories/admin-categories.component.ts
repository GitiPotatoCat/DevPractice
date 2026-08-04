// src/app/features/admin/categories/admin-categories.component.ts
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { CategoryApi } from '../../../core/services/category.api';
import { Category, CategoryCreate } from '../../../core/models/category.model';

@Component({
    selector: 'app-admin-categories',
    imports: [ReactiveFormsModule],
    templateUrl: './admin-categories.component.html',
    styleUrl: './admin-categories.component.scss',
})
export class AdminCategoriesComponent {
    private api = inject(CategoryApi);
    private fb = inject(FormBuilder);

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
    });

    constructor() { this.load(); }

    load() {
        this.isLoading.set(true);
        this.api.getAll().subscribe({
            next: items => { this.categories.set(items); this.isLoading.set(false); },
            error: () => { this.errorMessage.set('Could not load categories.'); this.isLoading.set(false); },
        });
    }

    openCreate() {
        this.form.reset({ name: '', description: '' });
        this.editingId.set(0);
        this.errorMessage.set(null);
    }

    openEdit(category: Category) {
        this.form.setValue({ name: category.name, description: category.description ?? '' });
        this.editingId.set(category.id);
        this.errorMessage.set(null);
    }

    closeDialog() {
        this.editingId.set(null);
        this.errorMessage.set(null);
    }

    save() {
        if (this.form.invalid || this.isSubmitting()) return;

        const values = this.form.getRawValue();
        const dto: CategoryCreate = {
            name: values.name,
            description: values.description || null,
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
                this.successMessage.set(id ? 'Category updated.' : 'Category created.');
                this.closeDialog();
                this.load();
            },
            error: (err: any) => {
                this.isSubmitting.set(false);
                this.errorMessage.set(err.error?.detail ?? 'Could not save category.');
            },
        });
    }

    remove(category: Category) {
        if (!confirm(`Delete category "${category.name}"?`)) return;
        this.api.delete(category.id).subscribe({
            next: () => { this.successMessage.set('Category deleted.'); this.load(); },
            error: (err: any) => {
                this.errorMessage.set(
                    err.error?.detail ?? 'Could not delete category. It may still be in use.'
                );
            },
        });
    }
}