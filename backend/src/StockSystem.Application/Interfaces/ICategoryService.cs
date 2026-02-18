using StockSystem.Application.Common;
using StockSystem.Application.DTOs.Category;

namespace StockSystem.Application.Interfaces;

public interface ICategoryService
{
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<IEnumerable<CategoryDto>> GetAllAsync();
    Task<PagedResult<CategoryDto>> GetPagedAsync(PaginationParams paginationParams);
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
    Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto);
    Task<bool> DeleteAsync(int id);
}
