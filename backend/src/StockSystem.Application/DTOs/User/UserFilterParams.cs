using StockSystem.Application.Common;

namespace StockSystem.Application.DTOs.User;

public class UserFilterParams : PaginationParams
{
    /// <summary>Matches email, first name or last name (contains, case-insensitive).</summary>
    public string? SearchTerm { get; set; }
}
