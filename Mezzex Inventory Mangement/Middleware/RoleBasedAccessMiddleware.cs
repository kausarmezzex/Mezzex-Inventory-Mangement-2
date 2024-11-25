using Mezzex_Inventory_Mangement.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using Mezzex_Inventory_Mangement.Models;

namespace Mezzex_Inventory_Mangement.Middleware
{
    public class RoleBasedAccessMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleBasedAccessMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            RolePermissionService permissionService,
            UserManager<ApplicationUser> userManager)
        {
            // Skip access control for unauthenticated users
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await _next(context);
                return;
            }

            // Get the current request path
            var requestedPath = context.Request.Path.Value.ToLower().Split('?')[0]; // Normalize: remove query params

            if (requestedPath == "/" ||
                requestedPath.StartsWith("/account/login") ||
                requestedPath.StartsWith("/home") ||
                requestedPath.StartsWith("/account/selectcompany") ||
                requestedPath.StartsWith("/account/logout") ||
                requestedPath.StartsWith("/account/accessdenied") ||
                requestedPath.StartsWith("/account/forgotpassword") ||
                requestedPath.StartsWith("/account/resetpassword") ||
                requestedPath.StartsWith("/account/verifyotp"))
            {
                await _next(context);
                return;
            }



            // Get the currently logged-in user
            var user = await userManager.GetUserAsync(context.User);
            if (user == null)
            {
                await _next(context);
                return;
            }

            // Get the user's roles
            var userRoles = await userManager.GetRolesAsync(user);
            if (!userRoles.Any())
            {
                context.Response.StatusCode = 403; // Forbidden
                await context.Response.WriteAsync("Access Denied: No roles assigned.");
                return;
            }

            // Check if the user has access to the requested path
            var hasAccess = await permissionService.HasAccessToPageAsync(userRoles, requestedPath);
            if (!hasAccess)
            {
                context.Response.StatusCode = 403; // Forbidden
                context.Response.Redirect($"/Home/Error?message=Access Denied: You do not have permission to access '{requestedPath}'.");

                return;
            }

            // Continue processing the request if access is granted
            await _next(context);
        }
    }
}
