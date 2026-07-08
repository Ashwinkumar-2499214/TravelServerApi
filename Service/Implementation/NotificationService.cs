using TravelEaseServer.Dto;
using TravelEaseServer.Enum;
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

        public async Task<NotificationResponseDto> CreateNotificationAsync(NotificationRequestDto dto)
        {
            var notification = new Notification
            {
                UserId = dto.UserId,
                Message = dto.Message,
                Category = dto.Category,
                Status = (int)NotificationStatus.Unread,
                CreatedDate = DateTime.UtcNow
            };
            return await _notificationRepository.CreateNotificationAsync(notification);
        }

        public Task<NotificationResponseDto?> GetNotificationByIdAsync(long notificationId) =>
            _notificationRepository.GetNotificationByIdAsync(notificationId);

        public Task<IEnumerable<NotificationResponseDto>> GetUserNotificationsAsync(long userId) =>
            _notificationRepository.GetNotificationsByUserIdAsync(userId);

        public Task<IEnumerable<NotificationResponseDto>> GetAllNotificationsAsync(NotificationSearchDto searchDto) =>
            _notificationRepository.GetAllNotificationsAsync(searchDto);

        public Task<bool> DeleteNotificationAsync(long notificationId) =>
            _notificationRepository.DeleteNotificationAsync(notificationId);

        public Task<NotificationResponseDto> MarkAsReadAsync(long notificationId) =>
            _notificationRepository.MarkAsReadAsync(notificationId);

        public Task<bool> MarkAllAsReadAsync(long userId) =>
            _notificationRepository.MarkAllAsReadAsync(userId);

        private Task<NotificationResponseDto> Trigger(long userId, string message, NotificationCategory category) =>
            CreateNotificationAsync(new NotificationRequestDto { UserId = userId, Message = message, Category = (int)category });

        public Task<NotificationResponseDto> TriggerAuthenticationNotificationAsync(long userId, string message, NotificationCategory category) => Trigger(userId, message, category);
        public Task<NotificationResponseDto> TriggerBookingNotificationAsync(long userId, string message, NotificationCategory category) => Trigger(userId, message, category);
        public Task<NotificationResponseDto> TriggerPaymentNotificationAsync(long userId, string message, NotificationCategory category) => Trigger(userId, message, category);
        public Task<NotificationResponseDto> TriggerReservationNotificationAsync(long userId, string message, NotificationCategory category) => Trigger(userId, message, category);
        public Task<NotificationResponseDto> TriggerItineraryNotificationAsync(long userId, string message, NotificationCategory category) => Trigger(userId, message, category);
        public Task<NotificationResponseDto> TriggerInvoiceNotificationAsync(long userId, string message, NotificationCategory category) => Trigger(userId, message, category);
    }
}
