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
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return MapUserToDto(user);
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(long userId)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);
            return user != null ? MapUserToDto(user) : null;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(UserSearchDto searchDto)
        {
            var query = _context.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
            {
                var term = searchDto.SearchTerm.ToLower();
                query = query.Where(u => u.Name.ToLower().Contains(term) || u.Email.ToLower().Contains(term));
            }

            if (searchDto.Role.HasValue)
                query = query.Where(u => u.Role == searchDto.Role.Value);

            if (searchDto.IsActive.HasValue)
                query = query.Where(u => u.IsActive == searchDto.IsActive.Value);
            else
                query = query.Where(u => u.IsActive);


            int skip = (searchDto.PageNumber - 1) * searchDto.PageSize;

            var users = await query
                .OrderByDescending(u => u.CreatedDate)
                .Skip(skip)
                .Take(searchDto.PageSize)
                .ToListAsync();

            return users.Select(MapUserToDto);
        }

        public async Task<UserResponseDto> UpdateUserAsync(User user)
        {
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.UserId == user.UserId);
            if (existing == null) throw new KeyNotFoundException($"User with ID {user.UserId} not found.");

            existing.Name = user.Name;
            existing.Email = user.Email;
            existing.Phone = user.Phone;
            existing.Role = user.Role;
            existing.ModifiedDate = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(user.PasswordHash))
                existing.PasswordHash = user.PasswordHash;

            await _context.SaveChangesAsync();
            return MapUserToDto(existing);
        }

        public async Task<bool> DeleteUserAsync(long userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User?> GetUserByEmailAsync(string email) =>
            await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);

        private UserResponseDto MapUserToDto(User user) => new()
        {
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Role = ((TravelEaseServer.Enum.UserRole)user.Role).ToString(),
            IsActive = user.IsActive,
            CreatedDate = user.CreatedDate
        };
        public async Task<User?> GetTrackedUserByIdAsync(long userId) =>
           await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

        public async Task SaveAuditLogAsync(AuditLog auditLog)
        {
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}