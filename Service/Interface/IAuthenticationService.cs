using TravelEaseServer.Dto;

namespace TravelEaseServer.Service.Interface
{
    public interface IAuthenticationService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto loginDto);
        Task<bool> LogoutAsync(LogoutRequestDto logoutDto);
        Task<bool> ResetPasswordAsync(PasswordResetDto resetDto);
        Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<bool> ValidateTokenAsync(string token);
    }
}
