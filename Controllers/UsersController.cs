using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers([FromQuery] UserSearchDto searchDto)
    {
        if (searchDto == null)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _userService.GetAllUsersAsync(searchDto);

        return data != null
            ? Ok(new { message = GeneralConstants.OperationSuccess, data })
            : NotFound(new { message = UserConstants.UserNotFound });
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] UserRequestDto userDto)
    {
        if (userDto == null)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _userService.CreateUserAsync(userDto);

        return data != null
            ? Ok(new { message = UserConstants.UserCreatedSuccess, data })
            : BadRequest(new { message = GeneralConstants.InvalidInput });
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserById(long userId)
    {
        if (userId <= 0)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _userService.GetUserByIdAsync(userId);

        return data != null
            ? Ok(new { message = GeneralConstants.OperationSuccess, data })
            : NotFound(new { message = UserConstants.UserNotFound });
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateUser(long userId, [FromBody] UserRequestDto userDto)
    {
        if (userId <= 0 || userDto == null)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _userService.UpdateUserAsync(userId, userDto);

        return data != null
            ? Ok(new { message = UserConstants.UserUpdateSuccess, data })
            : NotFound(new { message = UserConstants.UserNotFound });
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(long userId)
    {
        if (userId <= 0)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var result = await _userService.DeleteUserAsync(userId);

        return result
            ? Ok(new { message = UserConstants.UserDeleteSuccess })
            : NotFound(new { message = UserConstants.UserNotFound });
    }

    [HttpPut("{userId}/roles")]
    public async Task<IActionResult> AssignUserRole(long userId, [FromBody] UserRoleAssignmentDto roleDto)
    {
        if (userId <= 0 || roleDto == null)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _userService.AssignRoleAsync(userId, roleDto.NewRole);

        return data != null
            ? Ok(new { message = UserConstants.RoleAssignmentSuccess, data })
            : NotFound(new { message = UserConstants.UserNotFound });
    }
}