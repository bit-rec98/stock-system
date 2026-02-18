using StockSystem.Application.DTOs.Product;

namespace StockSystem.Infrastructure.Services;

public interface IPdfExportService
{
    byte[] ExportProductsToPdf(IEnumerable<ProductDto> products);
}
