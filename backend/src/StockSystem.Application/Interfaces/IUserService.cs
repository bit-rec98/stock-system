using StockSystem.Application.Common;
using StockSystem.Application.DTOs.User;

namespace StockSystem.Application.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserListItemDto>> GetUsersAsync(UserFilterParams filter);
}
