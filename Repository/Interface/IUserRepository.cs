using TravelEaseServer.Dto;
using TravelEaseServer.Model;

namespace TravelEaseServer.Repository.Interface
{
    public interface IUserRepository
    {
        Task<UserResponseDto> CreateUserAsync(User user);
        Task<UserResponseDto> GetUserByIdAsync(long userId);
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(UserSearchDto searchDto);
        Task<UserResponseDto> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(long userId);
        Task<User> GetUserByEmailAsync(string email);
    }
}
