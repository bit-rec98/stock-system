using Microsoft.EntityFrameworkCore;
using StockSystem.Application.Common;
using StockSystem.Application.DTOs.Category;
using StockSystem.Application.Interfaces;
using StockSystem.Application.Mappings;
using StockSystem.Domain.Entities;
using StockSystem.Domain.Interfaces;
using StockSystem.Infrastructure.Data;

namespace StockSystem.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;

    public CategoryService(IUnitOfWork unitOfWork, ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);
        return category == null ? null : category.ToDto();
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await _context.Categories
            .Include(c => c.Products)
            .ToListAsync();
        return categories.ToDtos();
    }

    public async Task<PagedResult<CategoryDto>> GetPagedAsync(PaginationParams paginationParams)
    {
        var query = _context.Categories.Include(c => c.Products);
        
        var totalCount = await query.CountAsync();
        
        var items = await query
            .OrderBy(c => c.Name)
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        var dtos = items.ToDtos();
        
        return new PagedResult<CategoryDto>(dtos, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        if (await _unitOfWork.Categories.NameExistsAsync(dto.Name))
            throw new InvalidOperationException($"A category with name '{dto.Name}' already exists.");

        var category = dto.ToEntity();
        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return category.ToDto();
    }

    public async Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null) return null;

        if (await _unitOfWork.Categories.NameExistsAsync(dto.Name, id))
            throw new InvalidOperationException($"A category with name '{dto.Name}' already exists.");

        dto.ApplyTo(category);
        await _unitOfWork.Categories.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return category.ToDto();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _unitOfWork.Categories.Query()
            .FirstOrDefaultAsync(c => c.Id == id);
        
        if (category == null) return false;

        if (category.Products.Any())
            throw new InvalidOperationException("Cannot delete category with associated products.");

        await _unitOfWork.Categories.DeleteAsync(category);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
