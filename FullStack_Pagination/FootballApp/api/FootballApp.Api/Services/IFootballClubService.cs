using FootballApp.Api.Dtos;
using FootballApp.Api.Models;

namespace FootballApp.Api.Services;

public interface IFootballClubService
{
    Task<PagedResult<FootballClubDto>> QueryAsync(
        PagedQueryRequest request,
        CancellationToken cancellationToken);
}