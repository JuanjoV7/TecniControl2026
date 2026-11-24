using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Authorization;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.Data.Enums;
using TecniControl2026.Web.DTOs.Customer;
using TecniControl2026.Web.DTOs.Device;
using TecniControl2026.Web.DTOs.FollowUp;
using TecniControl2026.Web.DTOs.ServiceOrder;
using TecniControl2026.Web.DTOs.User;
using TecniControl2026.Web.Models;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Controllers
{
    public class OrdersController : Controller
    {
        private readonly INotyfService _notyfService;
        private readonly IServiceOrdersService _ordersService;
        private readonly IFollowUpsService _followUpsService;
        private readonly ICustomersService _customersService;
        private readonly IDevicesService _devicesService;
        private readonly IUsersService _usersService;
        private readonly ICurrentUserService _currentUser;

        public OrdersController(
            INotyfService notyfService,
            IServiceOrdersService ordersService,
            IFollowUpsService followUpsService,
            ICustomersService customersService,
            IDevicesService devicesService,
            IUsersService usersService,
            ICurrentUserService currentUser)
        {
            _notyfService = notyfService;
            _ordersService = ordersService;
            _followUpsService = followUpsService;
            _customersService = customersService;
            _devicesService = devicesService;
            _usersService = usersService;
            _currentUser = currentUser;
        }



        // Whoever has orders.showAll sees everything; whoever has orders.showAssigned sees only
        // their own orders. The service applies that filter and rejects users with neither.
        [HttpGet, Authorize]
        public async Task<IActionResult> Index([FromQuery] OrdersPaginationRequest request)
        {
            Response<PaginationResponse<ServiceOrderDTO>> response = await _ordersService.GetPaginationAsync(request);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Status = request.Status;
            ViewBag.CurrentUserId = _currentUser.UserId;
            ViewBag.CanSeeAll = await Has(PermissionsCatalog.Orders.ShowAll);

            return View(response.Result);
        }

 

        [HttpGet, Authorize]
        public async Task<IActionResult> Details([FromRoute] Guid id)
        {
            Response<ServiceOrderDetailDTO> response = await _ordersService.GetOneAsync(id);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction(nameof(Index));
            }

            return View(await BuildDetailsAsync(response.Result));
        }

        

        [HttpGet]
        [HasPermission(PermissionsCatalog.Orders.Create)]
        public async Task<IActionResult> Create([FromQuery] Guid? deviceId)
        {
            if (!await LoadCreateFormAsync(deviceId))
            {
                return RedirectToAction(nameof(Index));
            }

            return View(new CreateServiceOrderDTO { DeviceId = deviceId ?? Guid.Empty });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [HasPermission(PermissionsCatalog.Orders.Create)]
        public async Task<IActionResult> Create([FromForm] CreateServiceOrderDTO dto)
        {
            // The technician can only be chosen by whoever has the assign permission
            if (!await Has(PermissionsCatalog.Orders.AssignTechnician))
            {
                dto.TechnicianId = null;
            }

            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                await LoadCreateFormAsync(dto.DeviceId);
                return View(dto);
            }

            Response<ServiceOrderDTO> response = await _ordersService.CreateAsync(dto);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                await LoadCreateFormAsync(dto.DeviceId);
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Details), new { id = response.Result.Id });
        }

        // Devices of a customer, to fill the device select of the create form
        [HttpGet]
        [HasPermission(PermissionsCatalog.Orders.Create)]
        public async Task<IActionResult> DevicesByCustomer([FromQuery] Guid customerId)
        {
            Response<List<DeviceDTO>> response = await _devicesService.GetByCustomerAsync(customerId);

            if (!response.IsSuccess || response.Result is null)
            {
                return BadRequest(new { message = response.Message });
            }

            return Json(response.Result.Select(d => new { id = d.Id, text = d.Description }));
        }

       

        [HttpPost, ValidateAntiForgeryToken]
        [HasPermission(PermissionsCatalog.Orders.AssignTechnician)]
        public Task<IActionResult> AssignTechnician([FromForm] AssignTechnicianDTO dto)
        {
            return ExecuteAsync(dto.ServiceOrderId, () => _ordersService.AssignTechnicianAsync(dto));
        }

       

        [HttpGet]
        [HasPermission(PermissionsCatalog.Orders.Attend)]
        public async Task<IActionResult> Attend([FromRoute] Guid id)
        {
            Response<ServiceOrderDetailDTO> response = await _ordersService.GetOneAsync(id);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction(nameof(Index));
            }

            ServiceOrderDetailDTO order = response.Result;
            string? problem = await AttendProblemAsync(order);

            if (problem is not null)
            {
                _notyfService.Error(problem);
                return RedirectToAction(nameof(Details), new { id });
            }

            List<OrderStatus> allowed = AllowedAttentionStatuses(order.Status);

            RegisterAttentionDTO dto = new RegisterAttentionDTO
            {
                ServiceOrderId = order.Id,
                Diagnosis = order.Diagnosis,
                WorkPerformed = order.WorkPerformed,
                Status = allowed.Contains(order.Status) ? order.Status : allowed.First()
            };

            FillAttendForm(order, allowed);

            return View(dto);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [HasPermission(PermissionsCatalog.Orders.Attend)]
        public async Task<IActionResult> Attend([FromForm] RegisterAttentionDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                return await RenderAttendAsync(dto);
            }

            Response<object> response = await _ordersService.RegisterAttentionAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                return await RenderAttendAsync(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Details), new { id = dto.ServiceOrderId });
        }

      

        [HttpPost, ValidateAntiForgeryToken]
        [HasPermission(PermissionsCatalog.Orders.Deliver)]
        public Task<IActionResult> Deliver([FromForm] DeliverOrderDTO dto)
        {
            return ExecuteAsync(dto.ServiceOrderId, () => _ordersService.DeliverAsync(dto));
        }

       

        [HttpPost, ValidateAntiForgeryToken]
        [HasPermission(PermissionsCatalog.Orders.Cancel)]
        public Task<IActionResult> Cancel([FromForm] CancelOrderDTO dto)
        {
            return ExecuteAsync(dto.ServiceOrderId, () => _ordersService.CancelAsync(dto));
        }



        [HttpPost, ValidateAntiForgeryToken]
        [HasPermission(PermissionsCatalog.FollowUps.Create)]
        public async Task<IActionResult> AddFollowUp([FromForm] CreateFollowUpDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error(FirstValidationError());
                return RedirectToAction(nameof(Details), new { id = dto.ServiceOrderId });
            }

            Response<FollowUpDTO> response = await _followUpsService.CreateAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
            }
            else
            {
                _notyfService.Success(response.Message);
            }

            return RedirectToAction(nameof(Details), new { id = dto.ServiceOrderId });
        }

     

        // Runs an action of the detail page (assign, deliver, cancel), shows the result and goes back to the detail
        private async Task<IActionResult> ExecuteAsync(Guid orderId, Func<Task<Response<object>>> action)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error(FirstValidationError());
                return RedirectToAction(nameof(Details), new { id = orderId });
            }

            Response<object> response = await action();

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
            }
            else
            {
                _notyfService.Success(response.Message);
            }

            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        // Redirects lose the ModelState, so the first error is shown in the notification
        private string FirstValidationError()
        {
            string? error = ModelState.Values.SelectMany(v => v.Errors)
                                             .Select(e => e.ErrorMessage)
                                             .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));

            return error ?? "Debe ajustar los errores de validación";
        }

        private Task<bool> Has(string permission)
        {
            return _currentUser.HasPermissionAsync(permission);
        }

        private async Task<OrderDetailsViewModel> BuildDetailsAsync(ServiceOrderDetailDTO order)
        {
            bool isFinal = OrderStatusRules.IsFinal(order.Status);
            Guid? userId = _currentUser.UserId;

            OrderDetailsViewModel model = new OrderDetailsViewModel
            {
                Order = order,
                CanAssignTechnician = !isFinal && await Has(PermissionsCatalog.Orders.AssignTechnician),
                CanDeliver = order.Status == OrderStatus.ReadyForDelivery && await Has(PermissionsCatalog.Orders.Deliver),
                CanCancel = !isFinal && await Has(PermissionsCatalog.Orders.Cancel),
                CanAddFollowUp = await Has(PermissionsCatalog.FollowUps.Create),
            };

            // Same rule as the service: the assigned technician, or whoever can see all orders
            model.CanAttend = !isFinal
                              && order.TechnicianId is not null
                              && await Has(PermissionsCatalog.Orders.Attend)
                              && (order.TechnicianId == userId || await Has(PermissionsCatalog.Orders.ShowAll));

            if (model.CanAssignTechnician)
            {
                Response<List<UserDTO>> technicians = await _usersService.GetTechniciansAsync();

                if (technicians.IsSuccess && technicians.Result is not null)
                {
                    model.Technicians = technicians.Result.Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = t.FullName,
                        Selected = t.Id == order.TechnicianId
                    }).ToList();
                }
            }

            return model;
        }

        private async Task<bool> LoadCreateFormAsync(Guid? deviceId)
        {
            Response<List<CustomerDTO>> customers = await _customersService.GetActiveAsync();

            if (!customers.IsSuccess || customers.Result is null)
            {
                _notyfService.Error(customers.Message);
                return false;
            }

            Guid? customerId = null;
            List<SelectListItem> devices = new();

            // With a chosen device (from the device list or after a failed save), preload its customer and devices
            if (deviceId is not null && deviceId != Guid.Empty)
            {
                Response<DeviceDTO> device = await _devicesService.GetOneAsync(deviceId.Value);

                if (device.IsSuccess && device.Result is not null)
                {
                    customerId = device.Result.CustomerId;

                    Response<List<DeviceDTO>> customerDevices = await _devicesService.GetByCustomerAsync(customerId.Value);

                    if (customerDevices.IsSuccess && customerDevices.Result is not null)
                    {
                        devices = customerDevices.Result.Select(d => new SelectListItem
                        {
                            Value = d.Id.ToString(),
                            Text = d.Description,
                            Selected = d.Id == deviceId
                        }).ToList();
                    }
                }
            }

            ViewBag.Customers = customers.Result.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Name} - {c.Document}",
                Selected = c.Id == customerId
            }).ToList();

            ViewBag.Devices = devices;

            bool canAssign = await Has(PermissionsCatalog.Orders.AssignTechnician);
            ViewBag.CanAssignTechnician = canAssign;
            ViewBag.Technicians = new List<SelectListItem>();

            if (canAssign)
            {
                Response<List<UserDTO>> technicians = await _usersService.GetTechniciansAsync();

                if (technicians.IsSuccess && technicians.Result is not null)
                {
                    ViewBag.Technicians = technicians.Result.Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = t.FullName
                    }).ToList();
                }
            }

            return true;
        }

        // Why the current user cannot register the attention (null when they can)
        private async Task<string?> AttendProblemAsync(ServiceOrderDetailDTO order)
        {
            if (OrderStatusRules.IsFinal(order.Status))
            {
                return "La orden ya fue entregada o cancelada";
            }

            if (order.TechnicianId is null)
            {
                return "La orden no tiene un técnico asignado";
            }

            if (order.TechnicianId != _currentUser.UserId && !await Has(PermissionsCatalog.Orders.ShowAll))
            {
                return "La orden no está asignada a usted";
            }

            return null;
        }

        // The current status (to save diagnosis without moving the order) plus the allowed next ones
        private static List<OrderStatus> AllowedAttentionStatuses(OrderStatus current)
        {
            return OrderStatusRules.AttentionStatuses
                                   .Where(s => s == current || OrderStatusRules.CanChange(current, s))
                                   .ToList();
        }

        private void FillAttendForm(ServiceOrderDetailDTO order, List<OrderStatus> allowed)
        {
            ViewBag.Order = order;
            ViewBag.Statuses = allowed.Select(s => new SelectListItem
            {
                Value = s.ToString(),
                Text = OrderStatusRules.Name(s)
            }).ToList();
        }

        // Shows the attention form again after a failed save, with what the user typed
        private async Task<IActionResult> RenderAttendAsync(RegisterAttentionDTO dto)
        {
            Response<ServiceOrderDetailDTO> response = await _ordersService.GetOneAsync(dto.ServiceOrderId);

            if (!response.IsSuccess || response.Result is null)
            {
                return RedirectToAction(nameof(Index));
            }

            FillAttendForm(response.Result, AllowedAttentionStatuses(response.Result.Status));

            return View(dto);
        }
    }
}
