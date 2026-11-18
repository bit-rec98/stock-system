namespace StockSystem.Application.DTOs.User;

public record UserListItemDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string? Role,
    bool IsActive,
    bool IsLockedOut,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);
