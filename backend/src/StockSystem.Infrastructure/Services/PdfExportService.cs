using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StockSystem.Application.DTOs.Product;

namespace StockSystem.Infrastructure.Services;

public class PdfExportService : IPdfExportService
{
    public byte[] ExportProductsToPdf(IEnumerable<ProductDto> products)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Text("Inventory Report")
                    .SemiBold()
                    .FontSize(20)
                    .FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(10)
                    .Table(table =>
                    {
                        // Define columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2); // SKU
                            columns.RelativeColumn(3); // Name
                            columns.RelativeColumn(2); // Category
                            columns.RelativeColumn(1.5f); // Price
                            columns.RelativeColumn(1); // Quantity
                            columns.RelativeColumn(1); // Status
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("SKU").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Product Name").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Category").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Price").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Qty").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Status").SemiBold();
                        });

                        // Data rows
                        foreach (var product in products)
                        {
                            var backgroundColor = product.IsLowStock ? Colors.Red.Lighten4 : Colors.White;
                            
                            table.Cell().Background(backgroundColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(product.SKU);
                            table.Cell().Background(backgroundColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(product.Name);
                            table.Cell().Background(backgroundColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(product.CategoryName);
                            table.Cell().Background(backgroundColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"${product.Price:F2}");
                            table.Cell().Background(backgroundColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(product.Quantity.ToString());
                            table.Cell().Background(backgroundColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                .Text(product.IsLowStock ? "Low Stock" : "In Stock")
                                .FontColor(product.IsLowStock ? Colors.Red.Medium : Colors.Green.Medium);
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Generated on ");
                        text.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                        text.Span(" | Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
            });
        });

        return document.GeneratePdf();
    }
}
