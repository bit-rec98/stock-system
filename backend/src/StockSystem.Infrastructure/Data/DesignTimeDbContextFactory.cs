using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StockSystem.Infrastructure.Data;

/// <summary>
/// Used only by `dotnet ef` to build the model and generate migrations without starting the API.
/// The connection string is a placeholder; no connection is opened when adding a migration.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
            ?? "Server=localhost;Database=StockSystemDb;User Id=design;Password=design;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connection)
            .Options;

        return new ApplicationDbContext(options);
    }
}
