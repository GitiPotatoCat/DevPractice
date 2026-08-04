using Microsoft.AspNetCore.Identity;

namespace MotorsShop.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }

    // Optional link back to the business profile
    public Customer? Customer { get; set; }
}