using Microsoft.EntityFrameworkCore;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;
using System.Security.Cryptography;

namespace TravelEaseServer.Repository.Implementation;

public class AuthenticationRepository : IAuthenticationRepository
{
    private readonly AppDbContext _context;

    public AuthenticationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> AuthenticateUserAsync(string email, string password)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);

        return user != null && ValidatePasswordHash(password, user.PasswordHash) ? user : null;
    }

    public async Task<bool> ValidatePasswordAsync(string password, string passwordHash)
    {
        return await Task.Run(() => ValidatePasswordHash(password, passwordHash));
    }

    public async Task<bool> UpdatePasswordAsync(long userId, string newPasswordHash)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
        {
            return false;
        }

        user.PasswordHash = newPasswordHash;
        user.ModifiedDate = DateTime.UtcNow;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> LogoutAsync(long userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        return user != null;
    }

    private bool ValidatePasswordHash(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
        {
            return false;
        }

        var parts = hash.Split('$');
        if (parts.Length != 4)
        {
            return false;
        }

        var iterations = int.Parse(parts[1]);
        var salt = Convert.FromBase64String(parts[2]);
        var storedHash = parts[3];

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        var computedHash = Convert.ToBase64String(pbkdf2.GetBytes(32));
        return computedHash == storedHash;
    }

    public static string CreatePasswordHash(string password)
    {
        const int iterations = 10000;

        using var pbkdf2 = new Rfc2898DeriveBytes(password, 16, iterations, HashAlgorithmName.SHA256);
        var salt = pbkdf2.Salt;
        var hash = pbkdf2.GetBytes(32);

        return $"PBKDF2${iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }
}