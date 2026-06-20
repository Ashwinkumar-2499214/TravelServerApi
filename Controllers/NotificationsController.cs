using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Roles = "ComplianceOfficer")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("~/api/v1/users/{userId}/notifications")]
        public async Task<IActionResult> GetUserNotifications([FromRoute] long userId)
        {
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            return Ok(new { message = GeneralConstants.OperationSuccess, data = notifications });
        }

        [HttpPost]
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
        public async Task<IActionResult> GetNotificationById([FromRoute] long notificationId)
        {
            var notification = await _notificationService.GetNotificationByIdAsync(notificationId);
            return Ok(new { message = GeneralConstants.OperationSuccess, data = notification });
        }

        [HttpDelete("{notificationId:long}")]
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
        public async Task<IActionResult> MarkAllAsRead([FromRoute] long userId)
        {
            var isSuccess = await _notificationService.MarkAllAsReadAsync(userId);
            if (!isSuccess)
            {
                return BadRequest(new { message = GeneralConstants.OperationFailed });
            }

            return Ok(new { message = NotificationConstants.AllNotificationsMarkedAsRead });
        }
    }
}