using StockSystem.Application.DTOs.Category;
using StockSystem.Application.DTOs.Product;
using StockSystem.Application.DTOs.Supplier;
using StockSystem.Domain.Entities;

namespace StockSystem.Application.Mappings;

/// <summary>
/// Explicit entity/DTO mapping. Replaces AutoMapper: the fixed versions for its known
/// advisories are only published on the commercially licensed 15.x+ line.
/// </summary>
public static class MappingExtensions
{
    // Product
    public static ProductDto ToDto(this Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        SKU = p.SKU,
        Price = p.Price,
        Quantity = p.Quantity,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.Name ?? string.Empty,
        SupplierId = p.SupplierId,
        SupplierName = p.Supplier?.Name,
        MinStockLevel = p.MinStockLevel,
        IsLowStock = p.Quantity <= p.MinStockLevel,
        IsActive = p.IsActive,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };

    public static IEnumerable<ProductDto> ToDtos(this IEnumerable<Product> items) =>
        items.Select(ToDto).ToList();

    public static Product ToEntity(this CreateProductDto dto) => new()
    {
        Name = dto.Name,
        Description = dto.Description,
        SKU = dto.SKU,
        Price = dto.Price,
        Quantity = dto.Quantity,
        CategoryId = dto.CategoryId,
        SupplierId = dto.SupplierId,
        MinStockLevel = dto.MinStockLevel
    };

    public static void ApplyTo(this UpdateProductDto dto, Product p)
    {
        p.Name = dto.Name;
        p.Description = dto.Description;
        p.Price = dto.Price;
        p.Quantity = dto.Quantity;
        p.CategoryId = dto.CategoryId;
        p.SupplierId = dto.SupplierId;
        p.MinStockLevel = dto.MinStockLevel;
        p.IsActive = dto.IsActive;
    }

    // Category
    public static CategoryDto ToDto(this Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Description = c.Description,
        IsActive = c.IsActive,
        ProductCount = c.Products.Count,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };

    public static IEnumerable<CategoryDto> ToDtos(this IEnumerable<Category> items) =>
        items.Select(ToDto).ToList();

    public static Category ToEntity(this CreateCategoryDto dto) => new()
    {
        Name = dto.Name,
        Description = dto.Description
    };

    public static void ApplyTo(this UpdateCategoryDto dto, Category c)
    {
        c.Name = dto.Name;
        c.Description = dto.Description;
        c.IsActive = dto.IsActive;
    }

    // Supplier
    public static SupplierDto ToDto(this Supplier s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        ContactName = s.ContactName,
        Email = s.Email,
        Phone = s.Phone,
        Address = s.Address,
        IsActive = s.IsActive,
        ProductCount = s.Products.Count,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };

    public static IEnumerable<SupplierDto> ToDtos(this IEnumerable<Supplier> items) =>
        items.Select(ToDto).ToList();

    public static Supplier ToEntity(this CreateSupplierDto dto) => new()
    {
        Name = dto.Name,
        ContactName = dto.ContactName,
        Email = dto.Email,
        Phone = dto.Phone,
        Address = dto.Address
    };

    public static void ApplyTo(this UpdateSupplierDto dto, Supplier s)
    {
        s.Name = dto.Name;
        s.ContactName = dto.ContactName;
        s.Email = dto.Email;
        s.Phone = dto.Phone;
        s.Address = dto.Address;
        s.IsActive = dto.IsActive;
    }
}
