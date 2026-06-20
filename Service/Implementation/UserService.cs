using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(
            IUserRepository userRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
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

            // Check for duplicate email before creating user
            var existing = await _userRepository.GetUserByEmailAsync(userDto.Email);
            if (existing != null)
            {
                throw new ArgumentException(Constant.UserConstants.EmailAlreadyExists);
            }

            var updatedUserResult = await _userRepository.CreateUserAsync(user);

            var clientIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            clientIp = (string.IsNullOrEmpty(clientIp) || clientIp == "::1") ? "127.0.0.1" : clientIp;

            await _userRepository.SaveAuditLogAsync(new AuditLog
            {
                UserId = updatedUserResult.UserId,
                User = user,
                Action = "Create",
                EntityType = "User",
                EntityId = updatedUserResult.UserId,
                OldValues = string.Empty,
                NewValues = $"Name: {user.Name}, Email: {user.Email}, Phone: {user.Phone}, Role: {user.Role}",
                Timestamp = DateTime.UtcNow,
                IpAddress = clientIp
            });

            return updatedUserResult;
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
            var existingUser = await _userRepository.GetTrackedUserByIdAsync(userId)
                ?? throw new KeyNotFoundException($"User with ID {userId} not found.");

            var oldValuesList = new List<string>();
            var newValuesList = new List<string>();

            if (existingUser.Name != userDto.Name)
            {
                oldValuesList.Add($"Name: {existingUser.Name}");
                newValuesList.Add($"Name: {userDto.Name}");
            }
            if (existingUser.Email != userDto.Email)
            {
                oldValuesList.Add($"Email: {existingUser.Email}");
                newValuesList.Add($"Email: {userDto.Email}");
            }
            if (existingUser.Phone != userDto.Phone)
            {
                oldValuesList.Add($"Phone: {existingUser.Phone}");
                newValuesList.Add($"Phone: {userDto.Phone}");
            }
            if (existingUser.Role != userDto.Role)
            {
                oldValuesList.Add($"Role: {existingUser.Role}");
                newValuesList.Add($"Role: {userDto.Role}");
            }

            existingUser.Name = userDto.Name;
            existingUser.Email = userDto.Email;
            existingUser.Phone = userDto.Phone;
            existingUser.Role = userDto.Role;
            existingUser.ModifiedDate = DateTime.UtcNow;

            var updatedUserResult = await _userRepository.UpdateUserAsync(existingUser);

            if (oldValuesList.Any())
            {
                var clientIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                clientIp = (string.IsNullOrEmpty(clientIp) || clientIp == "::1") ? "127.0.0.1" : clientIp;

                await _userRepository.SaveAuditLogAsync(new AuditLog
                {
                    UserId = userId,
                    User = existingUser,
                    Action = "Update",
                    EntityType = "User",
                    EntityId = userId,
                    OldValues = string.Join(", ", oldValuesList),
                    NewValues = string.Join(", ", newValuesList),
                    Timestamp = DateTime.UtcNow,
                    IpAddress = clientIp
                });
            }

            return updatedUserResult;
        }

        public async Task<bool> DeleteUserAsync(long userId)
        {
            var existingUser = await _userRepository.GetTrackedUserByIdAsync(userId);
            if (existingUser == null || !existingUser.IsActive) return false;

            existingUser.IsActive = false;
            existingUser.ModifiedDate = DateTime.UtcNow;
            await _userRepository.UpdateUserAsync(existingUser);

            var clientIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            clientIp = (string.IsNullOrEmpty(clientIp) || clientIp == "::1") ? "127.0.0.1" : clientIp;

            await _userRepository.SaveAuditLogAsync(new AuditLog
            {
                UserId = userId,
                User = existingUser,
                Action = "Delete",
                EntityType = "User",
                EntityId = userId,
                OldValues = $"Name: {existingUser.Name}, Email: {existingUser.Email}, Phone: {existingUser.Phone}, Role: {existingUser.Role}",
                NewValues = string.Empty,
                Timestamp = DateTime.UtcNow,
                IpAddress = clientIp
            });

            return true;
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
