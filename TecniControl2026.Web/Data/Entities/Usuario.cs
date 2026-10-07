using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using TecniControl2026.Web.Data.Abstractions;

namespace TecniControl2026.Web.Data.Entities
{
    // Email, PasswordHash, PhoneNumber, etc. vienen de IdentityUser
    public class Usuario : IdentityUser<Guid>, IId
    {
        [MaxLength(30)]
        public string Documento { get; set; } = null!;

        [MaxLength(100)]
        public string Nombres { get; set; } = null!;

        [MaxLength(100)]
        public string Apellidos { get; set; } = null!;

        public bool Activo { get; set; } = true;

        public Guid RolId { get; set; }
        public Rol Rol { get; set; } = null!;

        [NotMapped]
        public string NombreCompleto => $"{Nombres} {Apellidos}";
    }
}
