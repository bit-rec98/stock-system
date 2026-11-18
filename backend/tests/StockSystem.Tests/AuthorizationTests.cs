using System.Net;
using System.Net.Http.Json;

namespace StockSystem.Tests;

public class AuthorizationTests : IClassFixture<TestWebAppFactory>
{
    private readonly TestWebAppFactory _factory;

    public AuthorizationTests(TestWebAppFactory factory) => _factory = factory;

    [Theory]
    [InlineData("/api/products")]
    [InlineData("/api/categories")]
    [InlineData("/api/suppliers")]
    public async Task Anonymous_requests_are_rejected(string url)
    {
        var response = await _factory.CreateClient().GetAsync(url);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_cannot_register_users()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/api/auth/register", new
        {
            email = "x@test.local", password = "Passw0rd!x", firstName = "A", lastName = "B", role = "Admin"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Staff_cannot_register_users()
    {
        var admin = await _factory.LoginAsAdminAsync();
        var staff = await _factory.CreateUserAndLoginAsync(admin, "Staff");

        var response = await staff.PostAsJsonAsync("/api/auth/register", new
        {
            email = "y@test.local", password = "Passw0rd!x", firstName = "A", lastName = "B", role = "Admin"
        });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_can_create_staff_and_admin_but_not_unknown_roles()
    {
        var admin = await _factory.LoginAsAdminAsync();

        foreach (var role in new[] { "Staff", "Admin" })
            await _factory.CreateUserAndLoginAsync(admin, role);

        var response = await admin.PostAsJsonAsync("/api/auth/register", new
        {
            email = "z@test.local", password = "Passw0rd!x", firstName = "A", lastName = "B", role = "SuperUser"
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Staff_can_read_but_cannot_change_structure_or_delete()
    {
        var admin = await _factory.LoginAsAdminAsync();
        var staff = await _factory.CreateUserAndLoginAsync(admin, "Staff");

        Assert.Equal(HttpStatusCode.OK, (await staff.GetAsync("/api/products")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await staff.GetAsync("/api/categories")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await staff.GetAsync("/api/suppliers")).StatusCode);

        var category = await staff.PostAsJsonAsync("/api/categories", new { name = "Nope", description = "x" });
        Assert.Equal(HttpStatusCode.Forbidden, category.StatusCode);

        var supplier = await staff.PostAsJsonAsync("/api/suppliers", new
        {
            name = "Nope", contactName = "x", email = "a@b.co", phone = "1", address = "x"
        });
        Assert.Equal(HttpStatusCode.Forbidden, supplier.StatusCode);

        Assert.Equal(HttpStatusCode.Forbidden, (await staff.DeleteAsync("/api/products/1")).StatusCode);
    }

    [Fact]
    public async Task Staff_can_create_and_update_products_but_only_admin_deletes()
    {
        var admin = await _factory.LoginAsAdminAsync();
        var staff = await _factory.CreateUserAndLoginAsync(admin, "Staff");

        var category = await admin.PostAsJsonAsync("/api/categories", new { name = $"Cat-{Guid.NewGuid():N}", description = "d" });
        Assert.Equal(HttpStatusCode.Created, category.StatusCode);
        var categoryId = (await category.Content.ReadFromJsonAsync<IdOnly>(Json))!.Id;

        var created = await staff.PostAsJsonAsync("/api/products", new
        {
            name = "Hammer", description = "d", sku = $"SKU-{Guid.NewGuid():N}"[..20], price = 9.5m,
            quantity = 3, categoryId, supplierId = (int?)null, minStockLevel = 1
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var productId = (await created.Content.ReadFromJsonAsync<IdOnly>(Json))!.Id;

        var updated = await staff.PutAsJsonAsync($"/api/products/{productId}", new
        {
            name = "Hammer 2", description = "d", price = 10m, quantity = 4, categoryId,
            supplierId = (int?)null, minStockLevel = 1, isActive = true
        });
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);

        Assert.Equal(HttpStatusCode.Forbidden, (await staff.DeleteAsync($"/api/products/{productId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/api/products/{productId}")).StatusCode);
    }

    private record IdOnly(int Id);
    private static readonly System.Text.Json.JsonSerializerOptions Json = new(System.Text.Json.JsonSerializerDefaults.Web);
}
