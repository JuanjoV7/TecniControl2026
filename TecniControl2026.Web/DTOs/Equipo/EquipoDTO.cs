using System.ComponentModel.DataAnnotations;
using TecniControl2026.Web.Data.Enums;

namespace TecniControl2026.Web.DTOs.Equipo
{
    public class EquipoDTO
    {
        public Guid Id { get; set; }

        [Display(Name = "Tipo")]
        public TipoEquipo Tipo { get; set; }

        public string Marca { get; set; } = null!;
        public string Modelo { get; set; } = null!;
        public string? Serial { get; set; }

        public Guid ClienteId { get; set; }

        [Display(Name = "Cliente")]
        public string ClienteNombre { get; set; } = null!;

        public string ClienteDocumento { get; set; } = null!;

        public string Descripcion => $"{Tipo} {Marca} {Modelo}" + (string.IsNullOrWhiteSpace(Serial) ? "" : $" (S/N {Serial})");
    }
}
