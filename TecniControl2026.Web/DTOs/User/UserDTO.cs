namespace TecniControl2026.Web.DTOs.User
{
    public class UserDTO
    {
        public Guid Id { get; set; }
        public string Document { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = null!;
    }
}
