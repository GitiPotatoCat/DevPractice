using MotorsShop.Dtos;
using MotorsShop.Models;


namespace MotorsShop.Services;


public interface IOrderService
{
    Task<IEnumerable<OrderReadDto>> GetAllAsync();
    Task<OrderReadDto?> GetByIdAsync(int id);
    Task<(OrderReadDto? Order, bool Forbidden)> GetByIdForUserAsync(int id, string applicationUserId, bool isAdmin);
    Task<OrderReadDto> CreateAsync(string applicationUserId, OrderCreateDto dto);
    Task<bool> UpdateStatusAsync(int id, OrderStatus newStatus);
    Task<IEnumerable<OrderReadDto>> GetByUserAsync(string applicationUserId);
}