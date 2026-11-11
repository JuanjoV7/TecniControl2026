using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TecniControl2026.Web.Core.Authorization;
using TecniControl2026.Web.Data.Entities;

namespace TecniControl2026.Web.Data.Seeders
{
    // Runs on application startup: applies pending migrations, synchronizes the catalog
    // of permissions, creates the base roles and an initial administrator if none exists.
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SeedDb> _logger;

        public SeedDb(DataContext context, UserManager<User> userManager, IConfiguration configuration, ILogger<SeedDb> logger)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            await _context.Database.MigrateAsync();

            await SeedPermissionsAsync();
            await SeedRolesAsync();
            await SeedAdministratorAsync();
        }

        private async Task SeedPermissionsAsync()
        {
            List<Permission> existing = await _context.Permissions.ToListAsync();

            foreach (PermissionsCatalog.PermissionDefinition definition in PermissionsCatalog.All)
            {
                Permission? permission = existing.FirstOrDefault(p => p.Code == definition.Code);

                if (permission is null)
                {
                    await _context.Permissions.AddAsync(new Permission
                    {
                        Id = Guid.NewGuid(),
                        Code = definition.Code,
                        Module = definition.Module,
                        Description = definition.Description,
                    });
                }
                else
                {
                    permission.Module = definition.Module;
                    permission.Description = definition.Description;
                }
            }

            // Permissions that no longer exist in the catalog
            HashSet<string> codes = PermissionsCatalog.All.Select(p => p.Code).ToHashSet();
            _context.Permissions.RemoveRange(existing.Where(p => !codes.Contains(p.Code)));

            await _context.SaveChangesAsync();
        }

        private async Task SeedRolesAsync()
        {
            Dictionary<string, Guid> permissions = await _context.Permissions.ToDictionaryAsync(p => p.Code, p => p.Id);

            foreach ((string name, (string description, string[] codes)) in BaseRoles.Definitions)
            {
                Role? role = await _context.Roles.Include(r => r.RolePermissions)
                                               .FirstOrDefaultAsync(r => r.Name == name);

                if (role is null)
                {
                    role = new Role { Id = Guid.NewGuid(), Name = name, Description = description };
                    await _context.Roles.AddAsync(role);
                }
                else if (name != BaseRoles.Administrator)
                {
                    // The other base roles are only created; their permissions are configured later by the administrator
                    continue;
                }

                // The Administrator always has every permission, including the new ones in the catalog
                foreach (string code in codes)
                {
                    Guid permissionId = permissions[code];

                    if (!role.RolePermissions.Any(rp => rp.PermissionId == permissionId))
                    {
                        role.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permissionId });
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task SeedAdministratorAsync()
        {
            bool administratorExists = await _context.Users.AnyAsync(u => u.Role.Name == BaseRoles.Administrator);

            if (administratorExists)
            {
                return;
            }

            IConfigurationSection config = _configuration.GetSection("InitialAdministrator");
            string? email = config["Email"];
            string? password = config["Password"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("No hay administrador y falta la sección 'InitialAdministrator' (Email y Password) en la configuración");
                return;
            }

            Guid roleId = await _context.Roles.Where(r => r.Name == BaseRoles.Administrator)
                                             .Select(r => r.Id)
                                             .FirstAsync();

            User admin = new User
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Document = config["Document"] ?? "0000000000",
                FirstName = config["FirstName"] ?? "Administrador",
                LastName = config["LastName"] ?? "TecniControl",
                RoleId = roleId,
                IsActive = true,
            };

            IdentityResult result = await _userManager.CreateAsync(admin, password);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException("No fue posible crear el administrador inicial: "
                                                    + string.Join("; ", result.Errors.Select(e => e.Description)));
            }

            _logger.LogInformation("Administrador inicial creado: {Email}", email);
        }
    }
}
