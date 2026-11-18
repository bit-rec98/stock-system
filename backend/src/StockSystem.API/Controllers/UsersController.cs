using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockSystem.Application.DTOs.User;
using StockSystem.Application.Interfaces;

namespace StockSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.AdminOnly)]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>Paginated list of users with their role and status. Never includes credentials.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] UserFilterParams filter)
    {
        var result = await _userService.GetUsersAsync(filter);
        return Ok(result);
    }
}
