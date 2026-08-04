using Microsoft.EntityFrameworkCore;
using MotorsShop.Data;
using MotorsShop.Dtos;
using MotorsShop.Exceptions;
using MotorsShop.Models;

namespace MotorsShop.Services;

public class CustomerService : ICustomerService
{
    private readonly MotorsShopDbContext _db;
    public CustomerService(MotorsShopDbContext db) => _db = db;

    public async Task<IEnumerable<CustomerReadDto>> GetAllAsync() =>
        await _db.Customers
            .Select(c => new CustomerReadDto(c.Id, c.FullName, c.Email, c.Phone, c.Address, c.ApplicationUserId))
            .ToListAsync();

    public async Task<CustomerReadDto?> GetByIdAsync(int id)
    {
        var c = await _db.Customers.FindAsync(id);
        return c is null ? null
            : new CustomerReadDto(c.Id, c.FullName, c.Email, c.Phone, c.Address, c.ApplicationUserId);
    }

    public async Task<CustomerReadDto> CreateAsync(CustomerCreateDto dto)
    {
        if (await _db.Customers.AnyAsync(c => c.Email == dto.Email))
            throw new ConflictException("A customer with this email already exists.");

        var c = new Customer
        {
            FullName = dto.FullName, Email = dto.Email,
            Phone = dto.Phone, Address = dto.Address
        };
        _db.Customers.Add(c);
        await _db.SaveChangesAsync();
        return new CustomerReadDto(c.Id, c.FullName, c.Email, c.Phone, c.Address, c.ApplicationUserId);
    }

    public async Task UpdateAsync(int id, CustomerUpdateDto dto)
    {
        var c = await _db.Customers.FindAsync(id)
            ?? throw new NotFoundException("Customer", id);

        if (c.Email != dto.Email &&
            await _db.Customers.AnyAsync(x => x.Email == dto.Email))
            throw new ConflictException("A customer with this email already exists.");

        c.FullName = dto.FullName;
        c.Email = dto.Email;
        c.Phone = dto.Phone;
        c.Address = dto.Address;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var c = await _db.Customers.FindAsync(id)
            ?? throw new NotFoundException("Customer", id);
        _db.Customers.Remove(c);
        await _db.SaveChangesAsync();
    }

    public async Task<CustomerReadDto?> GetByApplicationUserIdAsync(string applicationUserId)
    {
        var c = await _db.Customers
            .FirstOrDefaultAsync(x => x.ApplicationUserId == applicationUserId);
        return c is null ? null
            : new CustomerReadDto(c.Id, c.FullName, c.Email, c.Phone, c.Address, c.ApplicationUserId);
    }

    public async Task UpdateByApplicationUserIdAsync(string applicationUserId, CustomerUpdateDto dto)
    {
        var c = await _db.Customers
            .FirstOrDefaultAsync(x => x.ApplicationUserId == applicationUserId)
            ?? throw new NotFoundException("No customer profile is linked to your account.");

        // duplicate-email check (same logic as UpdateAsync)
        if (c.Email != dto.Email && await _db.Customers.AnyAsync(x => x.Email == dto.Email))
            throw new ConflictException("A customer with this email already exists.");

        c.FullName = dto.FullName;
        c.Email = dto.Email;
        c.Phone = dto.Phone;
        c.Address = dto.Address;

        await _db.SaveChangesAsync();
    }

    public async Task PatchByApplicationUserIdAsync(string applicationUserId, CustomerPatchDto dto)
    {
        var c = await _db.Customers
            .FirstOrDefaultAsync(x => x.ApplicationUserId == applicationUserId)
            ?? throw new NotFoundException("No customer profile is linked to your account.");

        // Email change → check uniqueness
        if (dto.Email is not null && dto.Email != c.Email) {
            if (await _db.Customers.AnyAsync(x => x.Email == dto.Email))
                throw new ConflictException("A customer with this email already exists.");
            c.Email = dto.Email;
        }

        // Other fields — only update if provided
        if (dto.FullName is not null) c.FullName = dto.FullName;
        if (dto.Phone is not null) c.Phone = dto.Phone;
        if (dto.Address is not null) c.Address = dto.Address;

        await _db.SaveChangesAsync();
    }
}