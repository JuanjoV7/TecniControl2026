using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TecniControl2026.Web.Core.Authorization;
using TecniControl2026.Web.Data;
using TecniControl2026.Web.Data.Entities;
using TecniControl2026.Web.Data.Seeding;
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

            // Identity, autenticación y autorización
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
            builder.Services.AddIdentityCore<Usuario>(options =>
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
            .AddClaimsPrincipalFactory<UsuarioClaimsPrincipalFactory>();

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

            // Revisa con frecuencia el security stamp para cerrar las sesiones de usuarios desactivados
            builder.Services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.FromMinutes(1);
            });

            // Los tokens de recuperación de contraseña vencen en 2 horas
            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(2);
            });
        }

        private static void AddAuthorization(WebApplicationBuilder builder)
        {
            builder.Services.AddAuthorization();
            builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermisoPolicyProvider>();
            builder.Services.AddScoped<IAuthorizationHandler, PermisoAuthorizationHandler>();
        }

        private static void AddServices(WebApplicationBuilder builder)
        {
            builder.Services.AddHttpContextAccessor();

            // Infraestructura
            builder.Services.AddScoped<SeedDb>();
            builder.Services.AddScoped<IPermisosService, PermisosService>();
            builder.Services.AddScoped<IUsuarioActualService, UsuarioActualService>();
            builder.Services.AddScoped<ICorreoService, LogCorreoService>();

            // Negocio
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IClientesService, ClientesService>();
            builder.Services.AddScoped<IEquiposService, EquiposService>();
            builder.Services.AddScoped<IOrdenesServicioService, OrdenesServicioService>();
            builder.Services.AddScoped<IRolesService, RolesService>();
            builder.Services.AddScoped<ISeguimientosService, SeguimientosService>();
            builder.Services.AddScoped<IUsuariosService, UsuariosService>();
        }

        public static WebApplication AddCustomWebApplicationConfiguration(this WebApplication app)
        {
            app.UseNotyf();

            return app;
        }

        public static async Task SeedDatabaseAsync(this WebApplication app)
        {
            using IServiceScope scope = app.Services.CreateScope();

            SeedDb seedDb = scope.ServiceProvider.GetRequiredService<SeedDb>();
            await seedDb.SeedAsync();
        }
    }
}
