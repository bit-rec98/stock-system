using StockSystem.Application.Common;
using StockSystem.Application.DTOs.Product;

namespace StockSystem.Application.Interfaces;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(int id);
    Task<PagedResult<ProductDto>> GetAllAsync(ProductFilterParams filterParams);
    Task<IEnumerable<ProductDto>> GetLowStockProductsAsync();
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto);
    Task<bool> DeleteAsync(int id);
    Task<byte[]> ExportToPdfAsync(ProductFilterParams? filterParams = null);
    Task<byte[]> ExportToExcelAsync(ProductFilterParams? filterParams = null);
}
