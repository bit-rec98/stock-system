using Microsoft.EntityFrameworkCore;
using StockSystem.Application.Common;
using StockSystem.Application.DTOs.Supplier;
using StockSystem.Application.Interfaces;
using StockSystem.Application.Mappings;
using StockSystem.Domain.Entities;
using StockSystem.Domain.Interfaces;
using StockSystem.Infrastructure.Data;

namespace StockSystem.Infrastructure.Services;

public class SupplierService : ISupplierService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;

    public SupplierService(IUnitOfWork unitOfWork, ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<SupplierDto?> GetByIdAsync(int id)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.Products)
            .FirstOrDefaultAsync(s => s.Id == id);
        return supplier == null ? null : supplier.ToDto();
    }

    public async Task<IEnumerable<SupplierDto>> GetAllAsync()
    {
        var suppliers = await _context.Suppliers
            .Include(s => s.Products)
            .ToListAsync();
        return suppliers.ToDtos();
    }

    public async Task<PagedResult<SupplierDto>> GetPagedAsync(PaginationParams paginationParams)
    {
        var query = _context.Suppliers.Include(s => s.Products);
        
        var totalCount = await query.CountAsync();
        
        var items = await query
            .OrderBy(s => s.Name)
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        var dtos = items.ToDtos();
        
        return new PagedResult<SupplierDto>(dtos, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierDto dto)
    {
        if (await _unitOfWork.Suppliers.NameExistsAsync(dto.Name))
            throw new InvalidOperationException($"A supplier with name '{dto.Name}' already exists.");

        var supplier = dto.ToEntity();
        await _unitOfWork.Suppliers.AddAsync(supplier);
        await _unitOfWork.SaveChangesAsync();

        return supplier.ToDto();
    }

    public async Task<SupplierDto?> UpdateAsync(int id, UpdateSupplierDto dto)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
        if (supplier == null) return null;

        if (await _unitOfWork.Suppliers.NameExistsAsync(dto.Name, id))
            throw new InvalidOperationException($"A supplier with name '{dto.Name}' already exists.");

        dto.ApplyTo(supplier);
        await _unitOfWork.Suppliers.UpdateAsync(supplier);
        await _unitOfWork.SaveChangesAsync();

        return supplier.ToDto();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var supplier = await _unitOfWork.Suppliers.Query()
            .FirstOrDefaultAsync(s => s.Id == id);
        
        if (supplier == null) return false;

        if (supplier.Products.Any())
            throw new InvalidOperationException("Cannot delete supplier with associated products.");

        await _unitOfWork.Suppliers.DeleteAsync(supplier);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
