using Microsoft.EntityFrameworkCore;
using StockSystem.Application.Common;
using StockSystem.Application.DTOs.User;
using StockSystem.Application.Interfaces;
using StockSystem.Infrastructure.Data;

namespace StockSystem.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<UserListItemDto>> GetUsersAsync(UserFilterParams filter)
    {
        var users = _context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            users = users.Where(u =>
                (u.Email != null && u.Email.ToLower().Contains(term)) ||
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term));
        }

        var totalCount = await users.CountAsync();

        // Users have a single role in this application; the left join keeps users without one.
        var page = await (
            from u in users.OrderBy(u => u.Email).Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize)
            join ur in _context.UserRoles on u.Id equals ur.UserId into userRoles
            from ur in userRoles.DefaultIfEmpty()
            join r in _context.Roles on ur.RoleId equals r.Id into roles
            from r in roles.DefaultIfEmpty()
            orderby u.Email
            select new
            {
                u.Id, u.Email, u.FirstName, u.LastName, u.IsActive, u.LockoutEnd, u.CreatedAt, u.LastLoginAt,
                Role = r.Name
            })
            .ToListAsync();

        var now = DateTimeOffset.UtcNow;
        var items = page.Select(x => new UserListItemDto(
            x.Id,
            x.Email ?? string.Empty,
            x.FirstName,
            x.LastName,
            x.Role,
            x.IsActive,
            x.LockoutEnd.HasValue && x.LockoutEnd > now,
            x.CreatedAt,
            x.LastLoginAt)).ToList();

        return new PagedResult<UserListItemDto>(items, totalCount, filter.PageNumber, filter.PageSize);
    }
}
