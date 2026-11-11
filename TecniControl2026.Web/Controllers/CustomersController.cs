using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Authorization;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.DTOs.Customer;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Controllers
{
    public class CustomersController : Controller
    {
        private readonly INotyfService _notyfService;
        private readonly ICustomersService _customersService;

        public CustomersController(INotyfService notyfService, ICustomersService customersService)
        {
            _notyfService = notyfService;
            _customersService = customersService;
        }

        [HttpGet]
        [HasPermission(PermissionsCatalog.Customers.Show)]
        public async Task<IActionResult> Index([FromQuery] PaginationRequest request)
        {
            Response<PaginationResponse<CustomerDTO>> response = await _customersService.GetPaginationAsync(request);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction("Index", "Home");
            }

            return View(response.Result);
        }

        [HttpGet]
        [HasPermission(PermissionsCatalog.Customers.Create)]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        [HasPermission(PermissionsCatalog.Customers.Create)]
        public async Task<IActionResult> Create([FromForm] CreateCustomerDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                return View(dto);
            }

            Response<CreateCustomerDTO> response = await _customersService.CreateAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [HasPermission(PermissionsCatalog.Customers.Update)]
        public async Task<IActionResult> Edit([FromRoute] Guid id)
        {
            Response<CustomerDTO> response = await _customersService.GetOneAsync(id);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction(nameof(Index));
            }

            UpdateCustomerDTO dto = new UpdateCustomerDTO
            {
                Id = response.Result.Id,
                Document = response.Result.Document,
                Name = response.Result.Name,
                Phone = response.Result.Phone,
                Email = response.Result.Email,
                IsActive = response.Result.IsActive
            };

            return View(dto);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [HasPermission(PermissionsCatalog.Customers.Update)]
        public async Task<IActionResult> Edit([FromForm] UpdateCustomerDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                return View(dto);
            }

            Response<CustomerDTO> response = await _customersService.UpdateAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        [HasPermission(PermissionsCatalog.Customers.Update)]
        public async Task<IActionResult> Toggle(ToggleCustomerStatusDTO dto)
        {
            Response<object> response = await _customersService.ToggleAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
            }
            else
            {
                _notyfService.Success(response.Message);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
