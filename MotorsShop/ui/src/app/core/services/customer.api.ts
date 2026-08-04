// src/app/core/services/customer.api.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Customer, CustomerPatch, CustomerUpdate } from '../models/customer.model';

@Injectable({ providedIn: 'root' })
export class CustomerApi {
    private http = inject(HttpClient);
    private base = `${environment.apiBaseUrl}/customers`;

    getMine(): Observable<Customer> {
        return this.http.get<Customer>(`${this.base}/me`);
    }

    updateMine(dto: CustomerUpdate): Observable<void> {
        return this.http.put<void>(`${this.base}/me`, dto);
    }

    patchMine(dto: CustomerPatch): Observable<void> {
        return this.http.patch<void>(`${this.base}/me`, dto);
    }
}