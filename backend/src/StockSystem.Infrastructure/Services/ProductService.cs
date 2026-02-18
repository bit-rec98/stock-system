using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StockSystem.Application.Common;
using StockSystem.Application.DTOs.Product;
using StockSystem.Application.Interfaces;
using StockSystem.Domain.Entities;
using StockSystem.Domain.Interfaces;

namespace StockSystem.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPdfExportService _pdfExportService;
    private readonly IExcelExportService _excelExportService;

    public ProductService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPdfExportService pdfExportService,
        IExcelExportService excelExportService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _pdfExportService = pdfExportService;
        _excelExportService = excelExportService;
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdWithDetailsAsync(id);
        return product == null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<PagedResult<ProductDto>> GetAllAsync(ProductFilterParams filterParams)
    {
        var query = _unitOfWork.Products.Query();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
        {
            var searchTerm = filterParams.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(searchTerm) ||
                p.SKU.ToLower().Contains(searchTerm) ||
                p.Description.ToLower().Contains(searchTerm));
        }

        if (filterParams.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filterParams.CategoryId.Value);

        if (filterParams.SupplierId.HasValue)
            query = query.Where(p => p.SupplierId == filterParams.SupplierId.Value);

        if (filterParams.MinPrice.HasValue)
            query = query.Where(p => p.Price >= filterParams.MinPrice.Value);

        if (filterParams.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= filterParams.MaxPrice.Value);

        if (filterParams.IsLowStock.HasValue && filterParams.IsLowStock.Value)
            query = query.Where(p => p.Quantity <= p.MinStockLevel);

        if (filterParams.IsActive.HasValue)
            query = query.Where(p => p.IsActive == filterParams.IsActive.Value);

        // Apply sorting
        query = filterParams.SortBy?.ToLower() switch
        {
            "name" => filterParams.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => filterParams.SortDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "quantity" => filterParams.SortDescending ? query.OrderByDescending(p => p.Quantity) : query.OrderBy(p => p.Quantity),
            "sku" => filterParams.SortDescending ? query.OrderByDescending(p => p.SKU) : query.OrderBy(p => p.SKU),
            "category" => filterParams.SortDescending ? query.OrderByDescending(p => p.Category.Name) : query.OrderBy(p => p.Category.Name),
            "createdat" => filterParams.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .ToListAsync();

        var dtos = _mapper.Map<IEnumerable<ProductDto>>(items);

        return new PagedResult<ProductDto>(dtos, totalCount, filterParams.PageNumber, filterParams.PageSize);
    }

    public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync()
    {
        var products = await _unitOfWork.Products.GetLowStockProductsAsync();
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        if (await _unitOfWork.Products.SkuExistsAsync(dto.SKU))
            throw new InvalidOperationException($"A product with SKU '{dto.SKU}' already exists.");

        var product = _mapper.Map<Product>(dto);
        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var createdProduct = await _unitOfWork.Products.GetByIdWithDetailsAsync(product.Id);
        return _mapper.Map<ProductDto>(createdProduct!);
    }

    public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdWithDetailsAsync(id);
        if (product == null) return null;

        _mapper.Map(dto, product);
        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var updatedProduct = await _unitOfWork.Products.GetByIdWithDetailsAsync(id);
        return _mapper.Map<ProductDto>(updatedProduct!);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null) return false;

        await _unitOfWork.Products.DeleteAsync(product);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<byte[]> ExportToPdfAsync(ProductFilterParams? filterParams = null)
    {
        var products = await GetFilteredProductsForExport(filterParams);
        return _pdfExportService.ExportProductsToPdf(products);
    }

    public async Task<byte[]> ExportToExcelAsync(ProductFilterParams? filterParams = null)
    {
        var products = await GetFilteredProductsForExport(filterParams);
        return _excelExportService.ExportProductsToExcel(products);
    }

    private async Task<IEnumerable<ProductDto>> GetFilteredProductsForExport(ProductFilterParams? filterParams)
    {
        filterParams ??= new ProductFilterParams { PageSize = int.MaxValue };
        filterParams.PageSize = int.MaxValue; // Get all matching records for export
        var result = await GetAllAsync(filterParams);
        return result.Items;
    }
}
