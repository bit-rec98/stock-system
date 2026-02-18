namespace StockSystem.Application.DTOs.Product;

public record UpdateProductDto(
    string Name,
    string Description,
    decimal Price,
    int Quantity,
    int CategoryId,
    int? SupplierId,
    int MinStockLevel,
    bool IsActive
);
