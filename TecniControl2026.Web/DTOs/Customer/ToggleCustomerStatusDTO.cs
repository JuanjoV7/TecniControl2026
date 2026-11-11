using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.Customer
{
    public class ToggleCustomerStatusDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid CustomerId { get; set; }

        public bool IsActive { get; set; } = false;
    }
}
