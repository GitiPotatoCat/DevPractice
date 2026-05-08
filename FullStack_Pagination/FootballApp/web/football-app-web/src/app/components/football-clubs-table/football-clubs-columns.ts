import { ColumnDef } from '@tanstack/table-core';
import { FootballClubDto } from '../../models';

/**
 * Column definitions for the football clubs table.
 *
 * The `id` of each column is the SAME string the backend allowlist accepts
 * (see SortMap in FootballClubService.cs). Keeping these in sync is what
 * lets a sort or filter on the client become a valid sort or filter on
 * the server with no translation step.
 *
 * `accessorKey` matches the property name on FootballClubDto.
 */
export const footballClubColumns: ColumnDef<FootballClubDto>[] = [
    {
        id: 'id',
        accessorKey: 'id',
        header: 'ID',
        enableSorting: true,
    },
    {
        id: 'clubName',
        accessorKey: 'clubName',
        header: 'Club',
        enableSorting: true,
    },
    {
        id: 'country',
        accessorKey: 'country',
        header: 'Country',
        enableSorting: true,
    },
    {
        id: 'league',
        accessorKey: 'league',
        header: 'League',
        enableSorting: true,
    },
    {
        id: 'foundedYear',
        accessorKey: 'foundedYear',
        header: 'Founded',
        enableSorting: true,
    },
    {
        id: 'stadium',
        accessorKey: 'stadium',
        header: 'Stadium',
        enableSorting: true,
    },
    {
        id: 'titlesWon',
        accessorKey: 'titlesWon',
        header: 'Titles',
        enableSorting: true,
    },
];