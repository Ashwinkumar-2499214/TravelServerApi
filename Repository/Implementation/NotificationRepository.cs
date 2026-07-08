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

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<NotificationResponseDto> CreateNotificationAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var userName = await _context.Users
                .AsNoTracking()
                .Where(u => u.UserId == notification.UserId)
                .Select(u => u.Name)
                .FirstOrDefaultAsync() ?? "Unknown";

            return MapToDto(notification, userName);
        }

        public async Task<NotificationResponseDto?> GetNotificationByIdAsync(long notificationId)
        {
            var result = await _context.Notifications
                .AsNoTracking()
                .Where(n => n.NotificationId == notificationId)
                .Join(_context.Users.AsNoTracking(),
                    n => n.UserId,
                    u => u.UserId,
                    (n, u) => new { n, u.Name })
                .FirstOrDefaultAsync();

            return result != null ? MapToDto(result.n, result.Name) : null;
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetAllNotificationsAsync(NotificationSearchDto searchDto)
        {
            var query = _context.Notifications.AsNoTracking();

            if (searchDto.UserId.HasValue)
                query = query.Where(n => n.UserId == searchDto.UserId.Value);

            if (searchDto.Category.HasValue)
                query = query.Where(n => n.Category == searchDto.Category.Value);

            if (searchDto.Status.HasValue)
                query = query.Where(n => n.Status == searchDto.Status.Value);

            int skip = (searchDto.PageNumber - 1) * searchDto.PageSize;

            var results = await query
                .OrderByDescending(n => n.CreatedDate)
                .Skip(skip)
                .Take(searchDto.PageSize)
                .Join(_context.Users.AsNoTracking(),
                    n => n.UserId,
                    u => u.UserId,
                    (n, u) => new { n, u.Name })
                .ToListAsync();

            return results.Select(r => MapToDto(r.n, r.Name));
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetNotificationsByUserIdAsync(long userId)
        {
            var userName = await _context.Users
                .AsNoTracking()
                .Where(u => u.UserId == userId)
                .Select(u => u.Name)
                .FirstOrDefaultAsync() ?? "Unknown";

            var notifications = await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

            return notifications.Select(n => MapToDto(n, userName));
        }

        public async Task<bool> DeleteNotificationAsync(long notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

            if (notification == null) return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<NotificationResponseDto> MarkAsReadAsync(long notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId)
                ?? throw new KeyNotFoundException($"Notification with ID {notificationId} not found.");

            notification.Status = (int)NotificationStatus.Read;
            notification.ReadDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var userName = await _context.Users
                .AsNoTracking()
                .Where(u => u.UserId == notification.UserId)
                .Select(u => u.Name)
                .FirstOrDefaultAsync() ?? "Unknown";

            return MapToDto(notification, userName);
        }

        public async Task<bool> MarkAllAsReadAsync(long userId)
        {
            await _context.Notifications
                .Where(n => n.UserId == userId && n.Status == (int)NotificationStatus.Unread)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.Status, (int)NotificationStatus.Read)
                    .SetProperty(n => n.ReadDate, DateTime.UtcNow));

            return true;
        }

        private static NotificationResponseDto MapToDto(Notification n, string userName) => new()
        {
            NotificationId = n.NotificationId,
            UserId = n.UserId,
            UserName = userName,
            Message = n.Message,
            Category = (NotificationCategory)n.Category,
            Status = (NotificationStatus)n.Status,
            CreatedDate = n.CreatedDate,
            ReadDate = n.ReadDate
        };
    }
}
