using System.ComponentModel.DataAnnotations;

namespace MotorsShop.Dtos;

public record CustomerReadDto(
    int Id,
    string FullName,
    string Email,
    string? Phone,
    string? Address,
    string? ApplicationUserId);   // null = walk-in customer

public record CustomerCreateDto(
    [Required, StringLength(100, MinimumLength = 2)] string FullName,
    [Required, EmailAddress, StringLength(200)] string Email,
    [Phone, StringLength(30)] string? Phone,
    [StringLength(500)] string? Address);

public record CustomerUpdateDto(
    [Required, StringLength(100, MinimumLength = 2)] string FullName,
    [Required, EmailAddress, StringLength(200)] string Email,
    [Phone, StringLength(30)] string? Phone,
    [StringLength(500)] string? Address);

public record CustomerPatchDto(
    [StringLength(100, MinimumLength = 2)] string? FullName,
    [EmailAddress, StringLength(200)] string? Email,
    [Phone, StringLength(30)] string? Phone,
    [StringLength(500)] string? Address);