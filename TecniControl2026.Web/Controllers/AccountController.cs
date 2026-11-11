using AspNetCoreHero.ToastNotification.Abstractions;
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

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            if (ModelState.IsValid)
            {
                Response<MySignInResult> result = await _accountService.LoginAsync(dto);

                if (result.IsSuccess)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos");
                _notyfService.Error("Email o contraseña incorrectos");
            }
            else
            {
                _notyfService.Error("Debe ajustar los errores de validación");
            }

            return View(dto);
        }
    }
}