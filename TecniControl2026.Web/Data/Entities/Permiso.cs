using System.ComponentModel.DataAnnotations;
using TecniControl2026.Web.Data.Abstractions;

namespace TecniControl2026.Web.Data.Entities
{
    public class Permiso : IId
    {
        [Key]
        public Guid Id { get; set; }

        // Ej: "Clientes.Crear", "Ordenes.AsignarTecnico"
        [MaxLength(100)]
        public required string Codigo { get; set; }

        [MaxLength(50)]
        public required string Modulo { get; set; }

        [MaxLength(200)]
        public required string Descripcion { get; set; }

        public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
    }
}
