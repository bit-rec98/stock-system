using StockSystem.Domain.Entities;

namespace StockSystem.Domain.Interfaces;

public interface ISupplierRepository : IRepository<Supplier>
{
    Task<Supplier?> GetByNameAsync(string name);
    Task<bool> NameExistsAsync(string name, int? excludeId = null);
}
