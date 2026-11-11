using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Authorization;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.DTOs.Role;
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
        [HasPermission(PermissionsCatalog.Roles.Show)]
        public async Task<IActionResult> Index([FromQuery] PaginationRequest request)
        {
            Response<PaginationResponse<RoleDTO>> response = await _rolesService.GetPaginationAsync(request);
            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction("Index", "Home");
            }
            return View(response.Result);
        }

        [HttpGet]
        [HasPermission(PermissionsCatalog.Roles.Create)]
        public async Task<IActionResult> Create()
        {
            if (!await LoadPermissionsAsync()) return RedirectToAction(nameof(Index));
            return View(new CreateRoleDTO());
        }

        [HttpPost, ValidateAntiForgeryToken]
        [HasPermission(PermissionsCatalog.Roles.Create)]
        public async Task<IActionResult> Create([FromForm] CreateRoleDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                await LoadPermissionsAsync();
                return View(dto);
            }
            Response<RoleDTO> response = await _rolesService.CreateAsync(dto);
            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                await LoadPermissionsAsync();
                return View(dto);
            }
            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [HasPermission(PermissionsCatalog.Roles.Update)]
        public async Task<IActionResult> Edit([FromRoute] Guid id)
        {
            Response<RoleDTO> response = await _rolesService.GetOneAsync(id);
            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction(nameof(Index));
            }
            UpdateRoleDTO dto = new()
            {
                Id = response.Result.Id,
                Name = response.Result.Name,
                Description = response.Result.Description,
                PermissionIds = response.Result.Permissions.Select(permission => permission.Id).ToList(),
            };
            if (!await LoadPermissionsAsync()) return RedirectToAction(nameof(Index));
            return View(dto);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [HasPermission(PermissionsCatalog.Roles.Update)]
        public async Task<IActionResult> Edit([FromForm] UpdateRoleDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                await LoadPermissionsAsync();
                return View(dto);
            }
            Response<RoleDTO> response = await _rolesService.UpdateAsync(dto);
            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                await LoadPermissionsAsync();
                return View(dto);
            }
            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        [HasPermission(PermissionsCatalog.Roles.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            Response<object> response = await _rolesService.DeleteAsync(id);
            if (response.IsSuccess) _notyfService.Success(response.Message);
            else _notyfService.Error(response.Message);
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> LoadPermissionsAsync()
        {
            Response<List<PermissionDTO>> response = await _rolesService.GetPermissionsAsync();
            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return false;
            }
            ViewBag.Permissions = response.Result;
            return true;
        }
    }
}
