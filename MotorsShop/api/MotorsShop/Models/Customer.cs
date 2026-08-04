using System.ComponentModel.DataAnnotations;

namespace MotorsShop.Models;

public class Customer
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Phone, MaxLength(30)]
    public string? Phone { get; set; }

    public string? Address { get; set; }


    // Optional link to a login. Nullable because admin can create walk-in customers.
    public string? ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }


    public List<Order> Orders { get; set; } = new();
}