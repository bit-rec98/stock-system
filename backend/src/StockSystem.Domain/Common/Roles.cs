namespace StockSystem.Domain.Common;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Staff = "Staff";

    public static readonly IReadOnlyList<string> All = new[] { Admin, Staff };

    public static bool IsValid(string? role) =>
        role != null && All.Contains(role, StringComparer.Ordinal);
}
