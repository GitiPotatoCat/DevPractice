// src/app/core/services/category.api.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Category, CategoryCreate } from '../models/category.model';

@Injectable({ providedIn: 'root' })
export class CategoryApi {
    private http = inject(HttpClient);
    private base = `${environment.apiBaseUrl}/categories`;

    getAll(): Observable<Category[]> {
        return this.http.get<Category[]>(this.base);
    }
    getById(id: number): Observable<Category> {
        return this.http.get<Category>(`${this.base}/${id}`);
    }
    create(dto: CategoryCreate): Observable<Category> {
        return this.http.post<Category>(this.base, dto);
    }
    update(id: number, dto: CategoryCreate): Observable<void> {
        return this.http.put<void>(`${this.base}/${id}`, dto);
    }
    delete(id: number): Observable<void> {
        return this.http.delete<void>(`${this.base}/${id}`);
    }
}