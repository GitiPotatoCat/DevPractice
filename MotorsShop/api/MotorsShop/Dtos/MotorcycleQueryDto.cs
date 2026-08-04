using System.ComponentModel.DataAnnotations;

namespace MotorsShop.Dtos;

public class MotorcycleQueryDto
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    public int? BrandId { get; set; }
    public int? CategoryId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MinPrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MaxPrice { get; set; }

    public string? Search { get; set; }

    public string? SortBy { get; set; }         // "name" | "price" | "year"
    public string? SortOrder { get; set; }      // "asc" | "desc"
}
