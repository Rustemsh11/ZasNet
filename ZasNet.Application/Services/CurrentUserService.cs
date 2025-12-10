using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ZasNet.Application.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public int CurrentUserId
    {
        get 
        {
            var userIdStr = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
            {
                throw new UnauthorizedAccessException(); 
            }

            return userId;
        }
    }
}
