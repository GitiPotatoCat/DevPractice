namespace FootballApp.Api.Entities;

/// <summary>
/// Maps 1:1 to the dbo.FootballClubs table created in Step 1.
/// This type is INTERNAL to the API; it must never leave the service layer.
/// Outbound responses use FootballClubDto (added in Step 4) instead.
/// </summary>
public class FootballClub
{
    public int Id { get; set; }

    public string ClubName { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public string League { get; set; } = string.Empty;

    public int FoundedYear { get; set; }

    public string Stadium { get; set; } = string.Empty;

    public int TitlesWon { get; set; }
}