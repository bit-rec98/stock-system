using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StockSystem.Application.Common;
using StockSystem.Application.DTOs.Category;
using StockSystem.Application.Interfaces;
using StockSystem.Domain.Entities;
using StockSystem.Domain.Interfaces;
using StockSystem.Infrastructure.Data;

namespace StockSystem.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _context;

    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper, ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _context = context;
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);
        return category == null ? null : _mapper.Map<CategoryDto>(category);
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await _context.Categories
            .Include(c => c.Products)
            .ToListAsync();
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
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

        var dtos = _mapper.Map<IEnumerable<CategoryDto>>(items);
        
        return new PagedResult<CategoryDto>(dtos, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        if (await _unitOfWork.Categories.NameExistsAsync(dto.Name))
            throw new InvalidOperationException($"A category with name '{dto.Name}' already exists.");

        var category = _mapper.Map<Category>(dto);
        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null) return null;

        if (await _unitOfWork.Categories.NameExistsAsync(dto.Name, id))
            throw new InvalidOperationException($"A category with name '{dto.Name}' already exists.");

        _mapper.Map(dto, category);
        await _unitOfWork.Categories.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CategoryDto>(category);
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
