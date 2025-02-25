using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.ViewModels;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using System.Threading.Tasks;
using BCrypt.Net;

namespace BusinessLogicLayer.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUserRepository _userRepository;

        public ProfileService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<MyProfileViewModel> GetMyProfileAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null) return null;

            return new MyProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                CountryId = user.CountryId,
                StateId = user.StateId,
                CityId = user.CityId,
                Address = user.Address,
                ZipCode = user.ZipCode,
                ProfileImageUrl = user.ProfileImg
            };
        }

        public async Task<bool> UpdateProfileAsync(MyProfileViewModel model)
        {
            var user = await _userRepository.GetUserByEmailAsync(model.Email);
            if (user == null) return false;

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Username = model.UserName;
            user.Phone = model.Phone;
            user.CountryId = model.CountryId;
            user.StateId = model.StateId;
            user.CityId = model.CityId;
            user.Address = model.Address;
            user.ZipCode = model.ZipCode;

            return await _userRepository.UpdateUserAsync(user);
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordViewModel model)
        {
            var user = await _userRepository.GetUserByEmailAsync(model.Email);
            if (user == null) return false;

            bool verified = BCrypt.Net.BCrypt.Verify(model.OldPassword, user.Password);
            if (!verified) return false;

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            return await _userRepository.UpdateUserAsync(user);
        }
    }
}

