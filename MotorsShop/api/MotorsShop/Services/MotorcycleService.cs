using Microsoft.EntityFrameworkCore;
using MotorsShop.Data;
using MotorsShop.Dtos;
using MotorsShop.Exceptions;
using MotorsShop.Models;

namespace MotorsShop.Services;

public class MotorcycleService : IMotorcycleService
{
    private readonly MotorsShopDbContext _db;
    public MotorcycleService(MotorsShopDbContext db) => _db = db;

    public async Task<PagedResult<MotorcycleReadDto>> GetAllAsync(MotorcycleQueryDto q)
    {
        var query = _db.Motorcycles
            .Include(m => m.Brand)
            .Include(m => m.Category)
            .AsQueryable();

        // Filtering
        if (q.BrandId.HasValue)
            query = query.Where(m => m.BrandId == q.BrandId.Value);

        if (q.CategoryId.HasValue)
            query = query.Where(m => m.CategoryId == q.CategoryId.Value);

        if (q.MinPrice.HasValue)
            query = query.Where(m => m.Price >= q.MinPrice.Value);

        if (q.MaxPrice.HasValue)
            query = query.Where(m => m.Price <= q.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(q.Search)) {
            var term = q.Search.Trim().ToLower();
            query = query.Where(m =>
                m.Name.ToLower().Contains(term) ||
                (m.Description != null && m.Description.ToLower().Contains(term)));
        }

        // Sorting
        query = (q.SortBy?.ToLower(), q.SortOrder?.ToLower()) switch
        {
            ("price", "desc") => query.OrderByDescending(m => m.Price),
            ("price", _) => query.OrderBy(m => m.Price),
            ("year", "desc") => query.OrderByDescending(m => m.Year),
            ("year", _) => query.OrderBy(m => m.Year),
            ("name", "desc") => query.OrderByDescending(m => m.Name),
            ("name", _) => query.OrderBy(m => m.Name),
            _ => query.OrderBy(m => m.Id)   // default
        };

        // Count BEFORE pagination
        var totalCount = await query.CountAsync();

        // Pagination
        var items = await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(m => new MotorcycleReadDto(
                m.Id, m.Name, m.Description, m.Price, m.Year, m.Stock, m.EngineCc,
                m.Brand!.Name, m.Category!.Name))
            .ToListAsync();

        return new PagedResult<MotorcycleReadDto>(items, q.Page, q.PageSize, totalCount);
    }

    public async Task<MotorcycleReadDto?> GetByIdAsync(int id)
    {
        var m = await _db.Motorcycles
            .Include(x => x.Brand)
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id);

        return m is null ? null : new MotorcycleReadDto(
            m.Id, m.Name, m.Description, m.Price, m.Year, m.Stock, m.EngineCc,
            m.Brand!.Name, m.Category!.Name);
    }

    public async Task<MotorcycleReadDto?> CreateAsync(MotorcycleCreateDto dto)
    {
        // Validate foreign keys exist
        var brandExists = await _db.Brands.AnyAsync(b => b.Id == dto.BrandId);
        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!brandExists || !categoryExists) return null;

        var bike = new Motorcycle
        {
            Name = dto.Name, Description = dto.Description, Price = dto.Price,
            Year = dto.Year, Stock = dto.Stock, EngineCc = dto.EngineCc,
            BrandId = dto.BrandId, CategoryId = dto.CategoryId
        };
        _db.Motorcycles.Add(bike);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(bike.Id);
    }

    public async Task<bool> UpdateAsync(int id, MotorcycleUpdateDto dto)
    {
        var bike = await _db.Motorcycles.FindAsync(id);
        if (bike is null) return false;

        var brandExists = await _db.Brands.AnyAsync(b => b.Id == dto.BrandId);
        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!brandExists || !categoryExists) return false;

        bike.Name = dto.Name;
        bike.Description = dto.Description;
        bike.Price = dto.Price;
        bike.Year = dto.Year;
        bike.Stock = dto.Stock;
        bike.EngineCc = dto.EngineCc;
        bike.BrandId = dto.BrandId;
        bike.CategoryId = dto.CategoryId;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var bike = await _db.Motorcycles.FindAsync(id);
        if (bike is null) return false;
        _db.Motorcycles.Remove(bike);
        await _db.SaveChangesAsync();
        return true;
    }
}