using BusinessLogicLayer.ViewModels;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Interfaces
{
    public interface IProfileService
    {
        Task<MyProfileViewModel> GetMyProfileAsync(string email);
        Task<bool> UpdateProfileAsync(MyProfileViewModel model);
        Task<bool> ChangePasswordAsync(ChangePasswordViewModel model);
    }
}

