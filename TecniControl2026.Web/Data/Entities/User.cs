using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using TecniControl2026.Web.Data.Abstractions;

namespace TecniControl2026.Web.Data.Entities
{
    // Email, PasswordHash, PhoneNumber, etc. come from IdentityUser
    public class User : IdentityUser<Guid>, IId
    {
        [MaxLength(30)]
        public string Document { get; set; } = null!;

        [MaxLength(100)]
        public string FirstName { get; set; } = null!;

        [MaxLength(100)]
        public string LastName { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public Guid RoleId { get; set; }
        public Role Role { get; set; } = null!;

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}
