using Microsoft.AspNetCore.Mvc.Rendering;
using TecniControl2026.Web.DTOs.ServiceOrder;

namespace TecniControl2026.Web.Models
{
    
    public class OrderDetailsViewModel
    {
        public ServiceOrderDetailDTO Order { get; set; } = null!;

        public bool CanAssignTechnician { get; set; }
        public bool CanAttend { get; set; }
        public bool CanDeliver { get; set; }
        public bool CanCancel { get; set; }
        public bool CanAddFollowUp { get; set; }

        // Only filled when the user can assign technicians
        public List<SelectListItem> Technicians { get; set; } = new();

        public bool HasActions => CanAssignTechnician || CanAttend || CanDeliver || CanCancel;
    }
}
