using System.Net;
using System.Net.Http.Json;

namespace StockSystem.Tests;

public class AuthenticationTests : IClassFixture<TestWebAppFactory>
{
    private readonly TestWebAppFactory _factory;

    public AuthenticationTests(TestWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task Invalid_login_payload_is_rejected_by_server_side_validation()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/api/auth/login", new { email = "", password = "" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Weak_passwords_are_rejected_when_registering()
    {
        var admin = await _factory.LoginAsAdminAsync();
        var response = await admin.PostAsJsonAsync("/api/auth/register", new
        {
            email = "weak@test.local", password = "short", firstName = "A", lastName = "B", role = "Staff"
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Account_locks_after_repeated_failures_even_with_the_right_password()
    {
        var admin = await _factory.LoginAsAdminAsync();
        var email = $"lock-{Guid.NewGuid():N}@test.local";
        await _factory.CreateUserAndLoginAsync(admin, "Staff", email);

        var client = _factory.CreateClient();
        for (var i = 0; i < 5; i++)
        {
            var bad = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Wrong-pass1" });
            Assert.Equal(HttpStatusCode.Unauthorized, bad.StatusCode);
        }

        var good = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Passw0rd!x" });
        Assert.Equal(HttpStatusCode.Unauthorized, good.StatusCode);
    }

    [Fact]
    public async Task Tampered_token_is_rejected()
    {
        var admin = await _factory.LoginAsAdminAsync();
        var token = admin.DefaultRequestHeaders.Authorization!.Parameter!;
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token[..^3] + "abc");

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/products")).StatusCode);
    }

    [Fact]
    public async Task Health_endpoint_is_public_and_security_headers_are_set()
    {
        var response = await _factory.CreateClient().GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
    }

    [Fact]
    public async Task Change_password_works_for_the_current_user()
    {
        var admin = await _factory.LoginAsAdminAsync();
        var email = $"pw-{Guid.NewGuid():N}@test.local";
        var user = await _factory.CreateUserAndLoginAsync(admin, "Staff", email);

        var response = await user.PostAsJsonAsync("/api/auth/change-password", new
        {
            currentPassword = "Passw0rd!x", newPassword = "N3wPassword!", confirmNewPassword = "N3wPassword!"
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await _factory.LoginAsync(email, "N3wPassword!");
    }
}
