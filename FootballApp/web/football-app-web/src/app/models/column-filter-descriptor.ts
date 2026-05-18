/**
 * One column filter. Mirrors the C# ColumnFilterDescriptor and
 * TanStack Table's ColumnFilter: { id: "country", value: "Spain" }
 *
 * `value` is `unknown` because the API accepts:
 *   - string         (text contains)
 *   - number         (numeric equality)
 *   - [number|null, number|null]  (numeric range)
 *
 * We use `unknown` instead of `any` to force callers to narrow before use.
 */
export interface ColumnFilterDescriptor {
    readonly id: string;
    readonly value: unknown;
}