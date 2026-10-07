using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TecniControl2026.Web.Core.Authorization;
using TecniControl2026.Web.Data.Entities;

namespace TecniControl2026.Web.Data.Seeding
{
    // Se ejecuta al iniciar la aplicación: aplica migraciones pendientes, sincroniza el catálogo
    // de permisos, crea los roles base y un administrador inicial si no existe ninguno.
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SeedDb> _logger;

        public SeedDb(DataContext context, UserManager<Usuario> userManager, IConfiguration configuration, ILogger<SeedDb> logger)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            await _context.Database.MigrateAsync();

            await SeedPermisosAsync();
            await SeedRolesAsync();
            await SeedAdministradorAsync();
        }

        private async Task SeedPermisosAsync()
        {
            List<Permiso> existentes = await _context.Permisos.ToListAsync();

            foreach (PermisosCatalogo.PermisoDefinicion definicion in PermisosCatalogo.Todos)
            {
                Permiso? permiso = existentes.FirstOrDefault(p => p.Codigo == definicion.Codigo);

                if (permiso is null)
                {
                    await _context.Permisos.AddAsync(new Permiso
                    {
                        Id = Guid.NewGuid(),
                        Codigo = definicion.Codigo,
                        Modulo = definicion.Modulo,
                        Descripcion = definicion.Descripcion,
                    });
                }
                else
                {
                    permiso.Modulo = definicion.Modulo;
                    permiso.Descripcion = definicion.Descripcion;
                }
            }

            // Permisos que ya no existen en el catálogo
            HashSet<string> codigos = PermisosCatalogo.Todos.Select(p => p.Codigo).ToHashSet();
            _context.Permisos.RemoveRange(existentes.Where(p => !codigos.Contains(p.Codigo)));

            await _context.SaveChangesAsync();
        }

        private async Task SeedRolesAsync()
        {
            Dictionary<string, Guid> permisos = await _context.Permisos.ToDictionaryAsync(p => p.Codigo, p => p.Id);

            foreach ((string nombre, (string descripcion, string[] codigos)) in RolesBase.Definiciones)
            {
                Rol? rol = await _context.Roles.Include(r => r.RolPermisos)
                                               .FirstOrDefaultAsync(r => r.Nombre == nombre);

                if (rol is null)
                {
                    rol = new Rol { Id = Guid.NewGuid(), Nombre = nombre, Descripcion = descripcion };
                    await _context.Roles.AddAsync(rol);
                }
                else if (nombre != RolesBase.Administrador)
                {
                    // Los demás roles base solo se crean; después sus permisos los configura el administrador
                    continue;
                }

                // El Administrador siempre tiene todos los permisos, incluidos los nuevos del catálogo
                foreach (string codigo in codigos)
                {
                    Guid permisoId = permisos[codigo];

                    if (!rol.RolPermisos.Any(rp => rp.PermisoId == permisoId))
                    {
                        rol.RolPermisos.Add(new RolPermiso { RolId = rol.Id, PermisoId = permisoId });
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task SeedAdministradorAsync()
        {
            bool hayAdministrador = await _context.Users.AnyAsync(u => u.Rol.Nombre == RolesBase.Administrador);

            if (hayAdministrador)
            {
                return;
            }

            IConfigurationSection config = _configuration.GetSection("AdministradorInicial");
            string? email = config["Email"];
            string? password = config["Password"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("No hay administrador y falta la sección 'AdministradorInicial' (Email y Password) en la configuración");
                return;
            }

            Guid rolId = await _context.Roles.Where(r => r.Nombre == RolesBase.Administrador)
                                             .Select(r => r.Id)
                                             .FirstAsync();

            Usuario admin = new Usuario
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Documento = config["Documento"] ?? "0000000000",
                Nombres = config["Nombres"] ?? "Administrador",
                Apellidos = config["Apellidos"] ?? "TecniControl",
                RolId = rolId,
                Activo = true,
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
