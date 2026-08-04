// src/app/features/admin/orders/admin-orders.component.ts
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { OrderApi } from '../../../core/services/order.api';
import { Order, OrderStatus } from '../../../core/models/order.model';

@Component({
    selector: 'app-admin-orders',
    imports: [FormsModule, CurrencyPipe, DatePipe],
    templateUrl: './admin-orders.component.html',
    styleUrl: './admin-orders.component.scss',
})
export class AdminOrdersComponent {
    private api = inject(OrderApi);

    orders = signal<Order[]>([]);
    isLoading = signal(true);
    errorMessage = signal<string | null>(null);
    successMessage = signal<string | null>(null);

    expandedId = signal<number | null>(null);
    // Track which order is currently being updated to disable its dropdown
    updatingId = signal<number | null>(null);

    statuses: OrderStatus[] = ['Pending', 'Paid', 'Shipped', 'Delivered', 'Cancelled'];

    constructor() { this.load(); }

    load() {
        this.isLoading.set(true);
        this.api.getAll().subscribe({
            next: items => { this.orders.set(items); this.isLoading.set(false); },
            error: () => { this.errorMessage.set('Could not load orders.'); this.isLoading.set(false); },
        });
    }

    toggle(id: number) {
        this.expandedId.update(curr => curr === id ? null : id);
    }

    changeStatus(order: Order, newStatus: OrderStatus) {
        if (newStatus === order.status || this.updatingId() !== null) return;

        this.updatingId.set(order.id);
        this.errorMessage.set(null);

        this.api.updateStatus(order.id, newStatus).subscribe({
            next: () => {
                this.updatingId.set(null);
                // Optimistic update
                const updated = this.orders().map(o =>
                    o.id === order.id ? { ...o, status: newStatus } : o
                );
                this.orders.set(updated);
                this.successMessage.set(`Order #${order.id} updated to ${newStatus}.`);
            },
            error: (err: any) => {
                this.updatingId.set(null);
                this.errorMessage.set(err.error?.detail ?? 'Could not update status.');
            },
        });
    }
}