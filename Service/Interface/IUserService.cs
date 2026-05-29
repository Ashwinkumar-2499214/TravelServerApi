using TravelEaseServer.Dto;

namespace TravelEaseServer.Service.Interface
{
    public interface IUserService
    {
        Task<UserResponseDto> CreateUserAsync(UserRequestDto userDto);
        Task<UserResponseDto> GetUserByIdAsync(long userId);
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(UserSearchDto searchDto);
        Task<UserResponseDto> UpdateUserAsync(long userId, UserRequestDto userDto);
        Task<bool> DeleteUserAsync(long userId);
        Task<UserResponseDto> AssignRoleAsync(long userId, int newRole);
    }
}
