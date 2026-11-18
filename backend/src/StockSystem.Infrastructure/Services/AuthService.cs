using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StockSystem.Application.Common;
using StockSystem.Application.DTOs.Auth;
using StockSystem.Application.Interfaces;
using StockSystem.Domain.Common;
using StockSystem.Domain.Entities;

namespace StockSystem.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    private const string InvalidCredentials = "Invalid email or password.";

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
            return Result<AuthResponseDto>.Failure(InvalidCredentials);

        // Same message for locked and wrong-password cases so the response does not reveal account state.
        if (await _userManager.IsLockedOutAsync(user))
            return Result<AuthResponseDto>.Failure(InvalidCredentials);

        if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            await _userManager.AccessFailedAsync(user);
            return Result<AuthResponseDto>.Failure(InvalidCredentials);
        }

        if (!user.IsActive)
            return Result<AuthResponseDto>.Failure("User account is disabled.");

        await _userManager.ResetAccessFailedCountAsync(user);
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var token = await GenerateJwtToken(user);
        var roles = await _userManager.GetRolesAsync(user);

        return Result<AuthResponseDto>.Success(new AuthResponseDto(
            token,
            user.Email!,
            user.FirstName,
            user.LastName,
            roles,
            DateTime.UtcNow.AddHours(GetExpirationHours())
        ));
    }

    public async Task<Result<UserDto>> RegisterAsync(RegisterDto registerDto)
    {
        if (!Roles.IsValid(registerDto.Role))
            return Result<UserDto>.Failure("Invalid role.");

        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
            return Result<UserDto>.Failure("User with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
            return Result<UserDto>.Failure(result.Errors.Select(e => e.Description));

        var roleResult = await _userManager.AddToRoleAsync(user, registerDto.Role);
        if (!roleResult.Succeeded)
        {
            // Do not leave a user without a role behind.
            await _userManager.DeleteAsync(user);
            return Result<UserDto>.Failure(roleResult.Errors.Select(e => e.Description));
        }

        return Result<UserDto>.Success(new UserDto(user.Email!, user.FirstName, user.LastName, registerDto.Role));
    }

    public async Task<Result<bool>> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result<bool>.Failure("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
        if (!result.Succeeded)
            return Result<bool>.Failure(result.Errors.Select(e => e.Description));

        return Result<bool>.Success(true);
    }

    public async Task<Result<AuthResponseDto>> ChangeEmailAsync(string userId, ChangeEmailDto changeEmailDto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || !user.IsActive)
            return Result<AuthResponseDto>.Failure("User not found.");

        // The password check shares the login lockout counter, so this endpoint cannot be used to guess passwords.
        const string wrongPassword = "Current password is incorrect.";
        if (await _userManager.IsLockedOutAsync(user))
            return Result<AuthResponseDto>.Failure(wrongPassword);

        if (!await _userManager.CheckPasswordAsync(user, changeEmailDto.CurrentPassword))
        {
            await _userManager.AccessFailedAsync(user);
            return Result<AuthResponseDto>.Failure(wrongPassword);
        }
        await _userManager.ResetAccessFailedCountAsync(user);

        var newEmail = changeEmailDto.NewEmail.Trim();
        if (string.Equals(newEmail, user.Email, StringComparison.OrdinalIgnoreCase))
            return Result<AuthResponseDto>.Failure("The new email is the same as the current one.");

        var existing = await _userManager.FindByEmailAsync(newEmail);
        if (existing != null && existing.Id != user.Id)
            return Result<AuthResponseDto>.Failure("This email address is not available.");

        // Email and user name are kept identical (the email is the login). One update keeps them consistent.
        user.Email = newEmail;
        user.UserName = newEmail;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result<AuthResponseDto>.Failure(result.Errors.Select(e => e.Description));

        var token = await GenerateJwtToken(user);
        var roles = await _userManager.GetRolesAsync(user);

        return Result<AuthResponseDto>.Success(new AuthResponseDto(
            token,
            user.Email!,
            user.FirstName,
            user.LastName,
            roles,
            DateTime.UtcNow.AddHours(GetExpirationHours())
        ));
    }

    private double GetExpirationHours() =>
        double.TryParse(_configuration["Jwt:ExpirationHours"], System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var hours) && hours > 0 ? hours : 8;

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(GetExpirationHours()),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
