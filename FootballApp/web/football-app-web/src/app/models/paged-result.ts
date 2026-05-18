/**
 * Generic paged response wrapper. Mirrors C# PagedResult<T>.
 *
 * pageCount is server-computed (ceil(totalCount / pageSize)) so the client
 * never has to do that math itself. TanStack Table consumes it directly.
 */
export interface PagedResult<T> {
    readonly items: ReadonlyArray<T>;
    readonly totalCount: number;
    readonly pageIndex: number;
    readonly pageSize: number;
    readonly pageCount: number;
}