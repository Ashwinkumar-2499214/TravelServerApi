using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers;

[ApiController]
[Route("api/v1/[controller]")]

public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AuthenticationController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
    {
        if (loginDto == null)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _authenticationService.LoginAsync(loginDto);

        return data != null
            ? Ok(new { message = GeneralConstants.OperationSuccess, data })
            : BadRequest(new { message = AuthConstants.UnauthorizedAccess });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto logoutDto)
    {
        if (logoutDto == null)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var result = await _authenticationService.LogoutAsync(logoutDto);

        return result
            ? Ok(new { message = GeneralConstants.OperationSuccess })
            : BadRequest(new { message = AuthConstants.UnauthorizedAccess });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] PasswordResetDto resetDto)
    {
        if (resetDto == null)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var result = await _authenticationService.ResetPasswordAsync(resetDto);

        return result
            ? Ok(new { message = AuthConstants.PasswordResetSuccess })
            : BadRequest(new { message = AuthConstants.PasswordResetFailed });
    }
}