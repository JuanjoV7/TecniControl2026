using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TecniControl2026.Web.Data.Entities;

namespace TecniControl2026.Web.Data.Configurations
{
    public class FollowUpConfiguration : IEntityTypeConfiguration<FollowUp>
    {
        public void Configure(EntityTypeBuilder<FollowUp> builder)
        {
            builder.ToTable("FollowUps");

            builder.Property(s => s.PreviousStatus)
                   .HasConversion<string>()
                   .HasMaxLength(30);

            builder.Property(s => s.NewStatus)
                   .HasConversion<string>()
                   .HasMaxLength(30);

            builder.HasOne(s => s.ServiceOrder)
                   .WithMany(o => o.FollowUps)
                   .HasForeignKey(s => s.ServiceOrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Users are deactivated, not deleted, to keep the history
            builder.HasOne(s => s.User)
                   .WithMany()
                   .HasForeignKey(s => s.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
