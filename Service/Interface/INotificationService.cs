using TravelEaseServer.Dto;
using TravelEaseServer.Enum;

namespace TravelEaseServer.Service.Interface
{
    public interface INotificationService
    {
        Task<NotificationResponseDto> CreateNotificationAsync(NotificationRequestDto notificationDto);
        Task<NotificationResponseDto?> GetNotificationByIdAsync(long notificationId);
        Task<IEnumerable<NotificationResponseDto>> GetUserNotificationsAsync(long userId);
        Task<IEnumerable<NotificationResponseDto>> GetAllNotificationsAsync(NotificationSearchDto searchDto);
        Task<bool> DeleteNotificationAsync(long notificationId);
        Task<NotificationResponseDto> MarkAsReadAsync(long notificationId);
        Task<bool> MarkAllAsReadAsync(long userId);

        // Trigger notification methods for different operations
        Task<NotificationResponseDto> TriggerAuthenticationNotificationAsync(long userId, string message, NotificationCategory category);
        Task<NotificationResponseDto> TriggerBookingNotificationAsync(long userId, string message, NotificationCategory category);
        Task<NotificationResponseDto> TriggerPaymentNotificationAsync(long userId, string message, NotificationCategory category);
        Task<NotificationResponseDto> TriggerReservationNotificationAsync(long userId, string message, NotificationCategory category);
        Task<NotificationResponseDto> TriggerItineraryNotificationAsync(long userId, string message, NotificationCategory category);
        Task<NotificationResponseDto> TriggerInvoiceNotificationAsync(long userId, string message, NotificationCategory category);
    }
}
