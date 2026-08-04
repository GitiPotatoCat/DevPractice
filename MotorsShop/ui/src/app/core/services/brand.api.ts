// src/app/core/services/brand.api.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Brand, BrandCreate } from '../models/brand.model';

@Injectable({ providedIn: 'root' })
export class BrandApi {
    private http = inject(HttpClient);
    private base = `${environment.apiBaseUrl}/brands`;

    getAll(): Observable<Brand[]> {
        return this.http.get<Brand[]>(this.base);
    }
    getById(id: number): Observable<Brand> {
        return this.http.get<Brand>(`${this.base}/${id}`);
    }
    create(dto: BrandCreate): Observable<Brand> {
        return this.http.post<Brand>(this.base, dto);
    }
    update(id: number, dto: BrandCreate): Observable<void> {
        return this.http.put<void>(`${this.base}/${id}`, dto);
    }
    delete(id: number): Observable<void> {
        return this.http.delete<void>(`${this.base}/${id}`);
    }
}