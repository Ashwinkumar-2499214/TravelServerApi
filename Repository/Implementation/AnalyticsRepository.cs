using Microsoft.EntityFrameworkCore;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;

namespace TravelEaseServer.Repository.Implementation;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly AppDbContext _context;

    public AnalyticsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<KPIReport> CreateKPIReportAsync(KPIReport report)
    {
        _context.KPIReports.Add(report);
        await _context.SaveChangesAsync();
        return report;
    }

    public async Task<KPIReport?> GetKPIReportByIdAsync(long reportId)
    {
        return await _context.KPIReports
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.KPIReportId == reportId);
    }

    public async Task<IEnumerable<KPIReport>> GetAllKPIReportsAsync(string? searchTerm, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize)
    {
        var query = _context.KPIReports.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(r => r.Title.ToLower().Contains(term) || r.Scope.ToLower().Contains(term));
        }

        if (fromDate.HasValue)
        {
            query = query.Where(r => r.GeneratedDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(r => r.GeneratedDate <= toDate.Value);
        }

        int skip = (pageNumber - 1) * pageSize;

        return await query
            .OrderByDescending(r => r.GeneratedDate)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> DeleteKPIReportAsync(long reportId)
    {
        var report = await _context.KPIReports.FirstOrDefaultAsync(r => r.KPIReportId == reportId);
        if (report == null)
        {
            return false;
        }

        _context.KPIReports.Remove(report);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(decimal TotalSpend, int BookingCount)> GetTravelSpendDashboardMetricsAsync()
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var startOfNextMonth = startOfMonth.AddMonths(1);

        var metrics = await _context.Bookings
            .AsNoTracking()
            .Where(b => b.CreatedDate >= startOfMonth && b.CreatedDate < startOfNextMonth)
            .GroupBy(_ => 1)
            .Select(g => new 
            {
                TotalSpend = g.Sum(b => b.Amount),
                BookingCount = g.Count()
            })
            .FirstOrDefaultAsync();

        return metrics != null ? (metrics.TotalSpend, metrics.BookingCount) : (0, 0);
    }

    public async Task<(int TotalBookings, int ConfirmedBookings)> GetBookingVolumeDashboardMetricsAsync()
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var startOfNextMonth = startOfMonth.AddMonths(1);

        var metrics = await _context.Bookings
            .AsNoTracking()
            .Where(b => b.CreatedDate >= startOfMonth && b.CreatedDate < startOfNextMonth)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Confirmed = g.Count(b => b.Status == 1)
            })
            .FirstOrDefaultAsync();

        return metrics != null ? (metrics.Total, metrics.Confirmed) : (0, 0);
    }

    public async Task<(int CancelledBookings, int TotalBookings)> GetCancellationDashboardMetricsAsync()
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var startOfNextMonth = startOfMonth.AddMonths(1);

        var metrics = await _context.Bookings
            .AsNoTracking()
            .Where(b => b.CreatedDate >= startOfMonth && b.CreatedDate < startOfNextMonth)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Cancelled = g.Count(b => b.Status == 3),
                Total = g.Count()
            })
            .FirstOrDefaultAsync();

        return metrics != null ? (metrics.Cancelled, metrics.Total) : (0, 0);
    }

    public async Task<(decimal TotalSpend, int DistinctUsers)> GetSpendPerTravelerTrendMetricsAsync()
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var startOfNextMonth = startOfMonth.AddMonths(1);

        var totalSpend = await _context.Bookings
            .AsNoTracking()
            .Where(b => b.CreatedDate >= startOfMonth && b.CreatedDate < startOfNextMonth)
            .SumAsync(b => b.Amount);

        var totalUsers = await _context.Bookings
            .AsNoTracking()
            .Where(b => b.CreatedDate >= startOfMonth && b.CreatedDate < startOfNextMonth)
            .Select(b => b.UserId)
            .Distinct()
            .CountAsync();

        return (totalSpend, totalUsers);
    }

    public async Task<IEnumerable<object>> GetDestinationTrendDataAsync()
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var startOfNextMonth = startOfMonth.AddMonths(1);

        return await _context.Bookings
            .AsNoTracking()
            .Where(b => b.CreatedDate >= startOfMonth && b.CreatedDate < startOfNextMonth)
            .GroupBy(b => b.ItemType)
            .Select(g => new { ItemType = g.Key, Count = g.Count(), TotalAmount = g.Sum(b => b.Amount) })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync<object>();
    }
}