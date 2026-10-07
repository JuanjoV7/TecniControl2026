using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.DTOs.Cliente;
using TecniControl2026.Web.DTOs.Equipo;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Controllers
{
    public class EquiposController : Controller
    {
        private readonly INotyfService _notyfService;
        private readonly IEquiposService _equiposService;
        private readonly IClientesService _clientesService;

        public EquiposController(
            INotyfService notyfService,
            IEquiposService equiposService,
            IClientesService clientesService)
        {
            _notyfService = notyfService;
            _equiposService = equiposService;
            _clientesService = clientesService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] PaginationRequest request)
        {
            Response<PaginationResponse<EquipoDTO>> response = await _equiposService.GetPaginationAsync(request);

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
            if (!await LoadClientesAsync())
            {
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateEquipoDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                await LoadClientesAsync(dto.ClienteId);
                return View(dto);
            }

            Response<CreateEquipoDTO> response = await _equiposService.CreateAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                await LoadClientesAsync(dto.ClienteId);
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit([FromRoute] Guid id)
        {
            Response<EquipoDTO> response = await _equiposService.GetOneAsync(id);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction(nameof(Index));
            }

            UpdateEquipoDTO dto = new UpdateEquipoDTO
            {
                Id = response.Result.Id,
                Tipo = response.Result.Tipo,
                Marca = response.Result.Marca,
                Modelo = response.Result.Modelo,
                Serial = response.Result.Serial,
                ClienteId = response.Result.ClienteId
            };

            if (!await LoadClientesAsync(dto.ClienteId))
            {
                return RedirectToAction(nameof(Index));
            }

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromForm] UpdateEquipoDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                await LoadClientesAsync(dto.ClienteId);
                return View(dto);
            }

            Response<EquipoDTO> response = await _equiposService.UpdateAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                await LoadClientesAsync(dto.ClienteId);
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            Response<object> response = await _equiposService.DeleteAsync(id);

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

        private async Task<bool> LoadClientesAsync(Guid? selectedId = null)
        {
            Response<List<ClienteDTO>> response = await _clientesService.GetActiveAsync();

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return false;
            }

            ViewBag.Clientes = response.Result.Select(cliente => new SelectListItem
            {
                Value = cliente.Id.ToString(),
                Text = $"{cliente.Nombre} - {cliente.Documento}",
                Selected = cliente.Id == selectedId
            }).ToList();

            return true;
        }
    }
}
