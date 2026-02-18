namespace StockSystem.Application.DTOs.Supplier;

public record CreateSupplierDto(
    string Name,
    string ContactName,
    string Email,
    string Phone,
    string Address
);
