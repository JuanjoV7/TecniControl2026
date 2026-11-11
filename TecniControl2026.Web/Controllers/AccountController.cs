using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TecniControl2026.Web.Core;
using TecniControl2026.Web.DTOs.Account;
using TecniControl2026.Web.Services.Abstractions;
using MySignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace TecniControl2026.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly INotyfService _notyfService;

        public AccountController(IAccountService accountService, INotyfService notyfService)
        {
            _accountService = accountService;
            _notyfService = notyfService;
        }

        [HttpGet, AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDTO dto, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                return View(dto);
            }

            Response<MySignInResult> result = await _accountService.LoginAsync(dto);

            if (result.IsSuccess)
            {
                // Only local URLs are accepted, to avoid open redirects
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            string message = result.Result?.IsLockedOut == true
                ? "Cuenta bloqueada temporalmente por demasiados intentos fallidos. Intente de nuevo en unos minutos"
                : "Email o contraseña incorrectos";

            ModelState.AddModelError(string.Empty, message);
            _notyfService.Error(message);

            return View(dto);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutAsync();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            Response<ProfileDTO> response = await _accountService.GetProfileAsync();

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction("Index", "Home");
            }

            return View(response.Result);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile([FromForm] ProfileDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                return View(await WithReadOnlyDataAsync(dto));
            }

            Response<ProfileDTO> response = await _accountService.UpdateProfileAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                return View(await WithReadOnlyDataAsync(dto));
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                return View(dto);
            }

            Response<object> response = await _accountService.ChangePasswordAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);

                foreach (string error in response.Errors ?? new List<string>())
                {
                    ModelState.AddModelError(string.Empty, error);
                }

                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Profile));
        }


        [HttpGet, AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword([FromForm] ForgotPasswordDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                return View(dto);
            }

            string resetUrl = Url.Action(nameof(ResetPassword), "Account", values: null, protocol: Request.Scheme)!;

            Response<object> response = await _accountService.ForgotPasswordAsync(dto, resetUrl);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                return View(dto);
            }

            // The same message is shown whether or not the email exists
            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet, AllowAnonymous]
        public IActionResult ResetPassword([FromQuery] string? email, [FromQuery] string? token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            {
                _notyfService.Error("El enlace de recuperación no es válido o ya expiró");
                return RedirectToAction(nameof(Login));
            }

            return View(new ResetPasswordDTO { Email = email, Token = token });
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                return View(dto);
            }

            Response<object> response = await _accountService.ResetPasswordAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);

                foreach (string error in response.Errors ?? new List<string>())
                {
                    ModelState.AddModelError(string.Empty, error);
                }

                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Login));
        }

        // Email, document and role are not posted by the form; they are reloaded to redisplay the view
        private async Task<ProfileDTO> WithReadOnlyDataAsync(ProfileDTO dto)
        {
            Response<ProfileDTO> current = await _accountService.GetProfileAsync();

            if (current.IsSuccess && current.Result is not null)
            {
                dto.Email = current.Result.Email;
                dto.Document = current.Result.Document;
                dto.RoleName = current.Result.RoleName;
            }

            return dto;
        }
    }
}
