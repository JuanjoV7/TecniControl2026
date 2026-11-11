using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TecniControl2026.Web.Data.Entities;

namespace TecniControl2026.Web.Data.Configurations
{
    public class ServiceOrderConfiguration : IEntityTypeConfiguration<ServiceOrder>
    {
        public void Configure(EntityTypeBuilder<ServiceOrder> builder)
        {
            builder.ToTable("ServiceOrders");

            // Auto-increment number; never sent in an UPDATE
            builder.Property(o => o.Number)
                   .UseIdentityColumn();

            builder.HasIndex(o => o.Number).IsUnique();

            builder.Property(o => o.Status)
                   .HasConversion<string>()
                   .HasMaxLength(30);

            builder.HasIndex(o => o.Status);

            builder.HasOne(o => o.Device)
                   .WithMany(e => e.ServiceOrders)
                   .HasForeignKey(o => o.DeviceId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.ReceivedBy)
                   .WithMany()
                   .HasForeignKey(o => o.ReceivedById)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Technician)
                   .WithMany()
                   .HasForeignKey(o => o.TechnicianId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
