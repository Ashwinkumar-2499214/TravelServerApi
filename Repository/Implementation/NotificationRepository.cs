using Microsoft.EntityFrameworkCore;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;

namespace TravelEaseServer.Repository.Implementation;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Notification> CreateNotificationAsync(Notification notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<Notification?> GetNotificationByIdAsync(long notificationId)
    {
        return await _context.Notifications
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId);
    }

    public async Task<IEnumerable<Notification>> GetAllNotificationsAsync(long? userId, int? category, int? status, int pageNumber, int pageSize)
    {
        var query = _context.Notifications.AsNoTracking();

        if (userId.HasValue)
        {
            query = query.Where(n => n.UserId == userId.Value);
        }

        if (category.HasValue)
        {
            query = query.Where(n => n.Category == category.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(n => n.Status == status.Value);
        }

        int skip = (pageNumber - 1) * pageSize;

        return await query
            .OrderByDescending(n => n.CreatedDate)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(long userId)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
    }

    public async Task<bool> DeleteNotificationAsync(long notificationId)
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

    public async Task<Notification?> MarkAsReadAsync(long notificationId)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationId == notificationId);
        if (notification == null)
        {
            return null;
        }

        notification.Status = 1;
        notification.ReadDate = DateTime.UtcNow;

        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();

        return notification;
    }

    public async Task<bool> MarkAllAsReadAsync(long userId)
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
            notification.Status = 1;
            notification.ReadDate = DateTime.UtcNow;
        }

        _context.Notifications.UpdateRange(notifications);
        await _context.SaveChangesAsync();
        return true;
    }
}