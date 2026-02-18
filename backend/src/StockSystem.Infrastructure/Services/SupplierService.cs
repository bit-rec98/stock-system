using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StockSystem.Application.Common;
using StockSystem.Application.DTOs.Supplier;
using StockSystem.Application.Interfaces;
using StockSystem.Domain.Entities;
using StockSystem.Domain.Interfaces;
using StockSystem.Infrastructure.Data;

namespace StockSystem.Infrastructure.Services;

public class SupplierService : ISupplierService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _context;

    public SupplierService(IUnitOfWork unitOfWork, IMapper mapper, ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _context = context;
    }

    public async Task<SupplierDto?> GetByIdAsync(int id)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.Products)
            .FirstOrDefaultAsync(s => s.Id == id);
        return supplier == null ? null : _mapper.Map<SupplierDto>(supplier);
    }

    public async Task<IEnumerable<SupplierDto>> GetAllAsync()
    {
        var suppliers = await _context.Suppliers
            .Include(s => s.Products)
            .ToListAsync();
        return _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
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

        var dtos = _mapper.Map<IEnumerable<SupplierDto>>(items);
        
        return new PagedResult<SupplierDto>(dtos, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierDto dto)
    {
        if (await _unitOfWork.Suppliers.NameExistsAsync(dto.Name))
            throw new InvalidOperationException($"A supplier with name '{dto.Name}' already exists.");

        var supplier = _mapper.Map<Supplier>(dto);
        await _unitOfWork.Suppliers.AddAsync(supplier);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<SupplierDto>(supplier);
    }

    public async Task<SupplierDto?> UpdateAsync(int id, UpdateSupplierDto dto)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
        if (supplier == null) return null;

        if (await _unitOfWork.Suppliers.NameExistsAsync(dto.Name, id))
            throw new InvalidOperationException($"A supplier with name '{dto.Name}' already exists.");

        _mapper.Map(dto, supplier);
        await _unitOfWork.Suppliers.UpdateAsync(supplier);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<SupplierDto>(supplier);
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
