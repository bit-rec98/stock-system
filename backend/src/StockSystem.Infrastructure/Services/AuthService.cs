using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StockSystem.Application.Common;
using StockSystem.Application.DTOs.Auth;
using StockSystem.Application.Interfaces;
using StockSystem.Domain.Entities;

namespace StockSystem.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            return Result<AuthResponseDto>.Failure("Invalid email or password.");

        if (!user.IsActive)
            return Result<AuthResponseDto>.Failure("User account is disabled.");

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
            DateTime.UtcNow.AddHours(double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24"))
        ));
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
            return Result<AuthResponseDto>.Failure("User with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
            return Result<AuthResponseDto>.Failure(result.Errors.Select(e => e.Description));

        // Assign Admin role by default for this app (as per requirements)
        await AssignRoleAsync(user.Email, "Admin");

        var token = await GenerateJwtToken(user);
        var roles = await _userManager.GetRolesAsync(user);

        return Result<AuthResponseDto>.Success(new AuthResponseDto(
            token,
            user.Email!,
            user.FirstName,
            user.LastName,
            roles,
            DateTime.UtcNow.AddHours(double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24"))
        ));
    }

    public async Task<Result<bool>> AssignRoleAsync(string email, string role)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return Result<bool>.Failure("User not found.");

        if (!await _roleManager.RoleExistsAsync(role))
        {
            await _roleManager.CreateAsync(new IdentityRole(role));
        }

        var result = await _userManager.AddToRoleAsync(user, role);
        return result.Succeeded 
            ? Result<bool>.Success(true) 
            : Result<bool>.Failure(result.Errors.Select(e => e.Description));
    }

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
            expires: DateTime.UtcNow.AddHours(double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
