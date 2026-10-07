using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TecniControl2026.Web.Data.Entities;

namespace TecniControl2026.Web.Data.Configurations
{
    public class EquipoConfiguration : IEntityTypeConfiguration<Equipo>
    {
        public void Configure(EntityTypeBuilder<Equipo> builder)
        {
            builder.ToTable("Equipos");

            builder.Property(e => e.Tipo)
                   .HasConversion<string>()
                   .HasMaxLength(30);

            // El serial es opcional, pero si existe no se puede repetir
            builder.HasIndex(e => e.Serial)
                   .IsUnique()
                   .HasFilter("[Serial] IS NOT NULL");

            builder.HasOne(e => e.Cliente)
                   .WithMany(c => c.Equipos)
                   .HasForeignKey(e => e.ClienteId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
