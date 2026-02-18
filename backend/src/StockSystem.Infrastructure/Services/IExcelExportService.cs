using StockSystem.Application.DTOs.Product;

namespace StockSystem.Infrastructure.Services;

public interface IExcelExportService
{
    byte[] ExportProductsToExcel(IEnumerable<ProductDto> products);
}
