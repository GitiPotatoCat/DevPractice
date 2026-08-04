// src/app/features/customer/place-order/place-order.component.ts
import { Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { catchError, of } from 'rxjs';
import { MotorcycleApi } from '../../../core/services/motorcycle.api';
import { OrderApi } from '../../../core/services/order.api';
import { Motorcycle } from '../../../core/models/motorcycle.model';
import { PagedResult } from '../../../core/models/paged-result.model';
import { CurrencyPipe } from '@angular/common';

@Component({
    selector: 'app-place-order',
    imports: [FormsModule, CurrencyPipe],
    templateUrl: './place-order.component.html',
    styleUrl: './place-order.component.scss',
})
export class PlaceOrderComponent {
    private bikeApi = inject(MotorcycleApi);
    private orderApi = inject(OrderApi);
    private router = inject(Router);

    // Fetch all available bikes (large pageSize for simplicity)
    bikes = toSignal<PagedResult<Motorcycle> | null>(
        this.bikeApi.getAll({ pageSize: 100 }).pipe(
            catchError(() => of<PagedResult<Motorcycle>>({
                items: [], page: 1, pageSize: 100,
                totalCount: 0, totalPages: 0,
                hasPrevious: false, hasNext: false,
            }))
        ),
        { initialValue: null }
    );

    // Map of motorcycleId -> quantity selected
    // Values are typed `number | undefined` so the `?? 0` fallback in the template is type-honest.
    quantities = signal<Record<number, number | undefined>>({});

    total = computed(() => {
        const qs = this.quantities();
        const list = this.bikes()?.items ?? [];
        let sum = 0;
        for (const bike of list) {
            const qty = qs[bike.id] ?? 0;
            sum += qty * bike.price;
        }
        return sum;
    });

    selectedCount = computed(() => {
        return Object.values(this.quantities()).filter(q => (q ?? 0) > 0).length;
    });

    isSubmitting = signal(false);
    errorMessage = signal<string | null>(null);

    setQuantity(bikeId: number, value: number, maxStock: number) {
        const clamped = Math.max(0, Math.min(value || 0, maxStock));
        this.quantities.update(qs => ({ ...qs, [bikeId]: clamped }));
    }

    submit() {
        if (this.selectedCount() === 0 || this.isSubmitting()) return;

        const items = Object.entries(this.quantities())
            .filter((entry): entry is [string, number] => (entry[1] ?? 0) > 0)
            .map(([id, qty]) => ({ motorcycleId: Number(id), quantity: qty }));

        this.isSubmitting.set(true);
        this.errorMessage.set(null);

        this.orderApi.create({ items }).subscribe({
            next: () => {
                this.isSubmitting.set(false);
                this.router.navigate(['/my-orders']);
            },
            error: err => {
                this.isSubmitting.set(false);
                this.errorMessage.set(err.error?.detail ?? 'Could not place order.');
            },
        });
    }
}