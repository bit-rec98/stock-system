using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace StockSystem.Tests;

public static class TestHelpers
{
    public record LoginResponse(string Token);

    public static async Task<HttpClient> LoginAsync(this TestWebAppFactory factory, string email, string password)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<LoginResponse>(
            new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)))!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.Token);
        return client;
    }

    public static Task<HttpClient> LoginAsAdminAsync(this TestWebAppFactory factory) =>
        factory.LoginAsync(TestWebAppFactory.AdminEmail, TestWebAppFactory.AdminPassword);

    /// <summary>Creates a user through the admin-only endpoint and returns a client logged in as that user.</summary>
    public static async Task<HttpClient> CreateUserAndLoginAsync(
        this TestWebAppFactory factory, HttpClient admin, string role, string? email = null)
    {
        email ??= $"{role.ToLowerInvariant()}-{Guid.NewGuid():N}@test.local";
        const string password = "Passw0rd!x";
        var response = await admin.PostAsJsonAsync("/api/auth/register", new
        {
            email, password, firstName = "Test", lastName = role, role
        });
        response.EnsureSuccessStatusCode();
        return await factory.LoginAsync(email, password);
    }
}
