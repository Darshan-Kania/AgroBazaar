using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AgroBazaar.Models.Entities;

namespace AgroBazaar.Middleware
{
    public class JwtCookieAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public JwtCookieAuthenticationMiddleware(
            RequestDelegate next,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _next = next;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if user is already authenticated
            if (!context.User.Identity?.IsAuthenticated == true)
            {
                // Try to get JWT token from cookie
                if (context.Request.Cookies.TryGetValue("AuthToken", out var token) && !string.IsNullOrEmpty(token))
                {
                    try
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var jsonToken = handler.ReadJwtToken(token);
                        
                        if (jsonToken.ValidTo > DateTime.UtcNow)
                        {
                            var userId = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                            if (!string.IsNullOrEmpty(userId))
                            {
                                var user = await _userManager.FindByIdAsync(userId);
                                if (user != null && user.IsActive)
                                {
                                    // Sign in the user using Identity
                                    await _signInManager.SignInAsync(user, isPersistent: true);
                                }
                            }
                        }
                        else
                        {
                            // Token expired, remove cookie
                            context.Response.Cookies.Delete("AuthToken");
                        }
                    }
                    catch
                    {
                        // Invalid token, remove cookie
                        context.Response.Cookies.Delete("AuthToken");
                    }
                }
            }

            await _next(context);
        }
    }
}
