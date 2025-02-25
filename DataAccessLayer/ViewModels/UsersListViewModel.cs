using System.ComponentModel.DataAnnotations;
using DataAccessLayer.Models;

namespace DataAccessLayer.ViewModel
{
    public class UsersListViewModel
    {
        public List<UserInfoViewModel> User { get; set; } = new List<UserInfoViewModel>();

        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalRecords { get; set; }

        // Calculated properties for pagination
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
        public int FromRec => ((PageNumber - 1) * PageSize) + 1;
        public int ToRec => Math.Min(PageNumber * PageSize, TotalRecords);
    }
}
