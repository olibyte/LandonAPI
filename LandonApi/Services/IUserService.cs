using LandonApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LandonApi.Services
{
    public interface IUserService
    {
        Task<PagedResults<User>> GetUsersAsync(
            PagingOptions pagingOptions,
            SortOptions<User, UserEntity> sortOptions,
            SearchOptions<User, UserEntity> searchOptions);

        Task<(bool Succeeded, string ErrorMessage)> CreateUserAsync(RegisterForm form);

        Task<Guid?> GetUserIdAsync(ClaimsPrincipal principal);

        Task<User> GetUserByIdAsync(Guid userId);

        Task<User> GetUserAsync(ClaimsPrincipal user);
    }
}
