using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TecniControl2026.Web.Data.Entities;

namespace TecniControl2026.Web.Data.Configurations
{
    public class DeviceConfiguration : IEntityTypeConfiguration<Device>
    {
        public void Configure(EntityTypeBuilder<Device> builder)
        {
            builder.ToTable("Devices");

            builder.Property(e => e.Type)
                   .HasConversion<string>()
                   .HasMaxLength(30);

            // The serial number is optional, but if present it cannot be repeated
            builder.HasIndex(e => e.SerialNumber)
                   .IsUnique()
                   .HasFilter("[SerialNumber] IS NOT NULL");

            builder.HasOne(e => e.Customer)
                   .WithMany(c => c.Devices)
                   .HasForeignKey(e => e.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
