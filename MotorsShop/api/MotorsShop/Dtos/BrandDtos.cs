using System.ComponentModel.DataAnnotations;

namespace MotorsShop.Dtos;

public record BrandReadDto(int Id, string Name, string Country);
public record BrandCreateDto(
    [Required, StringLength(50, MinimumLength = 2)] string Name,
    [Required, StringLength(50)] string Country);
public record BrandUpdateDto(
    [Required, StringLength(50, MinimumLength = 2)] string Name,
    [Required, StringLength(50, MinimumLength = 2)] string Country);
