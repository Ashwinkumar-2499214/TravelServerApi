using Microsoft.EntityFrameworkCore;
using TravelEaseServer.Dto;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;

namespace TravelEaseServer.Repository.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserResponseDto> CreateUserAsync(User user)
        {
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return MapUserToDto(user);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Error creating user in database.", ex);
            }
        }

        public async Task<UserResponseDto> GetUserByIdAsync(long userId)
        {
            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                return user != null ? MapUserToDto(user) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving user with ID {userId}.", ex);
            }
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(UserSearchDto searchDto)
        {
            try
            {
                var query = _context.Users.AsNoTracking();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
                {
                    var term = searchDto.SearchTerm.ToLower();
                    query = query.Where(u => u.Name.ToLower().Contains(term) || 
                                           u.Email.ToLower().Contains(term));
                }

                if (searchDto.Role.HasValue)
                {
                    query = query.Where(u => u.Role == searchDto.Role.Value);
                }

                if (searchDto.IsActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == searchDto.IsActive.Value);
                }

                // Apply pagination
                int skip = (searchDto.PageNumber - 1) * searchDto.PageSize;
                var users = await query
                    .OrderByDescending(u => u.CreatedDate)
                    .Skip(skip)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                return users.Select(MapUserToDto);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving users.", ex);
            }
        }

        public async Task<UserResponseDto> UpdateUserAsync(User user)
        {
            try
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == user.UserId);
                if (existingUser == null)
                {
                    throw new KeyNotFoundException($"User with ID {user.UserId} not found.");
                }

                existingUser.Name = user.Name;
                existingUser.Email = user.Email;
                existingUser.Phone = user.Phone;
                existingUser.Role = user.Role;
                existingUser.IsActive = user.IsActive;
                existingUser.ModifiedDate = DateTime.UtcNow;

                _context.Users.Update(existingUser);
                await _context.SaveChangesAsync();

                return MapUserToDto(existingUser);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error updating user with ID {user.UserId}.", ex);
            }
        }

        public async Task<bool> DeleteUserAsync(long userId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                {
                    return false;
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error deleting user with ID {userId}.", ex);
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving user by email {email}.", ex);
            }
        }

        private UserResponseDto MapUserToDto(User user)
        {
            return new UserResponseDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate
            };
        }
    }
}
