using TravelEaseServer.Dto;

namespace TravelEaseServer.Service.Interface
{
    public interface INotificationService
    {
        Task<NotificationResponseDto> CreateNotificationAsync(NotificationRequestDto notificationDto);
        Task<NotificationResponseDto> GetNotificationByIdAsync(long notificationId);
        Task<IEnumerable<NotificationResponseDto>> GetUserNotificationsAsync(long userId);
        Task<IEnumerable<NotificationResponseDto>> GetAllNotificationsAsync(NotificationSearchDto searchDto);
        Task<bool> DeleteNotificationAsync(long notificationId);
        Task<NotificationResponseDto> MarkAsReadAsync(long notificationId);
        Task<bool> MarkAllAsReadAsync(long userId);
    }
}
