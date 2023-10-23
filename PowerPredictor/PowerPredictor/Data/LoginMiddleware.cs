using Microsoft.AspNetCore.Identity;
using PowerPredictor.Models;
using System.Collections.Concurrent;
using static PowerPredictor.Pages.Account.Login;

namespace PowerPredictor.Data
{
    /// <summary>
    /// Middleware for logging in users with User Manager
    /// </summary>
    public class LoginMiddleware
    {
        /// <summary>
        /// Stores login information for users, accessed by Guid key
        /// </summary>
        public static IDictionary<Guid, LoginModel> Logins { get; private set; }
            = new ConcurrentDictionary<Guid, LoginModel>();


        private readonly RequestDelegate _next;

        public LoginMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Called when user submits valid login form
        /// </summary>
        /// <param name="context"> Http context </param>
        /// <param name="signInManager"> Injected sign in manager</param>
        public async Task Invoke(HttpContext context, SignInManager<User> signInManager)
        {
            if (context.Request.Path == "/account/login" && context.Request.Query.ContainsKey("key"))
            {
                var key = Guid.Parse(context.Request.Query["key"]);

                var info = Logins[key];

                var result = await signInManager.PasswordSignInAsync(info.Email, info.Password, info.RememberMe, lockoutOnFailure: true);
                info.Password = null;
                if (result.Succeeded)
                {
                    Logins.Remove(key);
                    context.Response.Redirect("/");
                    return;
                }
            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }
}
