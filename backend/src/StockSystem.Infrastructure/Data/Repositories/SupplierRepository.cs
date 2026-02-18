using Microsoft.EntityFrameworkCore;
using StockSystem.Domain.Entities;
using StockSystem.Domain.Interfaces;

namespace StockSystem.Infrastructure.Data.Repositories;

public class SupplierRepository : Repository<Supplier>, ISupplierRepository
{
    public SupplierRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Supplier?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower());
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        return await _dbSet.AnyAsync(s => 
            s.Name.ToLower() == name.ToLower() && 
            (!excludeId.HasValue || s.Id != excludeId.Value));
    }

    public override IQueryable<Supplier> Query()
    {
        return _dbSet.AsQueryable();
    }

    public IQueryable<Supplier> QueryWithProducts()
    {
        return _dbSet.Include(s => s.Products).AsQueryable();
    }
}
