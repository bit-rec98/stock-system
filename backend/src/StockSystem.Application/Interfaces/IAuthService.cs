using StockSystem.Application.Common;
using StockSystem.Application.DTOs.Auth;

namespace StockSystem.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> LoginAsync(LoginDto loginDto);
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);
    Task<Result<bool>> AssignRoleAsync(string email, string role);
}
