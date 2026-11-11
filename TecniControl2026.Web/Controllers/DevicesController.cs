using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.DTOs.Customer;
using TecniControl2026.Web.DTOs.Device;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Controllers
{
    public class DevicesController : Controller
    {
        private readonly INotyfService _notyfService;
        private readonly IDevicesService _devicesService;
        private readonly ICustomersService _customersService;

        public DevicesController(
            INotyfService notyfService,
            IDevicesService devicesService,
            ICustomersService customersService)
        {
            _notyfService = notyfService;
            _devicesService = devicesService;
            _customersService = customersService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] PaginationRequest request)
        {
            Response<PaginationResponse<DeviceDTO>> response = await _devicesService.GetPaginationAsync(request);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction("Index", "Home");
            }

            return View(response.Result);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!await LoadCustomersAsync())
            {
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateDeviceDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                await LoadCustomersAsync(dto.CustomerId);
                return View(dto);
            }

            Response<CreateDeviceDTO> response = await _devicesService.CreateAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                await LoadCustomersAsync(dto.CustomerId);
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit([FromRoute] Guid id)
        {
            Response<DeviceDTO> response = await _devicesService.GetOneAsync(id);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction(nameof(Index));
            }

            UpdateDeviceDTO dto = new UpdateDeviceDTO
            {
                Id = response.Result.Id,
                Type = response.Result.Type,
                Brand = response.Result.Brand,
                DeviceModel = response.Result.DeviceModel,
                SerialNumber = response.Result.SerialNumber,
                CustomerId = response.Result.CustomerId
            };

            if (!await LoadCustomersAsync(dto.CustomerId))
            {
                return RedirectToAction(nameof(Index));
            }

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromForm] UpdateDeviceDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                await LoadCustomersAsync(dto.CustomerId);
                return View(dto);
            }

            Response<DeviceDTO> response = await _devicesService.UpdateAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                await LoadCustomersAsync(dto.CustomerId);
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            Response<object> response = await _devicesService.DeleteAsync(id);

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

        private async Task<bool> LoadCustomersAsync(Guid? selectedId = null)
        {
            Response<List<CustomerDTO>> response = await _customersService.GetActiveAsync();

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return false;
            }

            ViewBag.Customers = response.Result.Select(customer => new SelectListItem
            {
                Value = customer.Id.ToString(),
                Text = $"{customer.Name} - {customer.Document}",
                Selected = customer.Id == selectedId
            }).ToList();

            return true;
        }
    }
}
