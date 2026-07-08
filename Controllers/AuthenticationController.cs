using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Enum;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Authorize]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly INotificationService _notificationService;

    public AuthenticationController(
        IAuthenticationService authenticationService,
        INotificationService notificationService)
    {
        _authenticationService = authenticationService;
        _notificationService = notificationService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
    {
        if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
            return BadRequest(new { message = GeneralConstants.InvalidInput });

        var data = await _authenticationService.LoginAsync(loginDto);

        if (data != null)
        {
            try
            {
                if (data.UserId > 0)
                {
                    await _notificationService.TriggerAuthenticationNotificationAsync(
                        data.UserId,
                        $"Welcome back! You have successfully logged in at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC",
                        NotificationCategory.SystemAlert
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Notification trigger failed for login: {ex.Message}");
            }

            return Ok(new { message = GeneralConstants.OperationSuccess, data });
        }

        return Unauthorized(new { message = AuthConstants.UnauthorizedAccess });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto logoutDto)
    {
        if (logoutDto == null)
            return BadRequest(new { message = GeneralConstants.InvalidInput });

        var result = await _authenticationService.LogoutAsync(logoutDto);

        if (result)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (long.TryParse(userIdClaim, out var userId) && userId > 0)
                {
                    await _notificationService.TriggerAuthenticationNotificationAsync(
                        userId,
                        $"You have logged out at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC",
                        NotificationCategory.SystemAlert
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Notification trigger failed for logout: {ex.Message}");
            }

            return Ok(new { message = GeneralConstants.OperationSuccess });
        }

        return Unauthorized(new { message = AuthConstants.UnauthorizedAccess });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.NewPassword))
            return BadRequest(new { message = GeneralConstants.InvalidInput });

        var result = await _authenticationService.ForgotPasswordAsync(dto);

        if (result)
            return Ok(new { message = AuthConstants.PasswordResetSuccess });

        return BadRequest(new { message = AuthConstants.PasswordResetFailed });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] PasswordResetDto resetDto)
    {
        if (resetDto == null)
            return BadRequest(new { message = GeneralConstants.InvalidInput });

        var result = await _authenticationService.ResetPasswordAsync(resetDto);

        if (result)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (long.TryParse(userIdClaim, out var userId) && userId > 0)
                {
                    await _notificationService.TriggerAuthenticationNotificationAsync(
                        userId,
                        $"Your password has been successfully reset at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC",
                        NotificationCategory.SystemAlert
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Notification trigger failed for password reset: {ex.Message}");
            }

            return Ok(new { message = AuthConstants.PasswordResetSuccess });
        }

        return BadRequest(new { message = AuthConstants.PasswordResetFailed });
    }
}
