using System.ComponentModel.DataAnnotations;

using PizzaShop.Models;
using PizzaShop.Services;
using PizzaShop.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MailKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace PizzaShop.Controllers;

public class DashboardController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly PizzashopContext _context;
    private readonly JwtService _jwtService;

    public DashboardController(ILogger<HomeController> logger, PizzashopContext context, JwtService jwtService)
    {
        _logger = logger;
        _context = context;
        _jwtService = jwtService;
    }

    // Get Method for My Profile
    public IActionResult MyProfile()
    {
        // email is fetched from the token
        var token = Request.Cookies["authToken"];
        var email = _jwtService.GetClaimValue(token,"email");
        // var role = _jwtService.GetClaimValue(token,"role");

        // fetch user from database
        var user =  _context.Users.Where(u => u.Email == email).FirstOrDefault(); 

        //if user is null
        if (user == null)
        {
            return NotFound();
        }
        
        //
        MyProfileViewModel model = new MyProfileViewModel(); 
        model.FirstName = user.FirstName;
        model.LastName = user.LastName;
        model.UserName = user.Username;
        model.Email = user.Email;
        model.Phone = user.Phone;
        model.CountryId = user.CountryId;
        model.StateId = user.StateId;
        model.CityId = user.CityId;
        model.Address = user.Address;
        model.ZipCode = user.ZipCode;
        model.ProfileImageUrl = user.ProfileImg;
        model.Role = _context.Roles.Where(q=> q.Id == user.RoleId).FirstOrDefault().Name;

        //Country, State and City
        ViewBag.countries = _context.Countries.ToList();
        ViewBag.states = _context.States.Where(s => s.CountryId == user.CountryId).ToList();
        ViewBag.cities = _context.Cities.Where(c => c.StateId == user.StateId).ToList();

         
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> MyProfile(MyProfileViewModel model)
    {

        if(!ModelState.IsValid){
            return View(model);
        }

        // email is fetched from the token
        var token = Request.Cookies["authToken"];
        var email = _jwtService.GetClaimValue(token,"email");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Username = model.UserName;
        user.Phone = model.Phone;
        user.CountryId = model.CountryId;
        user.StateId = model.StateId;
        user.CityId = model.CityId;
        user.Address = model.Address;
        user.ZipCode = model.ZipCode;

        if (model.image != null)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

            //create folder if not exist
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fileName = $"{Guid.NewGuid()}_{model.image.FileName}";
            string fileNameWithPath = Path.Combine(path, fileName);

            using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
            {
                model.image.CopyTo(stream);
            }
            user.ProfileImg = $"/uploads/{fileName}";
        }

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Profile updated successfully!";
        return RedirectToAction("ChangePassword", "Dashboard");
    }

    public IActionResult ChangePassword(){
        // email is fetched from the token
        var token = Request.Cookies["authToken"];
        var email = _jwtService.GetClaimValue(token,"email");
        ViewBag.email = email;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model){

        if(!ModelState.IsValid){
            return View(model);
        }

        // fetch user from database
        var user = await _context.Users.Where(u => u.Email == model.Email).FirstOrDefaultAsync();

        //if user is null
        if(user == null)
        {
            ModelState.AddModelError("","Email not found");
        }

        bool verified = BCrypt.Net.BCrypt.Verify(model.OldPassword, user.Password);  
        if(user != null && verified){
            if(model.NewPassword == model.NewPassword)
            {
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                user.Password = passwordHash;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("MyProfile","Dashboard");
            }
            else{
                return View(model);
            }
        } 
        return View();
    }

    public async Task<IActionResult> Logout()
    {
        
        // Delete the "Remember Me" cookie
        if (Request.Cookies["emailCookie"] != null)
        {
            Response.Cookies.Delete("emailCookie");
        }

        // await this._authService.LogoutAsync();
        return RedirectToAction("Login","Home");
    }


    [HttpGet]
    public IActionResult GetCountries()
    {
        var countries = _context.Countries.ToList();
        return Json(new SelectList(countries, "Id", "Name"));
    }

    [HttpGet]
    public IActionResult GetStates(int countryId)
    {
        var states = _context.States.Where(x => x.CountryId == countryId).ToList();
        return Json(new SelectList(states, "Id", "Name"));
    }
    [HttpGet]
    public IActionResult GetCities(int stateId)
    {
        var cities = _context.Cities.Where(x => x.StateId == stateId).ToList();
        return Json(new SelectList(cities, "Id", "Name"));
    }

    // public IActionResult UsersList(){

    //     var allUsers = _context.Users.Include(x => x.Role).Select(u => new UserInfoViewModel{
    //         FirstName = u.FirstName,
    //         LastName = u.LastName,
    //         Email = u.Email,
    //         Phone = u.Phone,
    //         Role = u.Role.Name ,
    //         ProfileImageUrl = u.ProfileImg,
    //         Status =u.IsActive,
    //     }).ToList();

    //     UsersListViewModel model = new UsersListViewModel();
    //     model.User = allUsers;

    //     return View(model);
    // }

    public IActionResult UserList(string? search, int pageNumber = 1, int pageSize = 5)
    {
        var query = _context.Users.AsQueryable(); // Assuming _context is your DbContext

        // Apply search filter if provided
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.FirstName.Contains(search) || 
                                    u.LastName.Contains(search) ||
                                    u.Email.Contains(search));
                                    // u.Role.Contains(search));
        }

        // Get total record count after filtering
        int totalRecords = query.Count();

        // Apply pagination
        var users = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserInfoViewModel
            {
                ProfileImageUrl = u.ProfileImg ?? "~/images/Default_pfp.svg.png",
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Phone = u.Phone,
                // Role = u.RoleId,
                Status = u.IsActive
            })
            .ToList();

        var viewModel = new UsersListViewModel
        {
            User = users,
            SearchTerm = search,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords
        };

        return View(viewModel);
    }


    [HttpGet]
    public IActionResult AddUser(){
        ViewBag.roles = _context.Roles.ToList();
        ViewBag.countries = _context.Countries.ToList();
        ViewBag.states = _context.States.ToList();
        ViewBag.cities = _context.Cities.ToList();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddUser(MyProfileViewModel model)
    {

        if(!ModelState.IsValid){
            return View(model);
        }

        // email is fetched from the token
        var token = Request.Cookies["authToken"];
        var email = _jwtService.GetClaimValue(token,"email");

        var user = await _context.Users.FirstOrDefaultAsync();

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Username = model.UserName;
        user.Phone = model.Phone;
        user.CountryId = model.CountryId;
        user.StateId = model.StateId;
        user.CityId = model.CityId;
        user.Address = model.Address;
        user.ZipCode = model.ZipCode;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Profile updated successfully!";
        return RedirectToAction("ChangePassword", "Dashboard");
    }

//     [HttpPost]
// public async Task<IActionResult> AddUser(MyProfileViewModel model)
// {
//     if (!ModelState.IsValid)
//     {
//         ViewBag.roles = _context.Roles.ToList();
//         ViewBag.countries = _context.Countries.ToList();
//         ViewBag.states = _context.States.ToList();
//         ViewBag.cities = _context.Cities.ToList();
//         return View(model);
//     }

//     var token = Request.Cookies["authToken"];
//     var email = _jwtService.GetClaimValue(token, "email");

//     var user = new User
//     {
//         FirstName = model.FirstName,
//         LastName = model.LastName,
//         Username = model.UserName,
//         Phone = model.Phone,
//         CountryId = model.CountryId,
//         StateId = model.StateId,
//         CityId = model.CityId,
//         Address = model.Address,
//         ZipCode = model.ZipCode,
//         Email = model.Email,
//         Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
//         RoleId = model.Role
//     };

//     // Handling Profile Image Upload
//     if (model.ProfileImage != null)
//     {
//         var fileName = Path.GetFileName(model.ProfileImage.FileName);
//         var filePath = Path.Combine("wwwroot/uploads", fileName);

//         using (var stream = new FileStream(filePath, FileMode.Create))
//         {
//             await model.ProfileImage.CopyToAsync(stream);
//         }

//         user.ProfileImagePath = "/uploads/" + fileName;
//     }

//     _context.Users.Add(user);
//     await _context.SaveChangesAsync();

//     TempData["SuccessMessage"] = "User added successfully!";
//     return RedirectToAction("UsersList", "Dashboard");
// }


// [HttpGet]
// public async Task<IActionResult> EditUser(int id)
// {
//     var user = await _context.Users.FindAsync(id);
//     if (user == null) return NotFound();

//     var model = new EditUserViewModel
//     {
//         UserId = user.UserId,
//         FirstName = user.FirstName,
//         LastName = user.LastName,
//         Email = user.Email,
//         Phone = user.Phone,
//         RoleId = user.RoleId
//     };

//     ViewBag.Roles = _context.Roles.ToList();
//     return View(model);
// }

// [HttpPost]
// public async Task<IActionResult> EditUser(EditUserViewModel model)
// {
//     if (!ModelState.IsValid)
//     {
//         ViewBag.Roles = _context.Roles.ToList();
//         return View(model);
//     }

//     var user = await _context.Users.FindAsync(model.UserId);
//     if (user == null) return NotFound();

//     user.FirstName = model.FirstName;
//     user.LastName = model.LastName;
//     user.Email = model.Email;
//     user.Phone = model.Phone;
//     user.RoleId = model.RoleId;

//     if (model.ProfileImage != null)
//     {
//         var fileName = Path.GetFileName(model.ProfileImage.FileName);
//         var filePath = Path.Combine("wwwroot/uploads", fileName);
//         using (var stream = new FileStream(filePath, FileMode.Create))
//         {
//             await model.ProfileImage.CopyToAsync(stream);
//         }
//         user.ProfileImagePath = "/uploads/" + fileName;
//     }

//     _context.Users.Update(user);
//     await _context.SaveChangesAsync();
//     TempData["SuccessMessage"] = "User updated successfully!";
//     return RedirectToAction("UsersList");
// }

// [HttpPost]
// public async Task<IActionResult> DeleteUser(int id)
// {
//     var user = await _context.Users.FindAsync(id);
//     if (user == null) return NotFound();

//     user.IsDeleted = true;
//     _context.Users.Update(user);
//     await _context.SaveChangesAsync();
    
//     return Json(new { success = true });
// }

// [HttpPost]
// public async Task<IActionResult> AddUser(MyProfileViewModel model)
// {
//     if (!ModelState.IsValid)
//     {
//         return View(model);
//     }

//     var token = Request.Cookies["authToken"];
//     var email = _jwtService.GetClaimValue(token, "email");

//     var newUser = new User
//     {
//         FirstName = model.FirstName,
//         LastName = model.LastName,
//         Username = model.UserName,
//         Email = model.Email,
//         Phone = model.Phone,
//         CountryId = model.CountryId,
//         StateId = model.StateId,
//         CityId = model.CityId,
//         Address = model.Address,
//         ZipCode = model.ZipCode,
//         Password = GenerateTemporaryPassword(),
//         IsDeleted = false
//     };

//     _context.Users.Add(newUser);
//     await _context.SaveChangesAsync();

//     // Send Email
//     var emailService = new EmailService(_config);
//     var subject = "Welcome to Our System - Your Login Details";
//     var body = $"Hello {model.FirstName},<br/><br/>"
//              + "You have been added to our system.<br/>"
//              + $"Your username: <strong>{model.UserName}</strong><br/>"
//              + $"Your temporary password: <strong>{newUser.Password}</strong><br/><br/>"
//              + "Please change your password after logging in.<br/><br/>"
//              + "Best Regards,<br/>Your Company";

//     await emailService.SendEmailAsync(model.Email, subject, body);

//     TempData["SuccessMessage"] = "User added and email sent successfully!";
//     return RedirectToAction("UsersList");
// }

}