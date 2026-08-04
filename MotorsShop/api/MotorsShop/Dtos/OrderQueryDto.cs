using MotorsShop.Models;
using System.ComponentModel.DataAnnotations;

namespace MotorsShop.Dtos;

public class OrderQueryDto
{
    [Range(1, int.MaxValue)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 20;

    public int? CustomerId { get; set; }
    public OrderStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SortBy { get; set; }        // "date" | "total"
    public string? SortOrder { get; set; }
}