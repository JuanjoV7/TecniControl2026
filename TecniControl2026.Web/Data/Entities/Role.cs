using System.ComponentModel.DataAnnotations;
using TecniControl2026.Web.Data.Abstractions;

namespace TecniControl2026.Web.Data.Entities
{
    public class Role : IId
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(50)]
        public required string Name { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
