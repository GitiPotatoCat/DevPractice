// src/app/core/services/order.api.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Order, OrderCreate, OrderStatus } from '../models/order.model';

@Injectable({ providedIn: 'root' })
export class OrderApi {
    private http = inject(HttpClient);
    private base = `${environment.apiBaseUrl}/orders`;

    getMine(): Observable<Order[]> {
        return this.http.get<Order[]>(`${this.base}/mine`);
    }

    getById(id: number): Observable<Order> {
        return this.http.get<Order>(`${this.base}/${id}`);
    }

    create(dto: OrderCreate): Observable<Order> {
        return this.http.post<Order>(this.base, dto);
    }

    getAll(): Observable<Order[]> {
        return this.http.get<Order[]>(this.base);
    }

    updateStatus(id: number, status: OrderStatus): Observable<void> {
        return this.http.patch<void>(`${this.base}/${id}/status`, { status });
    }
}