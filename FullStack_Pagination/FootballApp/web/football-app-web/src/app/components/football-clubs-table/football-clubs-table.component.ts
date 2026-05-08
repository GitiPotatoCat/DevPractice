import {
    ChangeDetectionStrategy,
    Component,
    computed,
    effect,
    inject,
    signal,
    DestroyRef,
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

import {
    ColumnDef,
    ColumnFiltersState,
    PaginationState,
    SortingState,
    Table,
    Updater,
    createTable,
    getCoreRowModel,
    getSortedRowModel,
} from '@tanstack/table-core';

import { FootballClubsApiService } from '../../services/football-clubs-api.service';
import { FootballClubDto, PagedQueryRequest } from '../../models';
import { footballClubColumns } from './football-clubs-columns';

@Component({
    selector: 'app-football-clubs-table',
    standalone: true,
    imports: [],
    templateUrl: './football-clubs-table.component.html',
    styleUrl: './football-clubs-table.component.css',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FootballClubsTableComponent {
    // ---- Dependencies -----------------------------------------------------
    private readonly api = inject(FootballClubsApiService);
    private readonly destroyRef = inject(DestroyRef);

    // ---- Server-state signals --------------------------------------------
    /** Rows returned by the server for the CURRENT page. Server-side mode: this is exactly one page. */
    protected readonly data = signal<FootballClubDto[]>([]);

    /** Total row count across ALL pages (server-reported). Drives pageCount. */
    protected readonly totalCount = signal<number>(0);

    /** True while an HTTP request is in flight. */
    protected readonly isLoading = signal<boolean>(false);

    /** Last error message from the API, or null. */
    protected readonly errorMessage = signal<string | null>(null);

    // ---- Table-state signals ---------------------------------------------
    /**
     * TanStack pagination state. We own it; TanStack notifies us of changes
     * via the onPaginationChange callback below, and we update this signal.
     */
    protected readonly pagination = signal<PaginationState>({
        pageIndex: 0,
        pageSize: 20,
    });

    /**
   * TanStack sorting state. Same shape as the backend's SortDescriptor[]
   * so we can pass it straight through with no translation.
   *
   * Multi-column: order matters. First entry is primary, second is
   * tiebreaker, and so on. TanStack manages this for us when the user
   * Shift+Clicks; we just store the result.
   */
    protected readonly sorting = signal<SortingState>([]);

    /**
   * Per-column filters. Same shape as the backend's ColumnFilterDescriptor[].
   * The `value` is `unknown` because columns of different types put
   * different shapes into it:
   *   - text columns:    string ("Spain")
   *   - numeric columns: [min|null, max|null] tuple ([1900, 1925])
   */
    protected readonly columnFilters = signal<ColumnFiltersState>([]);

    /**
     * Free-text search. The backend ORs it across clubName, country,
     * league, stadium (see ApplyGlobalFilter in FootballClubService.cs).
     */
    protected readonly globalFilter = signal<string>('');

    /** Live-typing value for the global search input. Debounced into globalFilter. */
    protected readonly globalFilterDraft = signal<string>('');


    /**
     * Per-column filter draft values shown in the UI BEFORE debounce.
     * Keyed by column id. The committed values flow into columnFilters
     * after the debounce window. Separating "what the user is typing"
     * from "what we send to the server" is what makes debouncing clean.
     */
    protected readonly columnFilterDrafts = signal<Record<string, unknown>>({});

    // ---- Column definitions (constant for now) ---------------------------
    private readonly columns: ColumnDef<FootballClubDto>[] = footballClubColumns;

    // ---- The Table instance ----------------------------------------------
    /**
     * Rebuild the table whenever any input signal changes. We intentionally
     * call createTable on every change instead of mutating an existing
     * instance - the engine is cheap, and full reconstruction makes state
     * transitions trivially correct.
     */
    protected readonly table = computed<Table<FootballClubDto>>(() => {
        return createTable<FootballClubDto>({
            data: this.data(),
            columns: this.columns,

            // Server-side mode: tell TanStack we handle paging AND sorting ourselves.
            manualPagination: true,
            manualSorting: true,
            pageCount: this.computedPageCount(),

            // Bind the table's internal state to our signals.
            state: {
                pagination: this.pagination(),
                sorting: this.sorting(),
                columnFilters: this.columnFilters(),
                globalFilter: this.globalFilter(),
                columnPinning: { left: [], right: [] },
            },

            // Required even in headless usage.
            getCoreRowModel: getCoreRowModel(),
            getSortedRowModel: getSortedRowModel(),

            // Updater pattern: TanStack passes either the new value OR a function.
            onPaginationChange: (updater: Updater<PaginationState>) => {
                const next =
                    typeof updater === 'function' ? updater(this.pagination()) : updater;
                this.pagination.set(next);
            },

            onSortingChange: (updater: Updater<SortingState>) => {
                const next =
                    typeof updater === 'function' ? updater(this.sorting()) : updater;
                this.sorting.set(next);

                // Sort change should reset to page 0 - the user's current page index
                // is meaningless under a different ordering.
                this.pagination.update((p) => ({ ...p, pageIndex: 0 }));
            },

            onColumnFiltersChange: (updater: Updater<ColumnFiltersState>) => {
                const next =
                    typeof updater === 'function' ? updater(this.columnFilters()) : updater;
                this.columnFilters.set(next);
                this.pagination.update((p) => ({ ...p, pageIndex: 0 }));
            },

            onGlobalFilterChange: (updater: Updater<string>) => {
                const next =
                    typeof updater === 'function' ? updater(this.globalFilter()) : updater;
                this.globalFilter.set(next);
                this.pagination.update((p) => ({ ...p, pageIndex: 0 }));
            },

            // Required by createTable signature; safe defaults that no-op.
            renderFallbackValue: null,
            onStateChange: () => { /* no-op */ },
        });
    });

    // ---- Derived signals -------------------------------------------------
    /** ceil(totalCount / pageSize) - clamped to >= 0. */
    private readonly computedPageCount = computed<number>(() => {
        const size = this.pagination().pageSize;
        if (size <= 0) return 0;
        return Math.ceil(this.totalCount() / size);
    });

    /**
     * The exact request object we send to the API. Pulled into its own
     * signal so the load-effect can depend on a single thing.
     */
    private readonly currentRequest = computed<PagedQueryRequest>(() => {
        const { pageIndex, pageSize } = this.pagination();
        const sort = this.sorting();
        const filters = this.columnFilters();
        const global = this.globalFilter().trim();

        return {
            pageIndex,
            pageSize,
            // Only include the sort field when there's something to sort by;
            // omitting it lets the backend apply its default Id-ascending order.
            ...(sort.length > 0
                ? { sort: sort.map((s) => ({ id: s.id, desc: s.desc })) }
                : {}),
            ...(filters.length > 0
                ? { filters: filters.map((f) => ({ id: f.id, value: f.value })) }
                : {}),
            ...(global.length > 0
                ? { globalFilter: global }
                : {}),
        };
    });

    // ---- Effects ---------------------------------------------------------
    constructor() {
        // Whenever the request changes, fetch. effect() re-runs on every signal
        // it reads. Reading currentRequest() inside is what makes it reactive.
        effect(() => {
            const request = this.currentRequest();
            this.fetchPage(request);
        });

        // NEW: debounce the column-filter drafts before committing them as
        // real columnFilters state. Without this, every keystroke fires HTTP.
        toObservable(this.columnFilterDrafts)
            .pipe(
                debounceTime(300),
                distinctUntilChanged(
                    (a, b) => JSON.stringify(a) === JSON.stringify(b),
                ),
                takeUntilDestroyed(this.destroyRef),
            )
            .subscribe((drafts) => {
                this.commitDrafts(drafts);
            });


        toObservable(this.globalFilterDraft)
            .pipe(
                debounceTime(300),
                distinctUntilChanged(),
                takeUntilDestroyed(this.destroyRef),
            )
            .subscribe((draft) => {
                const trimmed = draft.trim();
                if (trimmed === this.globalFilter()) return;
                this.globalFilter.set(trimmed);
                this.pagination.update((p) => ({ ...p, pageIndex: 0 }));
            });
    }

    /**
   * Convert the drafts record into a clean ColumnFiltersState that
   * matches what the backend expects. Drops empty strings, drops
   * empty tuples, drops anything we don't recognize.
   */
    private commitDrafts(drafts: Record<string, unknown>): void {
        const next: ColumnFiltersState = [];

        for (const [id, value] of Object.entries(drafts)) {
            // Text column: empty string = no filter.
            if (typeof value === 'string') {
                if (value.trim().length > 0) {
                    next.push({ id, value: value.trim() });
                }
                continue;
            }
            // Numeric range column: tuple [min, max], either bound nullable.
            if (Array.isArray(value) && value.length === 2) {
                const [min, max] = value;
                if (min !== null || max !== null) {
                    next.push({ id, value: [min, max] });
                }
                continue;
            }
            // Any other shape: ignored.
        }

        // Skip the update if nothing actually changed - keeps the effect
        // from re-firing for keystrokes that resolve to the same committed state.
        const before = this.columnFilters();
        if (JSON.stringify(before) === JSON.stringify(next)) return;

        this.columnFilters.set(next);
        this.pagination.update((p) => ({ ...p, pageIndex: 0 }));
    }

    // ---- HTTP -------------------------------------------------------------
    private fetchPage(request: PagedQueryRequest): void {
        this.isLoading.set(true);
        this.errorMessage.set(null);

        this.api
            .query(request)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
                next: (result) => {
                    this.data.set([...result.items]);
                    this.totalCount.set(result.totalCount);
                    this.isLoading.set(false);
                },
                error: (err) => {
                    console.error('Failed to load football clubs:', err);
                    // Intentionally keep `data` and `totalCount` from the previous
                    // successful request so the user doesn't lose context. The
                    // error banner makes the failure obvious; wiping the table
                    // would be redundant punishment.
                    this.isLoading.set(false);
                    this.errorMessage.set(
                        err?.status
                            ? `API error ${err.status}: ${err.statusText ?? 'Unknown error'}`
                            : 'Could not reach the API. Is the backend running?',
                    );
                },
            });
    }

    /** Re-runs the most recent request after an error. */
    retry(): void {
        this.fetchPage(this.currentRequest());
    }



    // ---- Pagination derived signals --------------------------------------
    protected readonly pageIndex = computed(() => this.pagination().pageIndex);
    protected readonly pageSize = computed(() => this.pagination().pageSize);

    /** Last valid 0-based page index (e.g. pageCount=10 -> lastPageIndex=9). */
    protected readonly lastPageIndex = computed(() => {
        const pc = this.computedPageCount();
        return pc > 0 ? pc - 1 : 0;
    });

    protected readonly canPrev = computed(() => this.pageIndex() > 0);
    protected readonly canNext = computed(() => this.pageIndex() < this.lastPageIndex());

    /** Human-friendly page number (1-based) for display. */
    protected readonly displayPageNumber = computed(() => this.pageIndex() + 1);

    /** Total page count for display. At least 1, even when there are no rows. */
    protected readonly displayPageCount = computed(() =>
        Math.max(1, this.computedPageCount()),
    );

    /** First/last row number on the current page (1-based, for the "Showing X-Y of Z" string). */
    protected readonly rangeStart = computed(() => {
        if (this.totalCount() === 0) return 0;
        return this.pageIndex() * this.pageSize() + 1;
    });

    protected readonly rangeEnd = computed(() => {
        const end = (this.pageIndex() + 1) * this.pageSize();
        return Math.min(end, this.totalCount());
    });

    // ---- Pagination commands ---------------------------------------------
    /** Available page sizes for the dropdown. Bound by the backend's [Range(1,200)] cap. */
    protected readonly pageSizeOptions = [10, 20, 50, 100] as const;

    goToFirstPage(): void {
        this.setPageIndex(0);
    }

    goToPreviousPage(): void {
        this.setPageIndex(this.pageIndex() - 1);
    }

    goToNextPage(): void {
        this.setPageIndex(this.pageIndex() + 1);
    }

    goToLastPage(): void {
        this.setPageIndex(this.lastPageIndex());
    }

    /**
     * Jump to a specific 0-based page index. Clamps out-of-range values
     * silently rather than erroring; UX > correctness theatre here.
     */
    setPageIndex(target: number): void {
        if (this.isLoading()) return; // ignore clicks while a request is in flight
        const clamped = Math.max(0, Math.min(target, this.lastPageIndex()));
        if (clamped === this.pageIndex()) return; // no-op: don't refetch the same page

        this.pagination.update((p) => ({ ...p, pageIndex: clamped }));
    }

    /**
     * Handle the "jump to page" input. Accepts a 1-based number from the UI
     * and converts to 0-based internally. Falls back to no-op on bad input.
     */
    onJumpToPage(rawValue: string): void {
        const n = Number(rawValue);
        if (!Number.isFinite(n) || n < 1) return;
        this.setPageIndex(Math.floor(n) - 1);
    }

    /**
     * Change page size. Resets to page 0 because the user's current
     * pageIndex may not exist at the new size.
     */
    setPageSize(rawValue: string): void {
        const next = Number(rawValue);
        if (!Number.isFinite(next) || next <= 0) return;

        this.pagination.update(() => ({
            pageIndex: 0,
            pageSize: next,
        }));
    }



    // ---- Template helpers ------------------------------------------------
    /**
     * Returns a plain string for a header, regardless of which TanStack
     * version is installed. v8 stores `header` as a string; v9 stores it
     * as a ColumnDefTemplate object. We handle both.
     */
    protected headerLabel(header: import('@tanstack/table-core').Header<FootballClubDto, unknown>): string {
        if (header.isPlaceholder) {
            return '';
        }
        const def = header.column.columnDef.header;
        if (def == null) {
            return String(header.column.id);
        }
        if (typeof def === 'string') {
            return def;
        }
        // v9+: header is a function/object. Fall back to the column id.
        if (typeof def === 'function') {
            try {
                const result = (def as (ctx: unknown) => unknown)(header.getContext());
                return result == null ? String(header.column.id) : String(result);
            } catch {
                return String(header.column.id);
            }
        }
        return String(header.column.id);
    }

    /** Same idea for cell values - safe stringification. */
    protected cellValue(cell: import('@tanstack/table-core').Cell<FootballClubDto, unknown>): string {
        const v = cell.getValue();
        return v == null ? '' : String(v);
    }


    // ---- Filter helpers --------------------------------------------------

    /** Column ids that take free-text "contains" input. Mirror of ApplyColumnFilters. */
    private readonly textFilterColumns = new Set([
        'clubName', 'country', 'league', 'stadium',
    ]);

    /** Column ids that take [min, max] numeric range. Mirror of ApplyColumnFilters. */
    private readonly numberFilterColumns = new Set([
        'id', 'foundedYear', 'titlesWon',
    ]);

    protected isTextFilter(columnId: string): boolean {
        return this.textFilterColumns.has(columnId);
    }

    protected isNumberFilter(columnId: string): boolean {
        return this.numberFilterColumns.has(columnId);
    }

    /** Read the current draft string for a text column. */
    protected getTextDraft(columnId: string): string {
        const v = this.columnFilterDrafts()[columnId];
        return typeof v === 'string' ? v : '';
    }

    /** Read the current draft tuple for a numeric range column. */
    protected getNumberDraft(columnId: string, bound: 'min' | 'max'): string {
        const v = this.columnFilterDrafts()[columnId];
        if (!Array.isArray(v) || v.length !== 2) return '';
        const part = bound === 'min' ? v[0] : v[1];
        return part === null || part === undefined ? '' : String(part);
    }

    /**
     * User typed in a text-column input. Update the draft signal only;
     * the debounce effect (next sub-step) will commit to columnFilters.
     */
    setTextDraft(columnId: string, raw: string): void {
        this.columnFilterDrafts.update((drafts) => ({
            ...drafts,
            [columnId]: raw,
        }));
    }

    /**
     * User typed in one half of a numeric-range input. We keep the OTHER
     * half intact and update only the side they edited.
     */
    setNumberDraft(columnId: string, bound: 'min' | 'max', raw: string): void {
        this.columnFilterDrafts.update((drafts) => {
            const existing = drafts[columnId];
            const tuple: [number | null, number | null] =
                Array.isArray(existing) && existing.length === 2
                    ? [
                        existing[0] as number | null,
                        existing[1] as number | null,
                    ]
                    : [null, null];

            const parsed =
                raw.trim() === '' ? null : Number.isFinite(Number(raw)) ? Number(raw) : null;

            if (bound === 'min') tuple[0] = parsed;
            else tuple[1] = parsed;

            return { ...drafts, [columnId]: tuple };
        });
    }

    /** Clears all column filters AND the global search. */
    clearAllFilters(): void {
        this.columnFilterDrafts.set({});
        this.globalFilterDraft.set('');
        
        this.columnFilters.set([]);
        this.globalFilter.set('');
        this.pagination.update((p) => ({ ...p, pageIndex: 0 }));
    }

    /** True if anything is filtered (used to enable the "Clear" button). */
    protected readonly hasAnyFilter = computed(() =>
        this.columnFilters().length > 0 || this.globalFilter().trim().length > 0,
    );

    /**
   * True when filters are active but the result is empty - so we can
   * show "No matches for your filters" instead of "Nothing in the DB".
   */
    protected readonly isEmptyDueToFilters = computed(() =>
        this.totalCount() === 0 && this.hasAnyFilter(),
    );

    /**
   * Returns the visual sort indicator for a column header.
   * - "" if not sorted
   * - "▲" if ascending
   * - "▼" if descending
   * - For multi-column sorts, append the priority number ("▲ 1", "▼ 2", ...)
   */
    protected sortIndicator(columnId: string): string {
        const sorting = this.sorting();
        const index = sorting.findIndex((s) => s.id === columnId);
        if (index === -1) return '';

        const arrow = sorting[index].desc ? '▼' : '▲';
        return sorting.length > 1 ? `${arrow} ${index + 1}` : arrow;
    }

    /**
     * Returns the value for the standard `aria-sort` attribute on a <th>.
     * Only the FIRST column in the multi-sort gets a value other than
     * "none" - the ARIA spec doesn't have a clean way to express secondary
     * sort, and announcing every column as "sorted" would be confusing.
     */
    protected ariaSortFor(columnId: string): 'ascending' | 'descending' | 'none' {
        const sorting = this.sorting();
        if (sorting.length === 0 || sorting[0].id !== columnId) return 'none';
        return sorting[0].desc ? 'descending' : 'ascending';
    }

    /** Live-typing value for the global search input. Debounced into globalFilter. */
    setGlobalFilterDraft(value: string): void {
        this.globalFilterDraft.set(value);
    }
}