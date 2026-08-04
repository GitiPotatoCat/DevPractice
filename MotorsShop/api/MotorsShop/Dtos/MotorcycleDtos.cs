using System.ComponentModel.DataAnnotations;

namespace MotorsShop.Dtos;

public record MotorcycleReadDto(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int Year,
    int Stock,
    int EngineCc,
    string BrandName,
    string CategoryName);

public record MotorcycleCreateDto(
    [Required, StringLength(100, MinimumLength = 2)] string Name,
    [StringLength(1000)] string? Description,
    [Range(0.01, 1_000_000)] decimal Price,
    [Range(1900, 2100)] int Year,
    [Range(0, 10_000)] int Stock,
    [Range(50, 3000)] int EngineCc,
    [Range(1, int.MaxValue)] int BrandId,
    [Range(1, int.MaxValue)] int CategoryId);

public record MotorcycleUpdateDto(
    [Required, StringLength(100, MinimumLength = 2)] string Name,
    [StringLength(1000)] string? Description,
    [Range(0.01, 1_000_000)] decimal Price,
    [Range(1900, 2100)] int Year,
    [Range(0, 10_000)] int Stock,
    [Range(50, 3000)] int EngineCc,
    [Range(1, int.MaxValue)] int BrandId,
    [Range(1, int.MaxValue)] int CategoryId);

