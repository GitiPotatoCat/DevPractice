using System.ComponentModel.DataAnnotations;
using MotorsShop.Models;

namespace MotorsShop.Dtos;

public record OrderItemReadDto(
    int MotorcycleId, string MotorcycleName, int Quantity,
    decimal UnitPrice, decimal LineTotal);

public record OrderReadDto(
    int Id, DateTime OrderDate, OrderStatus Status,
    int CustomerId, string CustomerName,
    List<OrderItemReadDto> Items, decimal Total);

public record OrderItemCreateDto(
    [Range(1, int.MaxValue)] int MotorcycleId,
    [Range(1, 100)] int Quantity);

// OrderCreateDto — remove CustomerId
public record OrderCreateDto(
    [Required, MinLength(1)] 
    List<OrderItemCreateDto> Items);

public record OrderStatusUpdateDto([Required] OrderStatus Status);