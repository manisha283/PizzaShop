using System.ComponentModel.DataAnnotations;

using PizzaShop.Models;
using PizzaShop.Services;
using PizzaShop.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MailKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            // foreach (var key in ModelState.Keys)
            // {
            //     var errors = ModelState[key].Errors;
            //     foreach (var error in errors)
            //     {
            //         Console.WriteLine($"Key: {key}, Error: {error.ErrorMessage}");
            //     }
            // }
            return View(model);
        }

        // email is fetched from the token
        var token = Request.Cookies["authToken"];
        var email = _jwtService.GetClaimValue(token,"email");
        // model.Email = email;

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

    [HttpGet]
    public async Task<IActionResult> Users(string? search, int pageNumber = 1, int pageSize = 5)
    {
        var query = _context.Users.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.FirstName.Contains(search)|| u.LastName.Contains(search) || u.Email.Contains(search));
        }

        // Get total record count before pagination
        int totalRecords = await query.CountAsync();

        // Apply pagination
        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var viewModel = new UsersViewModel
        {
            Users = users,
            SearchTerm = search,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords
        };

        return View(viewModel);
    }

    public IActionResult AddUser(){
        return View();
    }

    // [HttpPost]
    // public async Task<IActionResult> AddUser(AddUserViewModel model)
    // {
    //     if (ModelState.IsValid)
    //     {
    //         // Map ViewModel to User Model
    //         var user = new User
    //         {
    //             FirstName = model.FirstName,
    //             LastName = model.LastName,
    //             UserName = model.UserName,
    //             Email = model.Email,
    //             Password = model.Password, // Encrypt this before saving
    //             Role = model.Role,
    //             Country = model.Country,
    //             State = model.State,
    //             City = model.City,
    //             ZipCode = model.ZipCode,
    //             Address = model.Address,
    //             Phone = model.Phone
    //         };

    //         // Handle Profile Image Upload (if any)
    //         if (model.ProfileImage != null)
    //         {
    //             var filePath = Path.Combine("wwwroot/uploads", model.ProfileImage.FileName);
    //             using (var stream = new FileStream(filePath, FileMode.Create))
    //             {
    //                 await model.ProfileImage.CopyToAsync(stream);
    //             }
    //             user.ProfileImagePath = "/uploads/" + model.ProfileImage.FileName;
    //         }

    //         // Add user to database
    //         _context.Users.Add(user);
    //         await _context.SaveChangesAsync();

    //         return RedirectToAction("UserList"); // Redirect after adding
    //     }

    //     return View(model); // Return form with validation errors
    // }
}