using Microsoft.EntityFrameworkCore;
using StockSystem.Domain.Entities;
using StockSystem.Domain.Interfaces;

namespace StockSystem.Infrastructure.Data.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        return await _dbSet.AnyAsync(c => 
            c.Name.ToLower() == name.ToLower() && 
            (!excludeId.HasValue || c.Id != excludeId.Value));
    }

    public override IQueryable<Category> Query()
    {
        return _dbSet.AsQueryable();
    }

    public IQueryable<Category> QueryWithProducts()
    {
        return _dbSet.Include(c => c.Products).AsQueryable();
    }
}
