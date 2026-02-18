namespace StockSystem.Application.DTOs.Auth;

public record AuthResponseDto(
    string Token,
    string Email,
    string FirstName,
    string LastName,
    IEnumerable<string> Roles,
    DateTime Expiration
);
