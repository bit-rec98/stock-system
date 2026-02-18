using StockSystem.Domain.Entities;

namespace StockSystem.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByIdWithDetailsAsync(int id);
    Task<Product?> GetBySkuAsync(string sku);
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
    Task<IEnumerable<Product>> GetLowStockProductsAsync();
    Task<bool> SkuExistsAsync(string sku, int? excludeId = null);
}
