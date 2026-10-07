using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TecniControl2026.Web.Data.Entities;

namespace TecniControl2026.Web.Data.Configurations
{
    public class SeguimientoConfiguration : IEntityTypeConfiguration<Seguimiento>
    {
        public void Configure(EntityTypeBuilder<Seguimiento> builder)
        {
            builder.ToTable("Seguimientos");

            builder.Property(s => s.EstadoAnterior)
                   .HasConversion<string>()
                   .HasMaxLength(30);

            builder.Property(s => s.EstadoNuevo)
                   .HasConversion<string>()
                   .HasMaxLength(30);

            builder.HasOne(s => s.OrdenServicio)
                   .WithMany(o => o.Seguimientos)
                   .HasForeignKey(s => s.OrdenServicioId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Los usuarios se desactivan, no se eliminan, para conservar el historial
            builder.HasOne(s => s.Usuario)
                   .WithMany()
                   .HasForeignKey(s => s.UsuarioId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
