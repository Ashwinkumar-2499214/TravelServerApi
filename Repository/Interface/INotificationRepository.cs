using TravelEaseServer.Dto;
using TravelEaseServer.Model;

namespace TravelEaseServer.Repository.Interface
{
    public interface INotificationRepository
    {
        Task<NotificationResponseDto> CreateNotificationAsync(Notification notification);
        Task<NotificationResponseDto> GetNotificationByIdAsync(long notificationId);
        Task<IEnumerable<NotificationResponseDto>> GetAllNotificationsAsync(NotificationSearchDto searchDto);
        Task<IEnumerable<NotificationResponseDto>> GetNotificationsByUserIdAsync(long userId);
        Task<bool> DeleteNotificationAsync(long notificationId);
        Task<NotificationResponseDto> MarkAsReadAsync(long notificationId);
        Task<bool> MarkAllAsReadAsync(long userId);
    }
}
