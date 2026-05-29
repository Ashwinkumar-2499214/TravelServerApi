using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("/api/v1/users/{userId}/notifications")]
        public async Task<IActionResult> GetUserNotifications(long userId)
        {
            return Ok(new { message = GeneralConstants.OperationSuccess, data = await _notificationService.GetUserNotificationsAsync(userId) });
        }

        [HttpPost]
        public async Task<IActionResult> CreateNotification([FromBody] NotificationRequestDto notificationDto)
        {
            return (ModelState.IsValid && notificationDto != null)
                ? Ok(new { message = NotificationConstants.NotificationCreatedSuccess, data = await _notificationService.CreateNotificationAsync(notificationDto) })
                : BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        [HttpGet("{notificationId}")]
        public async Task<IActionResult> GetNotificationById(long notificationId)
        {
            return Ok(new { message = GeneralConstants.OperationSuccess, data = await _notificationService.GetNotificationByIdAsync(notificationId) });
        }

        [HttpDelete("{notificationId}")]
        public async Task<IActionResult> DeleteNotification(long notificationId)
        {
            return await _notificationService.DeleteNotificationAsync(notificationId)
                ? Ok(new { message = NotificationConstants.NotificationDeleteSuccess })
                : BadRequest(new { message = NotificationConstants.NotificationNotFound });
        }

        [HttpPut("{notificationId}/read")]
        public async Task<IActionResult> MarkAsRead(long notificationId)
        {
            return Ok(new { message = NotificationConstants.NotificationMarkedAsRead, data = await _notificationService.MarkAsReadAsync(notificationId) });
        }

        [HttpPut("/api/v1/users/{userId}/notifications/read-all")]
        public async Task<IActionResult> MarkAllAsRead(long userId)
        {
            return await _notificationService.MarkAllAsReadAsync(userId)
                ? Ok(new { message = NotificationConstants.AllNotificationsMarkedAsRead })
                : BadRequest(new { message = GeneralConstants.OperationFailed });
        }
    }
}