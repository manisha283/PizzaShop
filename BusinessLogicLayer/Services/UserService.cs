using DataAccessLayer.Models;
using DataAccessLayer.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public UserPagination GetPagedUsers(int pageSize, int pageNumber)
        {
            UserPagination model = new() { Page = new() };
            
            var userDb = _userRepository.GetPagedRecords(
                pageSize,
                pageNumber,
                q => q.OrderBy(u => u.UserId)  // Assuming 'UserId' exists
            );

            model.Users = userDb.records.Select(user => new UserDetail()
            {
                ProfileImageUrl = user.ProfileImg,  // Mapping from database
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                Status = user.Status
            });

            model.Page.SetPagination(userDb.totalRecord, pageSize, pageNumber);  
            return model;
        }


        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<bool> AddUserAsync(User user)
        {
            return await _userRepository.AddUserAsync(user);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            return await _userRepository.UpdateUserAsync(user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _userRepository.DeleteUserAsync(id);
        }
    }
}
