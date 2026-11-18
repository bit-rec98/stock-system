namespace StockSystem.API;

public static class Policies
{
    /// <summary>Admin only: user management and destructive or structural changes.</summary>
    public const string AdminOnly = "AdminOnly";

    /// <summary>Admin or Staff: read access and day-to-day product maintenance.</summary>
    public const string StaffOrAdmin = "StaffOrAdmin";
}

public static class RateLimitPolicies
{
    public const string Auth = "auth";
}
