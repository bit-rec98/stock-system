namespace StockSystem.Application.DTOs.Auth;

public record UserDto(
    string Email,
    string FirstName,
    string LastName,
    string Role
);
