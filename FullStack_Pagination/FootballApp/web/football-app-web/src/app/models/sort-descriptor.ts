/**
 * One sort instruction. Mirrors the C# SortDescriptor and TanStack Table's
 * ColumnSort: { id: "clubName", desc: false }
 */
export interface SortDescriptor {
    readonly id: string;
    readonly desc: boolean;
}