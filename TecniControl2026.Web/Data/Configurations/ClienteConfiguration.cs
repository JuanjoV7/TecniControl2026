using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TecniControl2026.Web.Data.Entities;

namespace TecniControl2026.Web.Data.Configurations
{
    public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.HasIndex(c => c.Documento).IsUnique();
        }
    }
}
