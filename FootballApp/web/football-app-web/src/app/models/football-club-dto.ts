/**
 * Outbound representation of a football club.
 * Mirrors C# FootballClubDto exactly.
 */
export interface FootballClubDto {
    readonly id: number;
    readonly clubName: string;
    readonly country: string;
    readonly league: string;
    readonly foundedYear: number;
    readonly stadium: string;
    readonly titlesWon: number;
}