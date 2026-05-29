using Microsoft.EntityFrameworkCore;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;
using System.Security.Cryptography;
using System.Text;

namespace TravelEaseServer.Repository.Implementation
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly AppDbContext _context;

        public AuthenticationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> AuthenticateUserAsync(string email, string password)
        {
            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null || !ValidatePasswordHash(password, user.PasswordHash))
                {
                    return null;
                }

                return user;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error authenticating user with email {email}.", ex);
            }
        }

        public async Task<bool> ValidatePasswordAsync(string password, string passwordHash)
        {
            try
            {
                return await Task.Run(() => ValidatePasswordHash(password, passwordHash));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error validating password.", ex);
            }
        }

        public async Task<bool> UpdatePasswordAsync(long userId, string newPasswordHash)
        {
            try
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
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error updating password for user with ID {userId}.", ex);
            }
        }

        public async Task<bool> LogoutAsync(long userId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                {
                    return false;
                }

                // Log the logout event (if you have a session or audit log table)
                // This is a placeholder for future session tracking logic
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error during logout for user with ID {userId}.", ex);
            }
        }

        /// <summary>
        /// Validates a password against a hash using PBKDF2.
        /// </summary>
        private bool ValidatePasswordHash(string password, string hash)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
                {
                    return false;
                }

                // Hash format: algorithm$iterations$salt$hash
                var parts = hash.Split('$');
                if (parts.Length != 4)
                {
                    return false;
                }

                var iterations = int.Parse(parts[1]);
                var salt = Convert.FromBase64String(parts[2]);
                var storedHash = parts[3];

                // Compute hash with the same salt and iterations
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                {
                    var computedHash = Convert.ToBase64String(pbkdf2.GetBytes(32));
                    return computedHash == storedHash;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a password hash using PBKDF2.
        /// </summary>
        public static string CreatePasswordHash(string password)
        {
            const int iterations = 10000;

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, 16, iterations, HashAlgorithmName.SHA256))
            {
                var salt = pbkdf2.Salt;
                var hash = pbkdf2.GetBytes(32);

                var hashString = $"PBKDF2${iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
                return hashString;
            }
        }
    }
}
