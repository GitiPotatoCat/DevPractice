namespace FoodShop.DTOs;

public record ProductReadDto(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string CategoryName);

public record ProductCreateDto(
    string Name,
    string Description,
    decimal Price,
    int Stock,
    int CategoryId);

public record ProductUpdateDto(
    string Name,
    string Description,
    decimal Price,
    int Stock,
    int CategoryId);
