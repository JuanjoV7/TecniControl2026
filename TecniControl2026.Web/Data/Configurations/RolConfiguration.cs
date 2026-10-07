using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TecniControl2026.Web.Data.Entities;

namespace TecniControl2026.Web.Data.Configurations
{
    public class RolConfiguration : IEntityTypeConfiguration<Rol>
    {
        public void Configure(EntityTypeBuilder<Rol> builder)
        {
            builder.ToTable("Roles");

            builder.HasIndex(r => r.Nombre).IsUnique();
        }
    }
}
