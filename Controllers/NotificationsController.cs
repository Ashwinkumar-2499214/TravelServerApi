using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

    
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllNotifications([FromQuery] NotificationSearchDto searchDto)
        {
            if (searchDto == null)
            {
                searchDto = new NotificationSearchDto { PageNumber = 1, PageSize = 10 };
            }

            var notifications = await _notificationService.GetAllNotificationsAsync(searchDto);
            return Ok(new { message = GeneralConstants.OperationSuccess, data = notifications });
        }

     
        [HttpGet("~/api/v1/users/{userId}/notifications")]
        [Authorize(Roles = "Admin,TravelAgent,CorporateTravelManager,Traveler,FinanceOfficer,ComplianceOfficer")]
        public async Task<IActionResult> GetUserNotifications([FromRoute] long userId)
        {
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!long.TryParse(currentUserIdClaim, out var currentUserId))
                return Unauthorized(new { message = GeneralConstants.UnauthorizedAccess });

            if (!User.IsInRole("Admin") && currentUserId != userId)
                return Forbid();

            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            return Ok(new { message = GeneralConstants.OperationSuccess, data = notifications });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateNotification([FromBody] NotificationRequestDto notificationDto)
        {
            if (notificationDto == null || notificationDto.UserId <= 0 || string.IsNullOrWhiteSpace(notificationDto.Message))
            {
                return BadRequest(new { message = GeneralConstants.InvalidInput });
            }

            var result = await _notificationService.CreateNotificationAsync(notificationDto);
            return Ok(new { message = NotificationConstants.NotificationCreatedSuccess, data = result });
        }

        [HttpGet("{notificationId:long}")]
        [Authorize(Roles = "Admin,TravelAgent,CorporateTravelManager,Traveler,FinanceOfficer,ComplianceOfficer")]
        public async Task<IActionResult> GetNotificationById([FromRoute] long notificationId)
        {
            var notification = await _notificationService.GetNotificationByIdAsync(notificationId);
            return Ok(new { message = GeneralConstants.OperationSuccess, data = notification });
        }

        [HttpDelete("{notificationId:long}")]
        [Authorize(Roles = "Admin,TravelAgent,CorporateTravelManager,Traveler,FinanceOfficer,ComplianceOfficer")]
        public async Task<IActionResult> DeleteNotification([FromRoute] long notificationId)
        {
            var isDeleted = await _notificationService.DeleteNotificationAsync(notificationId);
            if (!isDeleted)
            {
                return BadRequest(new { message = NotificationConstants.NotificationNotFound });
            }

            return Ok(new { message = NotificationConstants.NotificationDeleteSuccess });
        }

        [HttpPatch("{notificationId:long}/read")]
        [Authorize(Roles = "Admin,TravelAgent,CorporateTravelManager,Traveler,FinanceOfficer,ComplianceOfficer")]
        public async Task<IActionResult> MarkAsRead([FromRoute] long notificationId)
        {
            var result = await _notificationService.MarkAsReadAsync(notificationId);
            if (result == null)
            {
                return BadRequest(new { message = NotificationConstants.NotificationNotFound });
            }

            return Ok(new { message = NotificationConstants.NotificationMarkedAsRead });
        }

        [HttpPatch("~/api/v1/users/{userId}/notifications/read-all")]
        [Authorize(Roles = "Admin,TravelAgent,CorporateTravelManager,Traveler,FinanceOfficer,ComplianceOfficer")]
        public async Task<IActionResult> MarkAllAsRead([FromRoute] long userId)
        {
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!long.TryParse(currentUserIdClaim, out var currentUserId))
                return Unauthorized(new { message = GeneralConstants.UnauthorizedAccess });

            if (!User.IsInRole("Admin") && currentUserId != userId)
                return Forbid();

            var isSuccess = await _notificationService.MarkAllAsReadAsync(userId);
            if (!isSuccess)
            {
                return BadRequest(new { message = GeneralConstants.OperationFailed });
            }

            return Ok(new { message = NotificationConstants.AllNotificationsMarkedAsRead });
        }
    }
}