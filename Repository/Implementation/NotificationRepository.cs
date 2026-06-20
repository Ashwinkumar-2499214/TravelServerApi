using Microsoft.EntityFrameworkCore;
using TravelEaseServer.Dto;
using TravelEaseServer.Enum;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;

namespace TravelEaseServer.Repository.Implementation
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        private const int StatusUnread = 0;
        private const int StatusRead = 1;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<NotificationResponseDto> CreateNotificationAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return MapNotificationToDto(notification);
        }

        public async Task<NotificationResponseDto?> GetNotificationByIdAsync(long notificationId)
        {
            var notification = await _context.Notifications
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

            return notification != null ? MapNotificationToDto(notification) : null;
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetAllNotificationsAsync(NotificationSearchDto searchDto)
        {
            var query = _context.Notifications.AsNoTracking();

            if (searchDto.UserId.HasValue)
            {
                query = query.Where(n => n.UserId == searchDto.UserId.Value);
            }

            if (searchDto.Category.HasValue)
            {
                query = query.Where(n => n.Category == searchDto.Category.Value);
            }

            if (searchDto.Status.HasValue)
            {
                query = query.Where(n => n.Status == searchDto.Status.Value);
            }

            int skip = (searchDto.PageNumber - 1) * searchDto.PageSize;
            var notifications = await query
                .OrderByDescending(n => n.CreatedDate)
                .Skip(skip)
                .Take(searchDto.PageSize)
                .ToListAsync();

            return notifications.Select(MapNotificationToDto);
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetNotificationsByUserIdAsync(long userId)
        {
            var notifications = await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

            return notifications.Select(MapNotificationToDto);
        }

        public async Task<bool> DeleteNotificationAsync(long notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

            if (notification == null)
            {
                return false;
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<NotificationResponseDto> MarkAsReadAsync(long notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

            if (notification == null)
            {
                throw new KeyNotFoundException($"Notification with ID {notificationId} not found.");
            }

            notification.Status = (int)NotificationStatus.Read;
            notification.ReadDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapNotificationToDto(notification);
        }

        public async Task<bool> MarkAllAsReadAsync(long userId)
        {
            await _context.Notifications
                .Where(n => n.UserId == userId && n.Status == StatusUnread)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(n => n.Status, StatusRead)
                    .SetProperty(n => n.ReadDate, DateTime.UtcNow));

            return true;
        }

        private static NotificationResponseDto MapNotificationToDto(Notification notification)
        {
            return new NotificationResponseDto
            {
                NotificationId = notification.NotificationId,
                UserId = notification.UserId,
                Message = notification.Message,
                Category = (NotificationCategory)notification.Category,
                Status = (NotificationStatus)notification.Status,
                CreatedDate = notification.CreatedDate,
                ReadDate = notification.ReadDate
            };
        }
    }
}