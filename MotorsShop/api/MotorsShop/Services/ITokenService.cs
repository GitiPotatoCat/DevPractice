using MotorsShop.Models;

namespace MotorsShop.Services;

public interface ITokenService
{
    Task<string> CreateTokenAsync(ApplicationUser user);
}