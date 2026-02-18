using StockSystem.Domain.Entities;

namespace StockSystem.Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByNameAsync(string name);
    Task<bool> NameExistsAsync(string name, int? excludeId = null);
}
