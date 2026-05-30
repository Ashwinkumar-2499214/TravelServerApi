using Microsoft.EntityFrameworkCore;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;

namespace TravelEaseServer.Repository.Implementation;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User> CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetUserByIdAsync(long userId)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync(string? searchTerm, int? role, bool? isActive, int pageNumber, int pageSize)
    {
        var query = _context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(u => u.Name.ToLower().Contains(term) || u.Email.ToLower().Contains(term));
        }

        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        int skip = (pageNumber - 1) * pageSize;

        return await query
            .OrderByDescending(u => u.CreatedDate)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<User?> UpdateUserAsync(User user)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == user.UserId);
        if (existingUser == null)
        {
            return null;
        }

        existingUser.Name = user.Name;
        existingUser.Email = user.Email;
        existingUser.Phone = user.Phone;
        existingUser.Role = user.Role;
        existingUser.IsActive = user.IsActive;
        existingUser.ModifiedDate = DateTime.UtcNow;

        _context.Users.Update(existingUser);
        await _context.SaveChangesAsync();

        return existingUser;
    }

    public async Task<bool> DeleteUserAsync(long userId)
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

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }
}