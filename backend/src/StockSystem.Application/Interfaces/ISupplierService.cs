using StockSystem.Application.Common;
using StockSystem.Application.DTOs.Supplier;

namespace StockSystem.Application.Interfaces;

public interface ISupplierService
{
    Task<SupplierDto?> GetByIdAsync(int id);
    Task<IEnumerable<SupplierDto>> GetAllAsync();
    Task<PagedResult<SupplierDto>> GetPagedAsync(PaginationParams paginationParams);
    Task<SupplierDto> CreateAsync(CreateSupplierDto dto);
    Task<SupplierDto?> UpdateAsync(int id, UpdateSupplierDto dto);
    Task<bool> DeleteAsync(int id);
}
