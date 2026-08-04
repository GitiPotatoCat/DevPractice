using MotorsShop.Dtos;

namespace MotorsShop.Services;

public interface IMotorcycleService
{
    Task<PagedResult<MotorcycleReadDto>> GetAllAsync(MotorcycleQueryDto query);
    Task<MotorcycleReadDto?> GetByIdAsync(int id);
    Task<MotorcycleReadDto?> CreateAsync(MotorcycleCreateDto dto);
    Task<bool> UpdateAsync(int id, MotorcycleUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}