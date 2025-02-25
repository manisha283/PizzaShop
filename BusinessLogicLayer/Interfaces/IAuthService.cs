using System.Threading.Tasks;
using DataAccessLayer.Models;

namespace BusinessLogicLayer.Interfaces
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string email, string password, bool rememberMe);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string email, string newPassword, string confirmPassword);
    }
}
