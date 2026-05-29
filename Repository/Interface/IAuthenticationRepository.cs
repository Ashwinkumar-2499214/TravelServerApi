using TravelEaseServer.Dto;
using TravelEaseServer.Model;

namespace TravelEaseServer.Repository.Interface
{
    public interface IAuthenticationRepository
    {
        Task<User> AuthenticateUserAsync(string email, string password);
        Task<bool> ValidatePasswordAsync(string password, string passwordHash);
        Task<bool> UpdatePasswordAsync(long userId, string newPasswordHash);
        Task<bool> LogoutAsync(long userId);
    }
}
