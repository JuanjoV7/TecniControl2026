using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Authorization;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.Data.Enums;
using TecniControl2026.Web.DTOs.ServiceOrder;
using TecniControl2026.Web.DTOs.User;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Controllers
{
    public class ServiceOrdersController : Controller
    {
        private readonly INotyfService _notyfService;
        private readonly IServiceOrdersService _serviceOrdersService;
        private readonly IUsersService _usersService;

        public ServiceOrdersController(
            INotyfService notyfService,
            IServiceOrdersService serviceOrdersService,
            IUsersService usersService)
        {
            _notyfService = notyfService;
            _serviceOrdersService = serviceOrdersService;
            _usersService = usersService;
        }

        // Lists orders with pagination, status filter and search filter
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] OrdersPaginationRequest request)
        {
            Response<PaginationResponse<ServiceOrderDTO>> response = await _serviceOrdersService.GetPaginationAsync(request);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Filter = request.Filter;
            ViewBag.SelectedStatus = request.Status;
            ViewBag.Statuses = GetStatusSelectList(request.Status);

            return View(response.Result);
        }

        // Displays full order details and follow-up history
        [HttpGet]
        public async Task<IActionResult> Details([FromRoute] Guid id)
        {
            Response<ServiceOrderDetailDTO> response = await _serviceOrdersService.GetOneAsync(id);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction(nameof(Index));
            }

            await LoadTechniciansAsync(response.Result.TechnicianId);

            return View(response.Result);
        }

        [HttpGet]
        [HasPermission(PermissionsCatalog.Orders.Create)]
        public async Task<IActionResult> Create()
        {
            if (!await LoadTechniciansAsync())
            {
                return RedirectToAction(nameof(Index));
            }

            return View(new CreateServiceOrderDTO());
        }

        [HttpPost, ValidateAntiForgeryToken]
        [HasPermission(PermissionsCatalog.Orders.Create)]
        public async Task<IActionResult> Create([FromForm] CreateServiceOrderDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                await LoadTechniciansAsync(dto.TechnicianId);
                return View(dto);
            }

            Response<ServiceOrderDTO> response = await _serviceOrdersService.CreateAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                await LoadTechniciansAsync(dto.TechnicianId);
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        // Assigns or reassigns a technician to the order
        [HttpPost, ValidateAntiForgeryToken]
        [HasPermission(PermissionsCatalog.Orders.AssignTechnician)]
        public async Task<IActionResult> AssignTechnician([FromForm] AssignTechnicianDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe seleccionar un técnico válido");
                return RedirectToAction(nameof(Details), new { id = dto.ServiceOrderId });
            }

            Response<object> response = await _serviceOrdersService.AssignTechnicianAsync(dto);

            if (response.IsSuccess)
            {
                _notyfService.Success(response.Message);
            }
            else
            {
                _notyfService.Error(response.Message);
            }

            return RedirectToAction(nameof(Details), new { id = dto.ServiceOrderId });
        }

        // Registers diagnosis, work performed, observations and status changes
        [HttpPost, ValidateAntiForgeryToken]
        [HasPermission(PermissionsCatalog.Orders.Attend)]
        public async Task<IActionResult> RegisterAttention([FromForm] RegisterAttentionDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación de la atención");
                return RedirectToAction(nameof(Details), new { id = dto.ServiceOrderId });
            }

            Response<object> response = await _serviceOrdersService.RegisterAttentionAsync(dto);

            if (response.IsSuccess)
            {
                _notyfService.Success(response.Message);
            }
            else
            {
                _notyfService.Error(response.Message);
            }

            return RedirectToAction(nameof(Details), new { id = dto.ServiceOrderId });
        }

        // Marks a ready order as delivered to the customer
        [HttpPost, ValidateAntiForgeryToken]
        [HasPermission(PermissionsCatalog.Orders.Deliver)]
        public async Task<IActionResult> Deliver([FromForm] DeliverOrderDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación para la entrega");
                return RedirectToAction(nameof(Details), new { id = dto.ServiceOrderId });
            }

            Response<object> response = await _serviceOrdersService.DeliverAsync(dto);

            if (response.IsSuccess)
            {
                _notyfService.Success(response.Message);
            }
            else
            {
                _notyfService.Error(response.Message);
            }

            return RedirectToAction(nameof(Details), new { id = dto.ServiceOrderId });
        }

        // Cancels an order with a required reason
        [HttpPost, ValidateAntiForgeryToken]
        [HasPermission(PermissionsCatalog.Orders.Cancel)]
        public async Task<IActionResult> Cancel([FromForm] CancelOrderDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe indicar el motivo de cancelación");
                return RedirectToAction(nameof(Details), new { id = dto.ServiceOrderId });
            }

            Response<object> response = await _serviceOrdersService.CancelAsync(dto);

            if (response.IsSuccess)
            {
                _notyfService.Success(response.Message);
            }
            else
            {
                _notyfService.Error(response.Message);
            }

            return RedirectToAction(nameof(Details), new { id = dto.ServiceOrderId });
        }

        private async Task<bool> LoadTechniciansAsync(Guid? selectedId = null)
        {
            Response<List<UserDTO>> response = await _usersService.GetTechniciansAsync();

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return false;
            }

            ViewBag.Technicians = response.Result.Select(tech => new SelectListItem
            {
                Value = tech.Id.ToString(),
                Text = tech.FullName,
                Selected = tech.Id == selectedId
            }).ToList();

            return true;
        }

        private static List<SelectListItem> GetStatusSelectList(OrderStatus? selectedStatus)
        {
            return Enum.GetValues<OrderStatus>().Select(status => new SelectListItem
            {
                Value = ((int)status).ToString(),
                Text = status.ToString(),
                Selected = status == selectedStatus
            }).ToList();
        }

        // Displays reception correction form before diagnosis is recorded
        [HttpGet]
        [HasPermission(PermissionsCatalog.Orders.Create)]
        public async Task<IActionResult> EditReception([FromRoute] Guid id)
        {
            Response<ServiceOrderDetailDTO> response = await _serviceOrdersService.GetOneAsync(id);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction(nameof(Index));
            }

            if (!string.IsNullOrWhiteSpace(response.Result.Diagnosis) || OrderStatusRules.IsFinal(response.Result.Status))
            {
                _notyfService.Error("Solo se puede corregir la recepción antes del diagnóstico");
                return RedirectToAction(nameof(Details), new { id });
            }

            UpdateReceptionDTO dto = new UpdateReceptionDTO
            {
                Id = response.Result.Id,
                ReportedIssue = response.Result.ReportedIssue,
                Accessories = response.Result.Accessories
            };

            return View(dto);
        }

        // Persists reception corrections before diagnosis starts
        [HttpPost, ValidateAntiForgeryToken]
        [HasPermission(PermissionsCatalog.Orders.Create)]
        public async Task<IActionResult> EditReception([FromForm] UpdateReceptionDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                return View(dto);
            }

            Response<object> response = await _serviceOrdersService.UpdateReceptionAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Details), new { id = dto.Id });
        }
    }
}