
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaShop.Models; // Replace with your actual namespace
using System.Linq;
using System.Threading.Tasks;

namespace PizzaShop.Controllers;
public class UsersController : Controller
{
    private readonly PizzashopContext _context;

    public UsersController(PizzashopContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string search, int page = 1, int pageSize = 5)
    {
        var query = _context.Users.AsQueryable(); // Assuming Users table exists

        // Apply search filter
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.FirstName.Contains(search) || u.Email.Contains(search));
        }

        // Pagination
        int totalItems = await query.CountAsync();
        var users = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        ViewBag.Search = search;

        return View(users);
    }
}
