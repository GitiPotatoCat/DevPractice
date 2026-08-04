// src/app/core/services/admin-customer.api.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Customer, CustomerCreate, CustomerUpdate } from '../models/customer.model';

@Injectable({ providedIn: 'root' })
export class AdminCustomerApi {
    private http = inject(HttpClient);
    private base = `${environment.apiBaseUrl}/admin/customers`;

    getAll(): Observable<Customer[]> {
        return this.http.get<Customer[]>(this.base);
    }
    getById(id: number): Observable<Customer> {
        return this.http.get<Customer>(`${this.base}/${id}`);
    }
    create(dto: CustomerCreate): Observable<Customer> {
        return this.http.post<Customer>(this.base, dto);
    }
    update(id: number, dto: CustomerUpdate): Observable<void> {
        return this.http.put<void>(`${this.base}/${id}`, dto);
    }
    delete(id: number): Observable<void> {
        return this.http.delete<void>(`${this.base}/${id}`);
    }
}