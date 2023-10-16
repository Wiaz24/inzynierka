using Microsoft.AspNetCore.Identity;
using PowerPredictor.Models;
using System.Collections.Concurrent;

namespace PowerPredictor.Data
{
    public class LoginInfo
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }

    public class BlazorCookieLoginMiddleware
    {
        public static IDictionary<Guid, LoginInfo> Logins { get; private set; }
            = new ConcurrentDictionary<Guid, LoginInfo>();


        private readonly RequestDelegate _next;

        public BlazorCookieLoginMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, SignInManager<User> signInMgr)
        {
            if (context.Request.Path == "/account/login" && context.Request.Query.ContainsKey("key"))
            {
                #pragma warning disable CS8604 // Possible null reference argument.
                var key = Guid.Parse(context.Request.Query["key"]);
                #pragma warning restore CS8604 // Possible null reference argument.
                var info = Logins[key];

                var result = await signInMgr.PasswordSignInAsync(info.Email, info.Password, info.RememberMe, lockoutOnFailure: true);
                info.Password = null;
                if (result.Succeeded)
                {
                    Logins.Remove(key);
                    context.Response.Redirect("/");
                    return;
                }
                else if (result.RequiresTwoFactor)
                {
                    //TODO: redirect to 2FA razor component
                    context.Response.Redirect("/loginwith2fa/" + key);
                    return;
                }
                else
                {
                    //TODO: Proper error handling
                    context.Response.Redirect("/loginfailed");
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
