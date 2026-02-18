namespace StockSystem.Application.DTOs.Product;

public record CreateProductDto(
    string Name,
    string Description,
    string SKU,
    decimal Price,
    int Quantity,
    int CategoryId,
    int? SupplierId,
    int MinStockLevel = 10
);
