using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.ServiceOrder
{
    // DTO used to correct reception details before diagnosis starts
    public class UpdateReceptionDTO
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El reporte de falla es obligatorio")]
        [MaxLength(1000, ErrorMessage = "El reporte de falla no puede superar los 1000 caracteres")]
        public string ReportedIssue { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Los accesorios no pueden superar los 500 caracteres")]
        public string? Accessories { get; set; }
    }
}