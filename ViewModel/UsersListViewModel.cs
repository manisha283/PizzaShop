using System.ComponentModel.DataAnnotations;
using PizzaShop.Models;

namespace PizzaShop.ViewModel{
    public class UsersListViewModel
    {
        public List<UserInfoViewModel> User { get; set; } = new List<UserInfoViewModel>();
        
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalRecords { get; set; }
        // Computed properties for pagination
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}