using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using UserManager.Models;

namespace UserManager
{
    public class UserStatusCheckMiddleware
    {
        private readonly RequestDelegate _next;

        public UserStatusCheckMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null)
                {
                    await signInManager.SignOutAsync();
                    context.Response.Redirect("/Identity/Account/Login?error=Your account has been deleted.");
                    return;
                }
                if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
                {
                    await signInManager.SignOutAsync();
                    context.Response.Redirect("/Identity/Account/Login?error=Your account has been locked out. Please contact an administrator.");
                    return;
                }
            }
            await _next(context);
        }
    }

    public static class UserStatusCheckMiddlewareExtensions
    {
        public static IApplicationBuilder UseUserStatusCheck(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserStatusCheckMiddleware>();
        }
    }
}