namespace StockSystem.Application.DTOs.Auth;

public record ChangeEmailDto(
    string NewEmail,
    string CurrentPassword
);
