using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace StockSystem.Tests;

public class UserManagementTests : IClassFixture<TestWebAppFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly TestWebAppFactory _factory;

    public UserManagementTests(TestWebAppFactory factory) => _factory = factory;

    private record UserItem(string Id, string Email, string FirstName, string LastName, string? Role, bool IsActive, bool IsLockedOut);
    private record UserPage(List<UserItem> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages);

    [Fact]
    public async Task Anonymous_cannot_list_users()
    {
        var response = await _factory.CreateClient().GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Staff_cannot_list_users()
    {
        var admin = await _factory.LoginAsAdminAsync();
        var staff = await _factory.CreateUserAndLoginAsync(admin, "Staff");

        Assert.Equal(HttpStatusCode.Forbidden, (await staff.GetAsync("/api/users")).StatusCode);
    }

    [Fact]
    public async Task Admin_lists_users_with_roles_paging_and_search_and_no_credentials()
    {
        var admin = await _factory.LoginAsAdminAsync();
        var marker = Guid.NewGuid().ToString("N")[..8];
        for (var i = 0; i < 3; i++)
            await _factory.CreateUserAndLoginAsync(admin, "Staff", $"list-{marker}-{i}@test.local");

        var response = await admin.GetAsync($"/api/users?searchTerm=list-{marker}&pageSize=2&pageNumber=1");
        response.EnsureSuccessStatusCode();
        var raw = await response.Content.ReadAsStringAsync();
        var page = JsonSerializer.Deserialize<UserPage>(raw, Json)!;

        Assert.Equal(3, page.TotalCount);
        Assert.Equal(2, page.Items.Count);
        Assert.Equal(2, page.TotalPages);
        Assert.All(page.Items, u => Assert.Equal("Staff", u.Role));

        var second = await admin.GetFromJsonAsync<UserPage>($"/api/users?searchTerm=list-{marker}&pageSize=2&pageNumber=2", Json);
        Assert.Single(second!.Items);

        Assert.DoesNotContain("passwordHash", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("securityStamp", raw, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Page_size_is_capped()
    {
        var admin = await _factory.LoginAsAdminAsync();
        var page = await admin.GetFromJsonAsync<UserPage>("/api/users?pageSize=100000", Json);
        Assert.Equal(100, page!.PageSize);
    }

    [Fact]
    public async Task User_can_change_email_and_log_in_with_the_new_one()
    {
        var admin = await _factory.LoginAsAdminAsync();
        var oldEmail = $"old-{Guid.NewGuid():N}@test.local";
        var newEmail = $"new-{Guid.NewGuid():N}@test.local";
        var user = await _factory.CreateUserAndLoginAsync(admin, "Staff", oldEmail);

        var response = await user.PostAsJsonAsync("/api/auth/change-email",
            new { newEmail, currentPassword = "Passw0rd!x" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var session = (await response.Content.ReadFromJsonAsync<TestHelpers.LoginResponse>(Json))!;
        Assert.False(string.IsNullOrEmpty(session.Token));

        await _factory.LoginAsync(newEmail, "Passw0rd!x");
        var oldLogin = await _factory.CreateClient().PostAsJsonAsync("/api/auth/login",
            new { email = oldEmail, password = "Passw0rd!x" });
        Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);

        var listed = await admin.GetFromJsonAsync<UserPage>($"/api/users?searchTerm={newEmail}", Json);
        Assert.Single(listed!.Items);
    }

    [Fact]
    public async Task Change_email_requires_the_correct_password_and_does_not_end_the_session()
    {
        var admin = await _factory.LoginAsAdminAsync();
        var email = $"pw-{Guid.NewGuid():N}@test.local";
        var user = await _factory.CreateUserAndLoginAsync(admin, "Staff", email);

        var response = await user.PostAsJsonAsync("/api/auth/change-email",
            new { newEmail = $"x-{Guid.NewGuid():N}@test.local", currentPassword = "Wrong-pass1" });

        // 400, not 401: a 401 would make the frontend treat it as an expired session.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await _factory.LoginAsync(email, "Passw0rd!x");
    }

    [Fact]
    public async Task Change_email_rejects_an_address_used_by_another_user_and_invalid_input()
    {
        var admin = await _factory.LoginAsAdminAsync();
        var taken = $"taken-{Guid.NewGuid():N}@test.local";
        await _factory.CreateUserAndLoginAsync(admin, "Staff", taken);
        var user = await _factory.CreateUserAndLoginAsync(admin, "Staff");

        var duplicate = await user.PostAsJsonAsync("/api/auth/change-email",
            new { newEmail = taken.ToUpperInvariant(), currentPassword = "Passw0rd!x" });
        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);

        var invalid = await user.PostAsJsonAsync("/api/auth/change-email",
            new { newEmail = "not-an-email", currentPassword = "Passw0rd!x" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
    }

    [Fact]
    public async Task Repeated_wrong_passwords_on_change_email_lock_the_account()
    {
        var admin = await _factory.LoginAsAdminAsync();
        var email = $"brute-{Guid.NewGuid():N}@test.local";
        var user = await _factory.CreateUserAndLoginAsync(admin, "Staff", email);

        for (var i = 0; i < 5; i++)
        {
            var bad = await user.PostAsJsonAsync("/api/auth/change-email",
                new { newEmail = $"y-{i}-{Guid.NewGuid():N}@test.local", currentPassword = "Wrong-pass1" });
            Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
        }

        var login = await _factory.CreateClient().PostAsJsonAsync("/api/auth/login",
            new { email, password = "Passw0rd!x" });
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    [Fact]
    public async Task Anonymous_cannot_change_email()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/api/auth/change-email",
            new { newEmail = "a@test.local", currentPassword = "x" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
