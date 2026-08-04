using Microsoft.EntityFrameworkCore;
using MotorsShop.Data;
using MotorsShop.Dtos;
using MotorsShop.Exceptions;
using MotorsShop.Models;

namespace MotorsShop.Services;


public class OrderService : IOrderService
{
    private readonly MotorsShopDbContext _db;
    public OrderService(MotorsShopDbContext db) => _db = db;

    public async Task<IEnumerable<OrderReadDto>> GetAllAsync() =>
        await _db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items).ThenInclude(i => i.Motorcycle)
            .Select(o => Map(o))
            .ToListAsync();

    public async Task<OrderReadDto?> GetByIdAsync(int id)
    {
        var o = await _db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items).ThenInclude(i => i.Motorcycle)
            .FirstOrDefaultAsync(o => o.Id == id);
        return o is null ? null : Map(o);
    }

    public async Task<(OrderReadDto? Order, bool Forbidden)> GetByIdForUserAsync(
        int id, string applicationUserId, bool isAdmin)
    {
        var order = await _db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items).ThenInclude(i => i.Motorcycle)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return (null, false);

        // Admins bypass the ownership check
        if (!isAdmin) {
            if (order.Customer?.ApplicationUserId != applicationUserId)
                return (null, true);   // exists, but not yours
        }

        return (Map(order), false);
    }

    public async Task<OrderReadDto> CreateAsync(string applicationUserId, OrderCreateDto dto)
    {
        var customer = await _db.Customers
        .FirstOrDefaultAsync(c => c.ApplicationUserId == applicationUserId)
        ?? throw new NotFoundException("No customer profile is linked to your account.");


        var motoIds = dto.Items.Select(i => i.MotorcycleId).Distinct().ToList();
        var motos = await _db.Motorcycles
            .Where(m => motoIds.Contains(m.Id))
            .ToListAsync();

        if (motos.Count != motoIds.Count) {
            var missing = motoIds.Except(motos.Select(m => m.Id));
            throw new NotFoundException(
                $"Motorcycle(s) not found: {string.Join(", ", missing)}.");
        }

        foreach (var item in dto.Items) {
            var moto = motos.First(m => m.Id == item.MotorcycleId);
            if (moto.Stock < item.Quantity)
                throw new ConflictException(
                    $"Insufficient stock for {moto.Name} (have {moto.Stock}, need {item.Quantity}).");
        }

        await using var tx = await _db.Database.BeginTransactionAsync();

        var order = new Order
        {
            CustomerId = customer.Id,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            Items = new List<OrderItem>()
        };

        foreach (var item in dto.Items) {
            var moto = motos.First(m => m.Id == item.MotorcycleId);
            moto.Stock -= item.Quantity;
            order.Items.Add(new OrderItem
            {
                MotorcycleId = moto.Id,
                Quantity = item.Quantity,
                UnitPrice = moto.Price
            });
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return (await GetByIdAsync(order.Id))!;
    }

    public async Task<bool> UpdateStatusAsync(int id, OrderStatus newStatus)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order is null) return false;
        order.Status = newStatus;
        await _db.SaveChangesAsync();
        return true;
    }

    private static OrderReadDto Map(Order o) => new(
        o.Id, o.OrderDate, o.Status,
        o.CustomerId, o.Customer!.FullName,
        o.Items.Select(i => new OrderItemReadDto(
            i.MotorcycleId, i.Motorcycle!.Name, i.Quantity, i.UnitPrice,
            i.UnitPrice * i.Quantity)).ToList(),
        o.Items.Sum(i => i.UnitPrice * i.Quantity));


    public async Task<IEnumerable<OrderReadDto>> GetByUserAsync(string applicationUserId)
    {
        return await _db.Orders
            .Where(o => o.Customer!.ApplicationUserId == applicationUserId)
            .Include(o => o.Customer)
            .Include(o => o.Items).ThenInclude(i => i.Motorcycle)
            .Select(o => Map(o))
            .ToListAsync();
    }
}