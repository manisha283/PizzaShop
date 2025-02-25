using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace PizzaShop.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;

        public DashboardController(IUserService userService, IJwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        public async Task<IActionResult> MyProfile()
        {
            var token = Request.Cookies["authToken"];
            var email = _jwtService.GetClaimValue(token, "email");

            var model = await _userService.GetMyProfileAsync(email);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MyProfile(MyProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var isUpdated = await _userService.UpdateProfileAsync(model);
            if (!isUpdated) return View(model);

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("ChangePassword");
        }

        public IActionResult ChangePassword()
        {
            var token = Request.Cookies["authToken"];
            var email = _jwtService.GetClaimValue(token, "email");
            ViewBag.email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var isPasswordChanged = await _userService.ChangePasswordAsync(model);
            if (!isPasswordChanged) return View(model);

            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToAction("MyProfile");
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete("authToken");
            return RedirectToAction("Login", "Home");
        }
    }
}

