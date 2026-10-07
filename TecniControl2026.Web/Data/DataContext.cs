using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TecniControl2026.Web.Data.Entities;

namespace TecniControl2026.Web.Data
{
    // IdentityUserContext aporta las tablas de usuarios (contraseñas, tokens de recuperación, etc.)
    // sin las tablas de roles de Identity: los roles y permisos son propios del sistema.
    public class DataContext : IdentityUserContext<Usuario, Guid>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Equipo> Equipos { get; set; }
        public DbSet<OrdenServicio> OrdenesServicio { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<RolPermiso> RolPermisos { get; set; }
        public DbSet<Seguimiento> Seguimientos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UsuarioClaims");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UsuarioLogins");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UsuarioTokens");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
        }
    }
}
