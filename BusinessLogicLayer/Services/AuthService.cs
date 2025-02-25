using System;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories;
using Microsoft.AspNetCore.Http;
using BCrypt.Net;

namespace BusinessLogicLayer.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly JwtService _jwtService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(IUserRepository userRepository, IEmailService emailService, JwtService jwtService, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _jwtService = jwtService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> LoginAsync(string email, string password, bool rememberMe)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
                return false;

            var role = await _userRepository.GetUserRoleAsync(user.RoleId);
            var token = _jwtService.GenerateToken(email, role);

            CookieOptions options = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(1),
                HttpOnly = true,
                IsEssential = true
            };

            _httpContextAccessor.HttpContext.Response.Cookies.Append("authToken", token, options);
            if (rememberMe)
                _httpContextAccessor.HttpContext.Response.Cookies.Append("emailCookie", email, options);

            return true;
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null) return false;

            var resetLink = $"https://localhost:5001/Home/ResetPassword?email={email}";
            // var resetLink = Url.Action("ResetPassword","Home", new{email = model.Email},Request.Scheme);

            string body = $@"
                <div style='background-color: #F2F2F2;'>
                    <p>Please click <a href='{resetLink}'>here</a> to reset your password.</p>
                    <p>For security reasons, the link will expire in 24 hours.</p>
                </div>";

            await _emailService.SendEmailAsync(email, "Reset Password", body);
            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword) return false;

            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null) return false;

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _userRepository.UpdateAsync(user);
            return true;
        }
    }
}
