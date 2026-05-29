using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers
{
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
            return !ModelState.IsValid || searchDto == null 
                ? BadRequest(new { message = GeneralConstants.InvalidInput })
                : Ok(new { message = GeneralConstants.OperationSuccess, data = await _userService.GetAllUsersAsync(searchDto) });
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserRequestDto userDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = GeneralConstants.InvalidInput, errors = ModelState.Values.SelectMany(v => v.Errors) });
            
            if (userDto == null)
                return BadRequest(new { message = "User data is required" });
            
            if (string.IsNullOrWhiteSpace(userDto.Name))
                return BadRequest(new { message = "Name is required" });
            
            if (string.IsNullOrWhiteSpace(userDto.Email))
                return BadRequest(new { message = "Email is required" });
            
            if (string.IsNullOrWhiteSpace(userDto.Password))
                return BadRequest(new { message = "Password is required" });
            
            if (userDto.Password.Length < 6)
                return BadRequest(new { message = "Password must be at least 6 characters" });

            var result = await _userService.CreateUserAsync(userDto);
            return Ok(new { message = UserConstants.UserCreatedSuccess, data = result });
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(long userId)
        {
            if (userId <= 0)
                return BadRequest(new { message = "Invalid User ID" });

            var user = await _userService.GetUserByIdAsync(userId);
            return user != null 
                ? Ok(new { message = GeneralConstants.OperationSuccess, data = user })
                : NotFound(new { message = UserConstants.UserNotFound });
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(long userId, [FromBody] UserRequestDto userDto)
        {
            if (userId <= 0)
                return BadRequest(new { message = "Invalid User ID" });

            if (!ModelState.IsValid)
                return BadRequest(new { message = GeneralConstants.InvalidInput, errors = ModelState.Values.SelectMany(v => v.Errors) });
            
            if (userDto == null)
                return BadRequest(new { message = "User data is required" });
            
            if (string.IsNullOrWhiteSpace(userDto.Name))
                return BadRequest(new { message = "Name is required" });
            
            if (string.IsNullOrWhiteSpace(userDto.Email))
                return BadRequest(new { message = "Email is required" });

            var result = await _userService.UpdateUserAsync(userId, userDto);
            return result != null
                ? Ok(new { message = UserConstants.UserUpdateSuccess, data = result })
                : NotFound(new { message = UserConstants.UserNotFound });
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(long userId)
        {
            if (userId <= 0)
                return BadRequest(new { message = "Invalid User ID" });

            var result = await _userService.DeleteUserAsync(userId);
            return result 
                ? Ok(new { message = UserConstants.UserDeleteSuccess })
                : NotFound(new { message = UserConstants.UserNotFound });
        }

        [HttpPut("{userId}/roles")]
        public async Task<IActionResult> AssignUserRole(long userId, [FromBody] UserRoleAssignmentDto roleDto)
        {
            if (userId <= 0)
                return BadRequest(new { message = "Invalid User ID" });

            if (!ModelState.IsValid)
                return BadRequest(new { message = GeneralConstants.InvalidInput, errors = ModelState.Values.SelectMany(v => v.Errors) });
            
            if (roleDto == null)
                return BadRequest(new { message = "Role data is required" });
            
            if (roleDto.NewRole < 0)
                return BadRequest(new { message = "Invalid role value" });

            var result = await _userService.AssignRoleAsync(userId, roleDto.NewRole);
            return result != null
                ? Ok(new { message = UserConstants.RoleAssignmentSuccess, data = result })
                : NotFound(new { message = UserConstants.UserNotFound });
        }
    }
}
