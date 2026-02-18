using StockSystem.Domain.Interfaces;

namespace StockSystem.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IProductRepository? _products;
    private ICategoryRepository? _categories;
    private ISupplierRepository? _suppliers;

    public UnitOfWork(
        ApplicationDbContext context,
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ISupplierRepository supplierRepository)
    {
        _context = context;
        _products = productRepository;
        _categories = categoryRepository;
        _suppliers = supplierRepository;
    }

    public IProductRepository Products => _products!;
    public ICategoryRepository Categories => _categories!;
    public ISupplierRepository Suppliers => _suppliers!;

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
