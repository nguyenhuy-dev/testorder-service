using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UnauthorizedAccessException=TestOrderService.Application.Exceptions.UnauthorizedAccessException;

namespace TestOrderService.API.Controllers
{
    /// <summary>
    ///     The base api controller class
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        ///     Gets the value of the current user id
        /// </summary>
        protected Guid CurrentUserId
        {
            get
            {
                var userIdValue = Request.Headers.TryGetValue("X-User-Id", out var headerValue)
                    ? headerValue.ToString()
                    : User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (Guid.TryParse(userIdValue, out var userId))
                {
                    return userId;
                }

                // Start of Optional: If you want to force 401 logic here, 
                // usually [Authorize] handles the gatekeeping, but safe parsing is good.
                throw new UnauthorizedAccessException("Authentication token is missing or invalid.");
            }
        }

        /// <summary>
        ///     Gets the value of the current user name
        /// </summary>
        protected string CurrentUserName
        {
            get => User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
        }

        /// <summary>
        ///     Gets the value of the current user role
        /// </summary>
        protected string CurrentUserRole
        {
            get => User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }
    }
}
