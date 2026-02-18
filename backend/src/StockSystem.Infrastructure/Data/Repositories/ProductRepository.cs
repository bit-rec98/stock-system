using Microsoft.EntityFrameworkCore;
using StockSystem.Domain.Entities;
using StockSystem.Domain.Interfaces;

namespace StockSystem.Infrastructure.Data.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product?> GetBySkuAsync(string sku)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.SKU == sku);
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync()
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.Quantity <= p.MinStockLevel && p.IsActive)
            .ToListAsync();
    }

    public async Task<bool> SkuExistsAsync(string sku, int? excludeId = null)
    {
        return await _dbSet.AnyAsync(p => 
            p.SKU == sku && (!excludeId.HasValue || p.Id != excludeId.Value));
    }

    public override IQueryable<Product> Query()
    {
        return _dbSet
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .AsQueryable();
    }
}
