namespace Demo.Repository.ViewModel;

public class UserPagination
{
    public IEnumerable<UserDetail>? Users { get; set; }  // List of users
    public Pagination? Page { get; set; }  // Pagination details
}
