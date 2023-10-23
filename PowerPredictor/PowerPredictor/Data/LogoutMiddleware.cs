using Microsoft.AspNetCore.Identity;
using PowerPredictor.Models;
using System.Collections.Concurrent;

namespace PowerPredictor.Data
{
    /// <summary>
    /// Middleware for logging out users with User Manager
    /// </summary>
    public class LogoutMiddleware
    {
        private readonly RequestDelegate _next;

        public LogoutMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Called when user clisks logout button
        /// </summary>
        /// <param name="context"> Http context </param>
        /// <param name="signInManager"> Injected sign in manager</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context, SignInManager<User> signInManager)
        {
            if (context.Request.Path == "/account/logout")
            {
                await signInManager.SignOutAsync();
                context.Response.Redirect("/");
                return;
            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }
}
