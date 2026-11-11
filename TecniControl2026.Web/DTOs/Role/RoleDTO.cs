namespace TecniControl2026.Web.DTOs.Role
{
    public class RoleDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int UserCount { get; set; }
        public List<PermissionDTO> Permissions { get; set; } = new();
    }
}
