using Microsoft.AspNetCore.Mvc;
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

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        
        if (!result.IsSuccess)
            return Unauthorized(new { message = result.Error ?? "Invalid credentials" });

        return Ok(result.Data);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error, errors = result.Errors });

        return Ok(result.Data);
    }
}
