using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StockSystem.Application.DTOs.Auth;
using StockSystem.Application.Interfaces;

namespace StockSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        
        if (!result.IsSuccess)
            return Unauthorized(new { message = result.Error ?? "Invalid credentials" });

        return Ok(result.Data);
    }

    /// <summary>Creates a user with the given role. Admin only; there is no self-registration.</summary>
    [Authorize(Policy = Policies.AdminOnly)]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error, errors = result.Errors });

        return StatusCode(StatusCodes.Status201Created, result.Data);
    }

    [Authorize]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User not authenticated" });

        var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error, errors = result.Errors });

        return Ok(new { message = "Password changed successfully" });
    }
}
