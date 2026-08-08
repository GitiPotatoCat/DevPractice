// src/app/features/motorcycles/motorcycle-list/motorcycle-list.component.ts
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { catchError, debounceTime, of, switchMap } from 'rxjs';
import { toObservable } from '@angular/core/rxjs-interop';

import { MotorcycleApi } from '../../../core/services/motorcycle.api';
import { Motorcycle, MotorcycleQuery } from '../../../core/models/motorcycle.model';
import { PagedResult } from '../../../core/models/paged-result.model';
import { CurrencyPipe } from '@angular/common';

@Component({
  selector: 'app-motorcycle-list',
  imports: [FormsModule, RouterLink, CurrencyPipe],
  templateUrl: './motorcycle-list.component.html',
  styleUrl: './motorcycle-list.component.scss',
})
export class MotorcycleListComponent {
  private api = inject(MotorcycleApi);

  // Filter signals — bound to the form
  search = signal('');
  sortBy = signal('');
  sortOrder = signal<'asc' | 'desc'>('asc');
  page = signal(1);
  pageSize = signal(9);

  // Derived signal: the query object
  private query = computed<MotorcycleQuery>(() => ({
    search: this.search() || undefined,
    sortBy: this.sortBy() || undefined,
    sortOrder: this.sortOrder(),
    page: this.page(),
    pageSize: this.pageSize(),
  }));

  // Result signal — calls API whenever query changes, with debouncing for typing
  result = toSignal(
    toObservable(this.query).pipe(
      debounceTime(250),
      switchMap(q => this.api.getAll(q).pipe(
        catchError(() => of(this.emptyPage()))
      ))
    ),
    { initialValue: this.emptyPage() }
  );

  items = computed(() => this.result().items);
  totalPages = computed(() => this.result().totalPages);
  totalCount = computed(() => this.result().totalCount);

  // Reset to page 1 whenever a filter changes
  onSearchChange(value: string) {
    this.search.set(value);
    this.page.set(1);
  }
  onSortChange(value: string) {
    this.sortBy.set(value);
    this.page.set(1);
  }
  onOrderChange(value: 'asc' | 'desc') {
    this.sortOrder.set(value);
    this.page.set(1);
  }

  previousPage() { if (this.result().hasPrevious) this.page.set(this.page() - 1); }
  nextPage()     { if (this.result().hasNext)     this.page.set(this.page() + 1); }

  /** Deterministic gradient per bike so cards look distinct without images. */
  mediaGradient(id: number): string {
    const palettes: Array<[string, string]> = [
      ['#0f172a', '#334155'], // slate
      ['#7c2d12', '#dc2626'], // red-clay
      ['#1e3a8a', '#3b82f6'], // ocean
      ['#064e3b', '#10b981'], // forest
      ['#4c1d95', '#8b5cf6'], // violet
      ['#78350f', '#f59e0b'], // amber
      ['#831843', '#ec4899'], // rose
      ['#134e4a', '#14b8a6'], // teal
    ];
    const [a, b] = palettes[Math.abs(id) % palettes.length];
    return `linear-gradient(135deg, ${a} 0%, ${b} 100%)`;
  }

  private emptyPage(): PagedResult<Motorcycle> {
    return {
      items: [], page: 1, pageSize: 9,
      totalCount: 0, totalPages: 0,
      hasPrevious: false, hasNext: false
    };
  }
}