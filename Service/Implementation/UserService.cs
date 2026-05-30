using System.Security.Cryptography;
using System.Text;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Service.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserResponseDto> CreateUserAsync(UserRequestDto userDto)
        {
            var passwordHash = HashPassword(userDto.Password);

            var user = new User
            {
                Name = userDto.Name,
                Email = userDto.Email,
                Phone = userDto.Phone,
                PasswordHash = passwordHash,
                Role = userDto.Role,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            return await _userRepository.CreateUserAsync(user);
        }

        public async Task<UserResponseDto> GetUserByIdAsync(long userId)
        {
            return await _userRepository.GetUserByIdAsync(userId);
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(UserSearchDto searchDto)
        {
            return await _userRepository.GetAllUsersAsync(searchDto);
        }

        public async Task<UserResponseDto> UpdateUserAsync(long userId, UserRequestDto userDto)
        {
         
            var user = new User
            {
                UserId = userId,
                Name = userDto.Name,
                Email = userDto.Email,
                Phone = userDto.Phone,
                Role = userDto.Role,
                PasswordHash = string.Empty,
                ModifiedDate = DateTime.UtcNow
            };

            return await _userRepository.UpdateUserAsync(user);
        }

        public async Task<bool> DeleteUserAsync(long userId)
        {
            return await _userRepository.DeleteUserAsync(userId);
        }

        public async Task<UserResponseDto> AssignRoleAsync(long userId, int newRole)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                    throw new InvalidOperationException(UserConstants.UserNotFound);

                var updatedUser = new User
                {
                    UserId = userId,
                    Name = user.Name,
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = newRole,
                    // keep required member
                    PasswordHash = string.Empty,
                    ModifiedDate = DateTime.UtcNow
                };

                return await _userRepository.UpdateUserAsync(updatedUser);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(UserConstants.RoleAssignmentSuccess, ex);
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
