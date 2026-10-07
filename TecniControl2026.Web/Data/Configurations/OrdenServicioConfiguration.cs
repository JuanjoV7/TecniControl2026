using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TecniControl2026.Web.Data.Entities;

namespace TecniControl2026.Web.Data.Configurations
{
    public class OrdenServicioConfiguration : IEntityTypeConfiguration<OrdenServicio>
    {
        public void Configure(EntityTypeBuilder<OrdenServicio> builder)
        {
            builder.ToTable("OrdenesServicio");

            // Consecutivo autoincremental; nunca se envía en un UPDATE
            builder.Property(o => o.Numero)
                   .UseIdentityColumn();

            builder.HasIndex(o => o.Numero).IsUnique();

            builder.Property(o => o.Estado)
                   .HasConversion<string>()
                   .HasMaxLength(30);

            builder.HasIndex(o => o.Estado);

            builder.HasOne(o => o.Equipo)
                   .WithMany(e => e.OrdenesServicio)
                   .HasForeignKey(o => o.EquipoId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.RecibidaPor)
                   .WithMany()
                   .HasForeignKey(o => o.RecibidaPorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Tecnico)
                   .WithMany()
                   .HasForeignKey(o => o.TecnicoId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
