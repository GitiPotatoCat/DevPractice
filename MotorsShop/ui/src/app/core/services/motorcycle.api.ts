// src/app/core/services/motorcycle.api.ts — add write methods
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Motorcycle, MotorcycleCreate, MotorcycleQuery } from '../models/motorcycle.model';
import { PagedResult } from '../models/paged-result.model';

@Injectable({ providedIn: 'root' })
export class MotorcycleApi {
    private http = inject(HttpClient);
    private base = `${environment.apiBaseUrl}/motorcycles`;

    getAll(query: MotorcycleQuery = {}): Observable<PagedResult<Motorcycle>> {
        let params = new HttpParams();
        for (const [key, value] of Object.entries(query)) {
            if (value !== undefined && value !== null && value !== '') {
                params = params.set(key, String(value));
            }
        }
        return this.http.get<PagedResult<Motorcycle>>(this.base, { params });
    }

    getById(id: number): Observable<Motorcycle> {
        return this.http.get<Motorcycle>(`${this.base}/${id}`);
    }

    create(dto: MotorcycleCreate): Observable<Motorcycle> {
        return this.http.post<Motorcycle>(this.base, dto);
    }

    update(id: number, dto: MotorcycleCreate): Observable<void> {
        return this.http.put<void>(`${this.base}/${id}`, dto);
    }

    delete(id: number): Observable<void> {
        return this.http.delete<void>(`${this.base}/${id}`);
    }
}