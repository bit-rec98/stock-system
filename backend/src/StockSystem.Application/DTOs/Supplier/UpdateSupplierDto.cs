namespace StockSystem.Application.DTOs.Supplier;

public record UpdateSupplierDto(
    string Name,
    string ContactName,
    string Email,
    string Phone,
    string Address,
    bool IsActive
);
