using System.ComponentModel.DataAnnotations;
using TecniControl2026.Web.Data.Abstractions;

namespace TecniControl2026.Web.Data.Entities
{
    public class Customer : IId
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(30)]
        public required string Document { get; set; }

        [MaxLength(100)]
        public required string Name { get; set; }

        [MaxLength(30)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Device> Devices { get; set; } = new List<Device>();
    }
}
