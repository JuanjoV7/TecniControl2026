using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Authorization;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.DTOs.Role;
using TecniControl2026.Web.DTOs.User;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Controllers
{
    public class UsersController : Controller
    {
        private readonly INotyfService _notyfService;
        private readonly IUsersService _usersService;
        private readonly IRolesService _rolesService;
        private readonly ICurrentUserService _currentUser;

        public UsersController(
            INotyfService notyfService,
            IUsersService usersService,
            IRolesService rolesService,
            ICurrentUserService currentUser)
        {
            _notyfService = notyfService;
            _usersService = usersService;
            _rolesService = rolesService;
            _currentUser = currentUser;
        }

        [HttpGet]
        [HasPermission(PermissionsCatalog.Users.Show)]
        public async Task<IActionResult> Index([FromQuery] PaginationRequest request)
        {
            Response<PaginationResponse<UserDTO>> response = await _usersService.GetPaginationAsync(request);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.CurrentUserId = _currentUser.UserId;

            return View(response.Result);
        }

        [HttpGet]
        [HasPermission(PermissionsCatalog.Users.Create)]
        public async Task<IActionResult> Create()
        {
            if (!await LoadRolesAsync())
            {
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        [HasPermission(PermissionsCatalog.Users.Create)]
        public async Task<IActionResult> Create([FromForm] CreateUserDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                await LoadRolesAsync(dto.RoleId);
                return View(dto);
            }

            Response<UserDTO> response = await _usersService.CreateAsync(dto);

            if (!response.IsSuccess)
            {
                ShowErrors(response);
                await LoadRolesAsync(dto.RoleId);
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [HasPermission(PermissionsCatalog.Users.Update)]
        public async Task<IActionResult> Edit([FromRoute] Guid id)
        {
            Response<UserDTO> response = await _usersService.GetOneAsync(id);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction(nameof(Index));
            }

            UpdateUserDTO dto = new UpdateUserDTO
            {
                Id = response.Result.Id,
                Document = response.Result.Document,
                FirstName = response.Result.FirstName,
                LastName = response.Result.LastName,
                Email = response.Result.Email,
                PhoneNumber = response.Result.PhoneNumber,
                RoleId = response.Result.RoleId,
            };

            if (!await LoadRolesAsync(dto.RoleId))
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.IsCurrentUser = dto.Id == _currentUser.UserId;

            return View(dto);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [HasPermission(PermissionsCatalog.Users.Update)]
        public async Task<IActionResult> Edit([FromForm] UpdateUserDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                await LoadRolesAsync(dto.RoleId);
                ViewBag.IsCurrentUser = dto.Id == _currentUser.UserId;
                return View(dto);
            }

            Response<UserDTO> response = await _usersService.UpdateAsync(dto);

            if (!response.IsSuccess)
            {
                ShowErrors(response);
                await LoadRolesAsync(dto.RoleId);
                ViewBag.IsCurrentUser = dto.Id == _currentUser.UserId;
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        [HasPermission(PermissionsCatalog.Users.Update)]
        public async Task<IActionResult> Toggle([FromForm] ToggleUserStatusDTO dto)
        {
            Response<object> response = await _usersService.ToggleAsync(dto);

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

        private void ShowErrors(Response<UserDTO> response)
        {
            _notyfService.Error(response.Message);

            // Identity errors (e.g. password rules) are shown in the form summary
            foreach (string error in response.Errors ?? new List<string>())
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        private async Task<bool> LoadRolesAsync(Guid? selectedId = null)
        {
            Response<List<RoleDTO>> response = await _rolesService.GetAllAsync();

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return false;
            }

            ViewBag.Roles = response.Result.Select(role => new SelectListItem
            {
                Value = role.Id.ToString(),
                Text = role.Name,
                Selected = role.Id == selectedId
            }).ToList();

            return true;
        }
    }
}
