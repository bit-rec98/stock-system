using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StockSystem.Infrastructure.Data;

namespace StockSystem.Tests;

/// <summary>Boots the real API pipeline against an in-memory database with test-only secrets.</summary>
public class TestWebAppFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@test.local";
    public const string AdminPassword = "Admin#Test123";

    static TestWebAppFactory()
    {
        // Program reads these from the process environment. Values are throwaway and only used in tests.
        Environment.SetEnvironmentVariable("DB_CONNECTION_STRING", "Server=unused;Database=unused");
        Environment.SetEnvironmentVariable("JWT_KEY", "test-only-key-that-is-at-least-32-bytes-long!");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "StockSystem.Tests");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "StockSystem.Tests.Users");
        Environment.SetEnvironmentVariable("ADMIN_EMAIL", AdminEmail);
        Environment.SetEnvironmentVariable("ADMIN_PASSWORD", AdminPassword);
        Environment.SetEnvironmentVariable("SEED_SAMPLE_DATA", "false");
        Environment.SetEnvironmentVariable("RateLimit__AuthPermitPerMinute", "1000");
    }

    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        builder.ConfigureServices(services =>
        {
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
                         || d.ServiceType == typeof(DbContextOptions))
                .ToList();
            foreach (var d in descriptors) services.Remove(d);

            services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(_databaseName));
        });
    }
}
