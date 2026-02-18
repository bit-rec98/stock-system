using ClosedXML.Excel;
using StockSystem.Application.DTOs.Product;

namespace StockSystem.Infrastructure.Services;

public class ExcelExportService : IExcelExportService
{
    public byte[] ExportProductsToExcel(IEnumerable<ProductDto> products)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Inventory");

        // Headers
        var headers = new[] { "SKU", "Product Name", "Description", "Category", "Supplier", "Price", "Quantity", "Min Stock", "Status", "Created At" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        // Data
        int row = 2;
        foreach (var product in products)
        {
            worksheet.Cell(row, 1).Value = product.SKU;
            worksheet.Cell(row, 2).Value = product.Name;
            worksheet.Cell(row, 3).Value = product.Description;
            worksheet.Cell(row, 4).Value = product.CategoryName;
            worksheet.Cell(row, 5).Value = product.SupplierName ?? "N/A";
            worksheet.Cell(row, 6).Value = product.Price;
            worksheet.Cell(row, 6).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(row, 7).Value = product.Quantity;
            worksheet.Cell(row, 8).Value = product.MinStockLevel;
            worksheet.Cell(row, 9).Value = product.IsLowStock ? "Low Stock" : "In Stock";
            worksheet.Cell(row, 10).Value = product.CreatedAt.ToString("yyyy-MM-dd");

            // Highlight low stock rows
            if (product.IsLowStock)
            {
                worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.LightPink;
            }

            row++;
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        // Add summary
        worksheet.Cell(row + 2, 1).Value = "Summary";
        worksheet.Cell(row + 2, 1).Style.Font.Bold = true;
        
        var productList = products.ToList();
        worksheet.Cell(row + 3, 1).Value = "Total Products:";
        worksheet.Cell(row + 3, 2).Value = productList.Count;
        
        worksheet.Cell(row + 4, 1).Value = "Total Inventory Value:";
        worksheet.Cell(row + 4, 2).Value = productList.Sum(p => p.Price * p.Quantity);
        worksheet.Cell(row + 4, 2).Style.NumberFormat.Format = "$#,##0.00";
        
        worksheet.Cell(row + 5, 1).Value = "Low Stock Items:";
        worksheet.Cell(row + 5, 2).Value = productList.Count(p => p.IsLowStock);
        
        worksheet.Cell(row + 7, 1).Value = $"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm}";
        worksheet.Cell(row + 7, 1).Style.Font.Italic = true;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
