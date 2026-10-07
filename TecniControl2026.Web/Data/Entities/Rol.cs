using System.ComponentModel.DataAnnotations;
using TecniControl2026.Web.Data.Abstractions;

namespace TecniControl2026.Web.Data.Entities
{
    public class Rol : IId
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(50)]
        public required string Nombre { get; set; }

        [MaxLength(200)]
        public string? Descripcion { get; set; }

        public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();

        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
