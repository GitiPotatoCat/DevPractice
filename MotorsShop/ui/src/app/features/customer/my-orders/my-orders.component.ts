// src/app/features/customer/my-orders/my-orders.component.ts
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { catchError, of } from 'rxjs';
import { OrderApi } from '../../../core/services/order.api';
import { Order } from '../../../core/models/order.model';
import { CurrencyPipe, DatePipe } from '@angular/common';

@Component({
    selector: 'app-my-orders',
    imports: [RouterLink, DatePipe, CurrencyPipe],
    templateUrl: './my-orders.component.html',
    styleUrl: './my-orders.component.scss',
})
export class MyOrdersComponent {
    private api = inject(OrderApi);

    orders = toSignal(
        this.api.getMine().pipe(catchError(() => of<Order[]>([]))),
        { initialValue: [] as Order[] }
    );

    expandedOrderId = signal<number | null>(null);

    toggle(id: number) {
        this.expandedOrderId.update(curr => (curr === id ? null : id));
    }
}