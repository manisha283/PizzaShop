using PizzaShop.Repository.Interfaces;
using PizzaShop.Entity.Models;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Service.Helpers;
using PizzaShop.Service.Interfaces;
using Microsoft.AspNetCore.Http;


namespace PizzaShop.Service.Services;
public class ProfileService : IProfileService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<Role> _roleRepository;
    private readonly IAddressService _addressService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProfileService(IGenericRepository<User> userRepository, IAddressService addressService, IGenericRepository<Role> roleRepository, IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _addressService = addressService;
        _roleRepository = roleRepository;
        _httpContextAccessor = httpContextAccessor;

    }

    /*-----------------------------------------------------------------My Profile---------------------------------------------------------------------------------
    ----------------------------------------------------------------------------------------------------------------------------------------------------------*/
    #region My Profile


    public async Task<MyProfileViewModel> GetMyProfileAsync(string email)
    {
        var user = await _userRepository.GetByStringAsync(u => u.Email == email);
        if (user == null)
            return null;

        var role = await _roleRepository.GetByIdAsync(user.RoleId);

        MyProfileViewModel userProfile =  new MyProfileViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = role.Name,
            UserName = user.Username,
            Email = user.Email,
            Phone = user.Phone,
            CountryId = user.CountryId,
            StateId = user.StateId,
            CityId = user.CityId,
            Address = user.Address,
            ZipCode = user.ZipCode,
            ProfileImageUrl = user.ProfileImg,
            Countries = _addressService.GetCountries(),
            States = _addressService.GetStates(user.CountryId),
            Cities = _addressService.GetCities(user.StateId)
        };

        return userProfile;
    }

#endregion

/*---------------------------------------------------------------Update Profile---------------------------------------------------------------------------------
----------------------------------------------------------------------------------------------------------------------------------------------------------*/
#region Update Profile

    public async Task<bool> UpdateProfileAsync(MyProfileViewModel model)
    {
        var user = await _userRepository.GetByStringAsync(u => u.Email == model.Email);
        
        if (user == null) 
            return false;

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Username = model.UserName;
        user.Phone = model.Phone;
        user.CountryId = model.CountryId;
        user.StateId = model.StateId;
        user.CityId = model.CityId;
        user.Address = model.Address;
        user.ZipCode = model.ZipCode;

        // Handle Image Upload
        if (model.Image != null)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string fileName = $"{Guid.NewGuid()}_{model.Image.FileName}";
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.Image.CopyToAsync(stream);
            }

            user.ProfileImg = $"/uploads/{fileName}";
        }

        _httpContextAccessor.HttpContext.Response.Cookies.Delete("userName");
        _httpContextAccessor.HttpContext.Response.Cookies.Delete("profileImg");

        CookieOptions options = new CookieOptions
        {
            Expires = DateTime.Now.AddDays(1),
            HttpOnly = true,
            IsEssential = true
        };
        _httpContextAccessor.HttpContext.Response.Cookies.Append("userName", user.Username, options);
        _httpContextAccessor.HttpContext.Response.Cookies.Append("profileImg", user.ProfileImg, options);
        
        return await _userRepository.UpdateAsync(user);
    }

#endregion

/*---------------------------------------------------------------Change Password---------------------------------------------------------------------------------
----------------------------------------------------------------------------------------------------------------------------------------------------------*/
#region Change Password

    public async Task<bool> ChangePasswordAsync(ChangePasswordViewModel model)
    {
        User user = await _userRepository.GetByStringAsync(u => u.Email == model.Email);

        if (user == null) 
            return false;

        bool verified = PasswordHelper.VerifyPassword(model.OldPassword, user.Password);
            
        if(verified)
        {
            user.Password = PasswordHelper.HashPassword(model.NewPassword);
            return await _userRepository.UpdateAsync(user);
        }
        
        return verified;
    }
    
#endregion

}

