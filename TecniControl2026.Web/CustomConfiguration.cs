using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TecniControl2026.Web.Core.Authorization;
using TecniControl2026.Web.Data;
using TecniControl2026.Web.Data.Entities;
using TecniControl2026.Web.Data.Seeders;
using TecniControl2026.Web.Services.Abstractions;
using TecniControl2026.Web.Services.Implementations;

namespace TecniControl2026.Web
{
    public static class CustomConfiguration
    {
        public static WebApplicationBuilder AddCustomConfiguration(this WebApplicationBuilder builder)
        {
            // Data Context
            builder.Services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("MyConnection"));
            });

            // Automapper
            builder.Services.AddAutoMapper(typeof(Program));

            // Identity, authentication and authorization
            AddIdentity(builder);
            AddAuthorization(builder);

            // Services
            AddServices(builder);

            // Toast Notifications
            builder.Services.AddNotyf(config =>
            {
                config.DurationInSeconds = 10;
                config.IsDismissable = true;
                config.Position = NotyfPosition.BottomRight;
            });

            return builder;
        }

        private static void AddIdentity(WebApplicationBuilder builder)
        {
            builder.Services.AddIdentityCore<User>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<DataContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders()
            .AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>();

            builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
                            .AddIdentityCookies();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
            });

            // Frequently checks the security stamp to close the sessions of deactivated users
            builder.Services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.FromMinutes(1);
            });

            // Password recovery tokens expire in 2 hours
            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(2);
            });
        }

        private static void AddAuthorization(WebApplicationBuilder builder)
        {
            builder.Services.AddAuthorization();
            builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        }

        private static void AddServices(WebApplicationBuilder builder)
        {
            builder.Services.AddHttpContextAccessor();

            // Infrastructure
            builder.Services.AddScoped<SeedDb>();
            builder.Services.AddScoped<IPermissionsService, PermissionsService>();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddScoped<IEmailService, LogEmailService>();

            // Business
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<ICustomersService, CustomersService>();
            builder.Services.AddScoped<IDevicesService, DevicesService>();
            builder.Services.AddScoped<IServiceOrdersService, ServiceOrdersService>();
            builder.Services.AddScoped<IRolesService, RolesService>();
            builder.Services.AddScoped<IFollowUpsService, FollowUpsService>();
            builder.Services.AddScoped<IUsersService, UsersService>();
        }

        public static WebApplication AddCustomWebApplicationConfiguration(this WebApplication app)
        {
            app.UseNotyf();

            SeedData(app);

            return app;
        }

        private static void SeedData(WebApplication app)
        {
            IServiceScopeFactory scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

            using IServiceScope scope = scopeFactory.CreateScope();
            SeedDb service = scope.ServiceProvider.GetRequiredService<SeedDb>();
            service.SeedAsync().Wait();
        }
    }
}
