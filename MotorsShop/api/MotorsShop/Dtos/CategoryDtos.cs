using System.ComponentModel.DataAnnotations;

namespace MotorsShop.Dtos;

public record CategoryReadDto(int Id, string Name, string? Description);

public record CategoryCreateDto(
    [Required, StringLength(50, MinimumLength = 2)] string Name,
    [StringLength(500)] string? Description);

public record CategoryUpdateDto(
    [Required, StringLength(50, MinimumLength = 2)] string Name,
    [StringLength(500)] string? Description);
