// src/app/features/motorcycles/motorcycle-detail/motorcycle-detail.component.ts
import { Component, computed, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { catchError, of, switchMap } from 'rxjs';
import { MotorcycleApi } from '../../../core/services/motorcycle.api';
import { Motorcycle } from '../../../core/models/motorcycle.model';
import { CurrencyPipe } from '@angular/common';

@Component({
    selector: 'app-motorcycle-detail',
    imports: [RouterLink, CurrencyPipe],
    templateUrl: './motorcycle-detail.component.html',
    styleUrl: './motorcycle-detail.component.scss',
})
export class MotorcycleDetailComponent {
    private route = inject(ActivatedRoute);
    private api = inject(MotorcycleApi);

    // route.paramMap is an observable — flatMap to the actual fetch
    bike = toSignal(
        this.route.paramMap.pipe(
            switchMap(params => {
                const id = Number(params.get('id'));
                return this.api.getById(id).pipe(catchError(() => of(null)));
            })
        ),
        { initialValue: null as Motorcycle | null }
    );
}