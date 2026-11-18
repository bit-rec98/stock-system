using ClosedXML.Excel;
using Microsoft.Extensions.DependencyInjection;
using StockSystem.Domain.Entities;
using StockSystem.Infrastructure.Data;

namespace StockSystem.Tests;

public class ExportTests : IClassFixture<TestWebAppFactory>
{
    private readonly TestWebAppFactory _factory;

    public ExportTests(TestWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task Excel_export_contains_every_matching_product_not_just_one_page()
    {
        const int total = 150;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var category = new Category { Name = $"Export-{Guid.NewGuid():N}", Description = "d" };
            db.Categories.Add(category);
            await db.SaveChangesAsync();
            for (var i = 0; i < total; i++)
                db.Products.Add(new Product { Name = $"P{i}", Description = "d", SKU = $"EXP-{Guid.NewGuid():N}"[..20], Price = 1, Quantity = 1, CategoryId = category.Id });
            await db.SaveChangesAsync();
        }

        var admin = await _factory.LoginAsAdminAsync();
        var response = await admin.GetAsync("/api/products/export/excel");
        response.EnsureSuccessStatusCode();

        using var workbook = new XLWorkbook(await response.Content.ReadAsStreamAsync());
        var rows = workbook.Worksheet(1).RowsUsed().Count();
        Assert.True(rows >= total, $"expected at least {total} rows but found {rows}");
    }
}
