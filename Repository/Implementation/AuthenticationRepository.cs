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

    // FIXED: Now correctly creates the PBKDF2 hash format before saving to MS SQL
    public async Task<bool> UpdatePasswordAsync(long userId, string newPassword)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
        {
            return false;
        }

        // Generate the formatted PBKDF2 hash string out of the plain text password
        user.PasswordHash = CreatePasswordHash(newPassword);
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
            return false;

        // If stored hash looks like the PBKDF2 formatted string: PBKDF2$iterations$salt$hash
        var parts = hash.Split('$');
        if (parts.Length == 4 && parts[0].Equals("PBKDF2", StringComparison.OrdinalIgnoreCase))
        {
            if (!int.TryParse(parts[1], out var iterations))
                return false;

            byte[] salt;
            try
            {
                salt = Convert.FromBase64String(parts[2]);
            }
            catch
            {
                return false;
            }

            var storedHash = parts[3];
            var computedBytes = Rfc2898DeriveBytes.Pbkdf2(password.AsSpan(), salt.AsSpan(), iterations, HashAlgorithmName.SHA256, 32);
            var computedHash = Convert.ToBase64String(computedBytes);
            return computedHash == storedHash;
        }

        // Fallback: support legacy Base64-encoded SHA256(password) stored in DB
        try
        {
            var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
            using var sha256 = SHA256.Create();
            var computed = sha256.ComputeHash(passwordBytes);
            var computedBase64 = Convert.ToBase64String(computed);
            return computedBase64 == hash;
        }
        catch
        {
            return false;
        }
    }

    public static string CreatePasswordHash(string password)
    {
        const int iterations = 10000;

        var salt = new byte[16];
        RandomNumberGenerator.Fill(salt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password.AsSpan(), salt.AsSpan(), iterations, HashAlgorithmName.SHA256, 32);

        return $"PBKDF2${iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public async Task<int> CountUsersByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return 0;
        return await _context.Users.AsNoTracking().CountAsync(u => u.Email == email);
    }
}