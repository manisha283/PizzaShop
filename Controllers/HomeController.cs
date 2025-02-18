﻿using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PizzaShop.Models;
using PizzaShop.Services;
using PizzaShop.ViewModel;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using System.Linq;

namespace PizzaShop.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly PizzashopContext _context;
    private readonly IEmailService _emailService;

    public HomeController(ILogger<HomeController> logger, PizzashopContext context, IEmailService emailService)
    {
        _logger = logger;
        _context = context;
        _emailService = emailService;
    }

    public IActionResult Login()
    {
        if (Request.Cookies["emailCookie"] != null)
        {
            return RedirectToAction("Privacy");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        
        if (user != null && VerifyPassword(model.Password, user.Password))
        {
            CookieOptions options = new CookieOptions { Expires = DateTime.Now.AddDays(15) };

            if (model.RememberMe)
            {
                Response.Cookies.Append("emailCookie", model.Email, options);
            }
            return RedirectToAction("Privacy");
        }

        ModelState.AddModelError("", "Invalid login credentials");
        return View(model);
    }

    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (user == null)
        {
            return RedirectToAction("ForgotPasswordConfirmation");
        }

        // Generate URL-safe Reset Token
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = WebEncoders.Base64UrlEncode(tokenBytes);
        
        user.ResetToken = token;
        user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        var resetLink = Url.Action("ResetPassword", "Home", new { token }, Request.Scheme);

        string body = $@"<div>
            <p>Please click <a href='{resetLink}'>here</a> to reset your password.</p>
            <p><b>Note:</b> This link expires in 1 hour.</p>
        </div>";

        await _emailService.SendEmailAsync(user.Email, "Reset Password", body);
        return RedirectToAction("ForgotPasswordConfirmation");
    }

    public IActionResult ResetPassword(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Invalid password reset link.");
        }

        var model = new ResetPasswordViewModel { Token = token };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == model.Token);
        if (user == null || user.ResetTokenExpiry == null || user.ResetTokenExpiry < DateTime.UtcNow)
        {
            ModelState.AddModelError("", "Invalid or expired reset token.");
            return View(model);
        }

        user.Password = HashPassword(model.NewPassword);
        user.ResetToken = null;
        user.ResetTokenExpiry = null;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return RedirectToAction("ResetPasswordConfirmation");
    }

    private static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256))
        {
            byte[] hash = pbkdf2.GetBytes(32);
            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }
    }

    private static bool VerifyPassword(string enteredPassword, string storedHash)
    {
        string[] parts = storedHash.Split('.');
        if (parts.Length != 2) return false;

        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] storedHashBytes = Convert.FromBase64String(parts[1]);

        using (var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 100000, HashAlgorithmName.SHA256))
        {
            byte[] enteredHashBytes = pbkdf2.GetBytes(32);
            return enteredHashBytes.SequenceEqual(storedHashBytes);
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
