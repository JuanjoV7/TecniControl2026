using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TecniControl2026.Web.Data.Entities;

namespace TecniControl2026.Web.Data.Configurations
{
    public class PermisoConfiguration : IEntityTypeConfiguration<Permiso>
    {
        public void Configure(EntityTypeBuilder<Permiso> builder)
        {
            builder.ToTable("Permisos");

            builder.HasIndex(p => p.Codigo).IsUnique();
        }
    }
}
