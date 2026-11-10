using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Authorization;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.DTOs.Rol;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Controllers
{
    public class RolesController : Controller
    {
        private readonly INotyfService _notyfService;
        private readonly IRolesService _rolesService;

        public RolesController(INotyfService notyfService, IRolesService rolesService)
        {
            _notyfService = notyfService;
            _rolesService = rolesService;
        }

        [HttpGet]
        [TienePermiso(PermisosCatalogo.Roles.Ver)]
        public async Task<IActionResult> Index([FromQuery] PaginationRequest request)
        {
            Response<PaginationResponse<RolDTO>> response = await _rolesService.GetPaginationAsync(request);
            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction("Index", "Home");
            }
            return View(response.Result);
        }

        [HttpGet]
        [TienePermiso(PermisosCatalogo.Roles.Crear)]
        public async Task<IActionResult> Create()
        {
            if (!await LoadPermisosAsync()) return RedirectToAction(nameof(Index));
            return View(new CreateRolDTO());
        }

        [HttpPost, ValidateAntiForgeryToken]
        [TienePermiso(PermisosCatalogo.Roles.Crear)]
        public async Task<IActionResult> Create([FromForm] CreateRolDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                await LoadPermisosAsync();
                return View(dto);
            }
            Response<RolDTO> response = await _rolesService.CreateAsync(dto);
            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                await LoadPermisosAsync();
                return View(dto);
            }
            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [TienePermiso(PermisosCatalogo.Roles.Editar)]
        public async Task<IActionResult> Edit([FromRoute] Guid id)
        {
            Response<RolDTO> response = await _rolesService.GetOneAsync(id);
            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction(nameof(Index));
            }
            UpdateRolDTO dto = new()
            {
                Id = response.Result.Id,
                Nombre = response.Result.Nombre,
                Descripcion = response.Result.Descripcion,
                PermisoIds = response.Result.Permisos.Select(p => p.Id).ToList(),
            };
            if (!await LoadPermisosAsync()) return RedirectToAction(nameof(Index));
            return View(dto);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [TienePermiso(PermisosCatalogo.Roles.Editar)]
        public async Task<IActionResult> Edit([FromForm] UpdateRolDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                await LoadPermisosAsync();
                return View(dto);
            }
            Response<RolDTO> response = await _rolesService.UpdateAsync(dto);
            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                await LoadPermisosAsync();
                return View(dto);
            }
            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        [TienePermiso(PermisosCatalogo.Roles.Eliminar)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            Response<object> response = await _rolesService.DeleteAsync(id);
            if (response.IsSuccess) _notyfService.Success(response.Message);
            else _notyfService.Error(response.Message);
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> LoadPermisosAsync()
        {
            Response<List<PermisoDTO>> response = await _rolesService.GetPermisosAsync();
            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return false;
            }
            ViewBag.Permisos = response.Result;
            return true;
        }
    }
}
