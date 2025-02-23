using PizzaShop.Models;

namespace PizzaShop.ViewModel{
    public class UsersViewModel{
        public List<User> Users { get; set; } = new List<User>();
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalRecords { get; set; }
    }
}
