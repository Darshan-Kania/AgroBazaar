using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AgroBazaar.Data;
using AgroBazaar.Models.Entities;
using AgroBazaar.Repositories.Interfaces;
using AgroBazaar.Repositories.Implementations;
using AgroBazaar.Repositories.UnitOfWork;
using AgroBazaar.Services.Interfaces;
using AgroBazaar.Services.Implementations;

namespace AgroBazaar
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Entity Framework with optimizations
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection"),
                    new MySqlServerVersion(new Version(8, 0, 21)), 
                    mySqlOptions =>
                    {
                        mySqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null);
                        mySqlOptions.CommandTimeout(30);
                    })
                .UseLazyLoadingProxies() // Enable lazy loading with correct method
                .EnableSensitiveDataLogging(false) // Disable in production
                .EnableServiceProviderCaching()
                .ConfigureWarnings(warnings => 
                    warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.LazyLoadOnDisposedContextWarning));
            });

            // Add Identity services with optimizations
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;

                // User settings
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
                
                // Lockout settings for better security
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Configure Authentication with optimizations
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtSettings = Configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"];
                
                if (!string.IsNullOrEmpty(secretKey))
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                        ClockSkew = TimeSpan.Zero
                    };
                }
            });

            // Configure cookie settings for Identity with optimizations
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Auth/Login";
                options.LogoutPath = "/Auth/Logout";
                options.AccessDeniedPath = "/Auth/AccessDenied";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(24);
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Events.OnSigningIn = context =>
                {
                    // Reduce cookie size by only storing essential claims
                    context.Properties.IsPersistent = false;
                    return Task.CompletedTask;
                };
            });

            // Configure Authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy("FarmerOnly", policy => policy.RequireRole("Farmer"));
                options.AddPolicy("ConsumerOnly", policy => policy.RequireRole("Consumer"));
                options.AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser());
            });

            // Add MVC with optimizations
            services.AddControllersWithViews(options =>
            {
                // Add response caching for static content
                options.CacheProfiles.Add("Default",
                    new Microsoft.AspNetCore.Mvc.CacheProfile()
                    {
                        Duration = 60,
                        Location = Microsoft.AspNetCore.Mvc.ResponseCacheLocation.Any,
                        NoStore = false
                    });
            });

            // Add Razor Pages
            services.AddRazorPages();

            // Session support with optimizations
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = "AgroBazaar.Session";
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });

            // Add HTTP Context Accessor
            services.AddHttpContextAccessor();

            // Add memory cache with optimizations
            services.AddMemoryCache(options =>
            {
                options.SizeLimit = 1024; // Limit cache size
                options.CompactionPercentage = 0.25; // Compact when 75% full
            });

            // Add response caching
            services.AddResponseCaching(options =>
            {
                options.MaximumBodySize = 1024;
                options.UseCaseSensitivePaths = false;
            });

            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });

            // CORS policy for API calls
            services.AddCors(options =>
            {
                options.AddPolicy("AgroBazaarPolicy", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Add API controllers with optimizations
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                });

            // Repository Pattern - Register Repositories
            services.AddScoped<IGenericRepository<ApplicationUser>, GenericRepository<ApplicationUser>>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRatingRepository, ProductRatingRepository>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Business Services
            services.AddScoped<IAuthService, AuthService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios.
                app.UseHsts();
            }

            // Disable HTTPS redirection in development to avoid issues
            if (!env.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }
            
            app.UseStaticFiles();

            app.UseRouting();

            // CORS
            app.UseCors("AgroBazaarPolicy");

            // Session
            app.UseSession();

            // Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // Default MVC route
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                // Area routes for Farmer and Consumer modules
                endpoints.MapControllerRoute(
                    name: "farmer",
                    pattern: "Farmer/{action=Dashboard}/{id?}",
                    defaults: new { controller = "Farmer" });

                endpoints.MapControllerRoute(
                    name: "consumer",
                    pattern: "Consumer/{action=Dashboard}/{id?}",
                    defaults: new { controller = "Consumer" });

                // API routes
                endpoints.MapControllerRoute(
                    name: "api",
                    pattern: "api/{controller}/{action=Index}/{id?}");

                // Razor Pages
                endpoints.MapRazorPages();
            });
        }
    }
}
