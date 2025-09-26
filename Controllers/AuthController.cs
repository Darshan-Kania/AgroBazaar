using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using AgroBazaar.Models.Entities;
using AgroBazaar.Models.ViewModels;
using AgroBazaar.Services.Interfaces;

namespace AgroBazaar.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IMemoryCache cache,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _cache = cache;
            _logger = logger;
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Dashboard", GetControllerByUserType());
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Use optimized user lookup with caching
                var user = await GetUserByEmailCachedAsync(model.Email);
                if (user == null || !user.IsActive)
                {
                    ModelState.AddModelError(string.Empty, "Invalid email or password");
                    return View(model);
                }

                // Use SignInManager's optimized password check
                var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: true);
                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                    {
                        ModelState.AddModelError(string.Empty, "Account is locked out. Please try again later.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid email or password");
                    }
                    return View(model);
                }

                // Store minimal user info in session synchronously
                await StoreUserSessionAsync(user);

                TempData["SuccessMessage"] = $"Welcome back, {user.FirstName}!";

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                // Redirect based on user type
                return RedirectToAction("Dashboard", GetControllerByUserType(user.UserType));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Dashboard", GetControllerByUserType());
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Ensure roles exist synchronously
                await EnsureRolesExistAsync();

                // Quick email existence check with caching
                var existingUser = await GetUserByEmailCachedAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "Email is already registered");
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserType = model.UserType,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    City = model.City,
                    State = model.State,
                    PinCode = model.PinCode,
                    EmailConfirmed = true, // Auto-confirm for simplicity
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }

                // Add user to role asynchronously
                await _userManager.AddToRoleAsync(user, model.UserType);

                // Clear user cache since we added a new user
                _cache.Remove($"user_email_{model.Email.ToLower()}");

                // Sign in the user
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Store user session info asynchronously
                _ = Task.Run(() => StoreUserSessionAsync(user));

                TempData["SuccessMessage"] = $"Welcome to AgroBazaar, {user.FirstName}! Your account has been created successfully.";

                // Redirect to appropriate dashboard
                return RedirectToAction("Dashboard", GetControllerByUserType(user.UserType));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
                return View(model);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            
            // Clear session asynchronously
            _ = Task.Run(() =>
            {
                HttpContext.Session.Remove("UserId");
                HttpContext.Session.Remove("UserType");
                HttpContext.Session.Remove("UserName");
            });

            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            var vm = new ProfileViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserType = user.UserType,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,
                State = user.State,
                PinCode = user.PinCode,
                CreatedAt = user.CreatedAt
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.City = model.City;
            user.State = model.State;
            user.PinCode = model.PinCode;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // Update cached user if present
            _cache.Remove($"user_email_{(user.Email ?? string.Empty).ToLower()}");
            _ = Task.Run(() => StoreUserSessionAsync(user));

            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Re-hydrate profile data for the view on validation errors
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = userId != null ? await _userManager.FindByIdAsync(userId) : null;
                var profileVm = user == null ? new ProfileViewModel() : new ProfileViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    UserType = user.UserType,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    City = user.City,
                    State = user.State,
                    PinCode = user.PinCode,
                    CreatedAt = user.CreatedAt
                };
                ViewData["PasswordErrors"] = true;
                return View("Profile", profileVm);
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                TempData["ErrorMessage"] = "Please login again.";
                return RedirectToAction("Login");
            }

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            var changeResult = await _userManager.ChangePasswordAsync(currentUser, model.CurrentPassword, model.NewPassword);
            if (!changeResult.Succeeded)
            {
                foreach (var error in changeResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                // reload profile info to redisplay the page with errors
                var vm = new ProfileViewModel
                {
                    Id = currentUser.Id,
                    Email = currentUser.Email ?? string.Empty,
                    UserType = currentUser.UserType,
                    FirstName = currentUser.FirstName,
                    LastName = currentUser.LastName,
                    PhoneNumber = currentUser.PhoneNumber,
                    Address = currentUser.Address,
                    City = currentUser.City,
                    State = currentUser.State,
                    PinCode = currentUser.PinCode,
                    CreatedAt = currentUser.CreatedAt
                };
                ViewData["PasswordErrors"] = true;
                return View("Profile", vm);
            }

            await _signInManager.RefreshSignInAsync(currentUser);
            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction(nameof(Profile));
        }

        // Optimized helper methods
        private async Task<ApplicationUser?> GetUserByEmailCachedAsync(string email)
        {
            var cacheKey = $"user_email_{email.ToLower()}";
            
            if (_cache.TryGetValue(cacheKey, out ApplicationUser? cachedUser))
            {
                return cachedUser;
            }

            var user = await _userManager.FindByEmailAsync(email);
            
            if (user != null)
            {
                // Cache for 5 minutes with proper size specification
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                    Size = 1 // Specify size for the cache entry
                };
                _cache.Set(cacheKey, user, cacheEntryOptions);
            }
            
            return user;
        }

        private async Task StoreUserSessionAsync(ApplicationUser user)
        {
            try
            {
                HttpContext.Session.SetString("UserId", user.Id);
                HttpContext.Session.SetString("UserType", user.UserType);
                HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");
                await HttpContext.Session.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to store user session for user: {UserId}", user.Id);
            }
        }

        private async Task EnsureRolesExistAsync()
        {
            try
            {
                var roles = new[] { "Farmer", "Consumer" };
                foreach (var role in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring roles exist");
            }
        }

        private string GetControllerByUserType(string? userType = null)
        {
            if (userType != null)
            {
                return userType == "Farmer" ? "Farmer" : "Consumer";
            }

            if (User.IsInRole("Farmer"))
                return "Farmer";
            
            return "Consumer";
        }
    }
}
