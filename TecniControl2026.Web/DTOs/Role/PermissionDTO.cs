namespace TecniControl2026.Web.DTOs.Role
{
    public class PermissionDTO
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string Module { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
