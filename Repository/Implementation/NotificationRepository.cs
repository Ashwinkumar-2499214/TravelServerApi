using Microsoft.EntityFrameworkCore;
using TravelEaseServer.Dto;
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
            try
            {
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                return MapNotificationToDto(notification);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Error creating notification in database.", ex);
            }
        }

        public async Task<NotificationResponseDto> GetNotificationByIdAsync(long notificationId)
        {
            try
            {
                var notification = await _context.Notifications
                    .AsNoTracking()
                    .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

                return notification != null ? MapNotificationToDto(notification) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving notification with ID {notificationId}.", ex);
            }
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetAllNotificationsAsync(NotificationSearchDto searchDto)
        {
            try
            {
                var query = _context.Notifications.AsNoTracking();

                // Apply filters
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

                // Apply pagination
                int skip = (searchDto.PageNumber - 1) * searchDto.PageSize;
                var notifications = await query
                    .OrderByDescending(n => n.CreatedDate)
                    .Skip(skip)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                return notifications.Select(MapNotificationToDto);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving notifications.", ex);
            }
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetNotificationsByUserIdAsync(long userId)
        {
            try
            {
                var notifications = await _context.Notifications
                    .AsNoTracking()
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedDate)
                    .ToListAsync();

                return notifications.Select(MapNotificationToDto);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving notifications for user with ID {userId}.", ex);
            }
        }

        public async Task<bool> DeleteNotificationAsync(long notificationId)
        {
            try
            {
                var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationId == notificationId);
                if (notification == null)
                {
                    return false;
                }

                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error deleting notification with ID {notificationId}.", ex);
            }
        }

        public async Task<NotificationResponseDto> MarkAsReadAsync(long notificationId)
        {
            try
            {
                var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationId == notificationId);
                if (notification == null)
                {
                    throw new KeyNotFoundException($"Notification with ID {notificationId} not found.");
                }

                notification.Status = 1; // Assuming 1 is read status
                notification.ReadDate = DateTime.UtcNow;

                _context.Notifications.Update(notification);
                await _context.SaveChangesAsync();

                return MapNotificationToDto(notification);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error marking notification as read with ID {notificationId}.", ex);
            }
        }

        public async Task<bool> MarkAllAsReadAsync(long userId)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId && n.Status == 0)
                    .ToListAsync();

                if (notifications.Count == 0)
                {
                    return false;
                }

                foreach (var notification in notifications)
                {
                    notification.Status = 1; // Mark as read
                    notification.ReadDate = DateTime.UtcNow;
                }

                _context.Notifications.UpdateRange(notifications);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error marking all notifications as read for user with ID {userId}.", ex);
            }
        }

        private NotificationResponseDto MapNotificationToDto(Notification notification)
        {
            return new NotificationResponseDto
            {
                NotificationId = notification.NotificationId,
                UserId = notification.UserId,
                Message = notification.Message,
                Category = notification.Category,
                Status = notification.Status,
                CreatedDate = notification.CreatedDate,
                ReadDate = notification.ReadDate
            };
        }
    }
}
