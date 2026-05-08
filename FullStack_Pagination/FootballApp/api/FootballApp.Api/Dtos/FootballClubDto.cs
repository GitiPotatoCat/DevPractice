namespace FootballApp.Api.Dtos;

/// <summary>
/// Outbound representation of a football club.
/// This is the public-facing shape returned by the API.
/// It is intentionally separate from the FootballClub entity so that
/// internal schema changes do not leak to the client.
/// </summary>
public sealed record FootballClubDto
(
    int Id,
    string ClubName,
    string Country,
    string League,
    int FoundedYear,
    string Stadium,
    int TitlesWon
);