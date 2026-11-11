using System.ComponentModel.DataAnnotations;
using TecniControl2026.Web.Data.Abstractions;

namespace TecniControl2026.Web.Data.Entities
{
    public class Permission : IId
    {
        [Key]
        public Guid Id { get; set; }

        // E.g.: "customers.create", "orders.assignTechnician"
        [MaxLength(100)]
        public required string Code { get; set; }

        [MaxLength(50)]
        public required string Module { get; set; }

        [MaxLength(200)]
        public required string Description { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
