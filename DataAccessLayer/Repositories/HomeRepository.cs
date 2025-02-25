using System.Threading.Tasks;
using DataAccessLayer.ViewModel;
using DataAccessLayer.Models;
using DataAccessLayer.Services;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;
public class HomeRepository : IHomeRepository
{
    private readonly PizzaShopContext _context;

    public async Task<User> GetOneByEmailAsync(string email)
    {
        return await _context.Users.Where(u => u.Email == email).FirstOrDefaultAsync();  
    }
}
