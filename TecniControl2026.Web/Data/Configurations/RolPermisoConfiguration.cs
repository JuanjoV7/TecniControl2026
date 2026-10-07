using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TecniControl2026.Web.Data.Entities;

namespace TecniControl2026.Web.Data.Configurations
{
    public class RolPermisoConfiguration : IEntityTypeConfiguration<RolPermiso>
    {
        public void Configure(EntityTypeBuilder<RolPermiso> builder)
        {
            builder.ToTable("RolPermisos");

            builder.HasKey(rp => new { rp.RolId, rp.PermisoId });

            builder.HasOne(rp => rp.Rol)
                   .WithMany(r => r.RolPermisos)
                   .HasForeignKey(rp => rp.RolId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rp => rp.Permiso)
                   .WithMany(p => p.RolPermisos)
                   .HasForeignKey(rp => rp.PermisoId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
