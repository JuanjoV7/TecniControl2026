using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using TecniControl2026.Web.Core;
using TecniControl2026.Web.DTOs.Account;
using TecniControl2026.Web.Services.Abstractions;

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
                Response<object> response = await _accountService.LoginAsync(dto);

                if (response.IsSuccess)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, response.Message!);
                _notyfService.Error(response.Message!);
            }
            else
            {
                _notyfService.Error("Debe ajustar los errores de validación");
            }

            return View(dto);
        }
    }
}
