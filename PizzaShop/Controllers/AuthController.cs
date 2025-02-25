using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.ViewModel;

namespace PizzaShop.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        public IActionResult Login()
        {
            if (Request.Cookies["emailCookie"] != null)
                return RedirectToAction("MyProfile", "Dashboard");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var success = await _authService.LoginAsync(model.Email, model.Password, model.RememberMe);
            if (success)
                return RedirectToAction("MyProfile", "Dashboard");

            ViewBag.Error = "Invalid credentials";
            return View(model);
        }

        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            await _authService.ForgotPasswordAsync(model.Email);
            return RedirectToAction("Privacy");
        }

        public IActionResult ResetPassword(string email)
        {
            ViewBag.email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Privacy");
            } 

            var success = await _authService.ResetPasswordAsync(model.Email, model.NewPassword, model.ConfirmPassword);
            if (success)
            {
                return RedirectToAction("Login");
            } 

            ViewBag.Error = "Error resetting password";
            return View(model);
        }
    }
}

