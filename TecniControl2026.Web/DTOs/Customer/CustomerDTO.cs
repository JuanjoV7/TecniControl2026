namespace TecniControl2026.Web.DTOs.Customer
{
    public class CustomerDTO
    {
        public Guid Id { get; set; }
        public required string Document { get; set; }
        public required string Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
