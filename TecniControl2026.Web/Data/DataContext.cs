using Microsoft.EntityFrameworkCore;
using TecniControl2026.Web.Data.Entities;

namespace TecniControl2026.Web.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cliente>().HasIndex(c => c.Documento).IsUnique();
        }
    }
}
