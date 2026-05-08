import { SortDescriptor } from './sort-descriptor';
import { ColumnFilterDescriptor } from './column-filter-descriptor';

/**
 * Inbound request body for the paged list endpoint.
 * Field names match the C# PagedQueryRequest 1:1 (camelCase on the wire).
 */
export interface PagedQueryRequest {
    readonly pageIndex: number;
    readonly pageSize: number;
    readonly sort?: ReadonlyArray<SortDescriptor>;
    readonly filters?: ReadonlyArray<ColumnFilterDescriptor>;
    readonly globalFilter?: string;
}