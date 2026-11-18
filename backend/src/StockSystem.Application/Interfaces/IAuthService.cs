using StockSystem.Application.Common;
using StockSystem.Application.DTOs.Auth;

namespace StockSystem.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> LoginAsync(LoginDto loginDto);

    /// <summary>Creates a user with the requested role. Callers must be authorized as Admin.</summary>
    Task<Result<UserDto>> RegisterAsync(RegisterDto registerDto);

    Task<Result<bool>> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);

    /// <summary>
    /// Changes the signed-in user's email (which is also the login name). Requires the current password.
    /// Returns a new session because the token carries the email.
    /// </summary>
    Task<Result<AuthResponseDto>> ChangeEmailAsync(string userId, ChangeEmailDto changeEmailDto);
}
