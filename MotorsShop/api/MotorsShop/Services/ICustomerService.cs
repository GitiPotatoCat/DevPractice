using MotorsShop.Dtos;

namespace MotorsShop.Services;

public interface ICustomerService
{
    Task<IEnumerable<CustomerReadDto>> GetAllAsync();
    Task<CustomerReadDto?> GetByIdAsync(int id);
    Task<CustomerReadDto> CreateAsync(CustomerCreateDto dto);
    Task UpdateAsync(int id, CustomerUpdateDto dto);
    Task DeleteAsync(int id);

    Task<CustomerReadDto?> GetByApplicationUserIdAsync(string applicationUserId);
    Task UpdateByApplicationUserIdAsync(string applicationUserId, CustomerUpdateDto dto);
    Task PatchByApplicationUserIdAsync(string applicationUserId, CustomerPatchDto dto);
}