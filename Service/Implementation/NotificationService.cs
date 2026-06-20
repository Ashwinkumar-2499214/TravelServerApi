using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Service.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<NotificationResponseDto> CreateNotificationAsync(NotificationRequestDto notificationDto)
        {
            var notification = new Notification
            {
                UserId = notificationDto.UserId,
                Message = notificationDto.Message,
                Category = notificationDto.Category,
                Status = (int)Enum.NotificationStatus.Unread,
                CreatedDate = DateTime.UtcNow
            };

            return await _notificationRepository.CreateNotificationAsync(notification);
        }

        public async Task<NotificationResponseDto?> GetNotificationByIdAsync(long notificationId)
        {
            return await _notificationRepository.GetNotificationByIdAsync(notificationId);
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetUserNotificationsAsync(long userId)
        {
            return await _notificationRepository.GetNotificationsByUserIdAsync(userId);
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetAllNotificationsAsync(NotificationSearchDto searchDto)
        {
            return await _notificationRepository.GetAllNotificationsAsync(searchDto);
        }

        public async Task<bool> DeleteNotificationAsync(long notificationId)
        {
            return await _notificationRepository.DeleteNotificationAsync(notificationId);
        }

        public async Task<NotificationResponseDto> MarkAsReadAsync(long notificationId)
        {
            return await _notificationRepository.MarkAsReadAsync(notificationId);
        }

        public async Task<bool> MarkAllAsReadAsync(long userId)
        {
            return await _notificationRepository.MarkAllAsReadAsync(userId);
        }
    }
}