using FoodShop.DTOs;

namespace FoodShop.Services;

public interface IProductService
{
    Task<IEnumerable<ProductReadDto>> GetAllProductsAsync();
    Task<ProductReadDto> GetProductByIdAsync(int id);
    Task<ProductReadDto> CreateNewProductAsync(ProductCreateDto dto);
    Task<bool> UpdateProductAsync(int id, ProductUpdateDto dto);
    Task<bool> DeleteProductAsync(int id);
}
