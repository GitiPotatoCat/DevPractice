import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import {
    FootballClubDto,
    PagedQueryRequest,
    PagedResult,
} from '../models';

/**
 * Talks to the ASP.NET Core API for football clubs.
 *
 * Single responsibility: HTTP. No caching, no debouncing, no state.
 * Components and feature services compose on top of this.
 */
@Injectable({ providedIn: 'root' })
export class FootballClubsApiService {
    private readonly http = inject(HttpClient);
    private readonly baseUrl = `${environment.apiBaseUrl}/footballclubs`;

    /**
     * POST /api/footballclubs/query
     * Server-side paged + sorted + filtered query.
     */
    query(request: PagedQueryRequest): Observable<PagedResult<FootballClubDto>> {
        return this.http.post<PagedResult<FootballClubDto>>(
            `${this.baseUrl}/query`,
            request,
        );
    }
}