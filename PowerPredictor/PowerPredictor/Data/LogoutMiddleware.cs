using Microsoft.AspNetCore.Identity;
using PowerPredictor.Models;
using System.Collections.Concurrent;

namespace PowerPredictor.Data
{
    public class LogoutMiddleware
    {
        private readonly RequestDelegate _next;

        public LogoutMiddleware(RequestDelegate next)
        {
            _next = next;
        }

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
