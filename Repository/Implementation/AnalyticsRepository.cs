using Microsoft.EntityFrameworkCore;
using TravelEaseServer.Dto;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;

namespace TravelEaseServer.Repository.Implementation
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly AppDbContext _context;

        public AnalyticsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<KPIReportResponseDto> CreateKPIReportAsync(KPIReport report)
        {
            try
            {
                _context.KPIReports.Add(report);
                await _context.SaveChangesAsync();
                return MapKPIReportToDto(report);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Error creating KPI report in database.", ex);
            }
        }

        public async Task<KPIReportResponseDto> GetKPIReportByIdAsync(long reportId)
        {
            try
            {
                var report = await _context.KPIReports
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.KPIReportId == reportId);

                return report != null ? MapKPIReportToDto(report) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving KPI report with ID {reportId}.", ex);
            }
        }

        public async Task<IEnumerable<KPIReportResponseDto>> GetAllKPIReportsAsync(KPIReportSearchDto searchDto)
        {
            try
            {
                var query = _context.KPIReports.AsNoTracking();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
                {
                    var term = searchDto.SearchTerm.ToLower();
                    query = query.Where(r => r.Title.ToLower().Contains(term) ||
                                           r.Scope.ToLower().Contains(term));
                }

                if (searchDto.FromDate.HasValue)
                {
                    query = query.Where(r => r.GeneratedDate >= searchDto.FromDate.Value);
                }

                if (searchDto.ToDate.HasValue)
                {
                    query = query.Where(r => r.GeneratedDate <= searchDto.ToDate.Value);
                }

                // Apply pagination
                int skip = (searchDto.PageNumber - 1) * searchDto.PageSize;
                var reports = await query
                    .OrderByDescending(r => r.GeneratedDate)
                    .Skip(skip)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                return reports.Select(MapKPIReportToDto);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving KPI reports.", ex);
            }
        }

        public async Task<bool> DeleteKPIReportAsync(long reportId)
        {
            try
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
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error deleting KPI report with ID {reportId}.", ex);
            }
        }

        public async Task<DashboardDataDto> GetTravelSpendDashboardAsync(string filter)
        {
            try
            {
                var (from, to, period) = GetDateRange(filter);
                var totalSpend = await _context.Bookings.AsNoTracking()
                    .Where(b => b.CreatedDate >= from && b.CreatedDate <= to)
                    .SumAsync(b => (decimal?)b.Amount) ?? 0;
                var bookingCount = await _context.Bookings.AsNoTracking()
                    .Where(b => b.CreatedDate >= from && b.CreatedDate <= to)
                    .CountAsync();
                return new DashboardDataDto { Title = "Travel Spend", TotalAmount = totalSpend, TotalCount = bookingCount, Period = period, Data = new { CurrencyCode = "USD" } };
            }
            catch (Exception ex) { throw new InvalidOperationException("Error retrieving travel spend dashboard data.", ex); }
        }

        public async Task<DashboardDataDto> GetBookingVolumeDashboardAsync(string filter)
        {
            try
            {
                var (from, to, period) = GetDateRange(filter);
                var totalBookings = await _context.Bookings.AsNoTracking()
                    .Where(b => b.CreatedDate >= from && b.CreatedDate <= to)
                    .CountAsync();
                var confirmedBookings = await _context.Bookings.AsNoTracking()
                    .Where(b => b.CreatedDate >= from && b.CreatedDate <= to && b.Status == 1)
                    .CountAsync();
                return new DashboardDataDto { Title = "Booking Volume", TotalAmount = totalBookings, TotalCount = confirmedBookings, Period = period, Data = new { ConfirmationRate = totalBookings > 0 ? Math.Round((decimal)confirmedBookings / totalBookings * 100, 1) : 0 } };
            }
            catch (Exception ex) { throw new InvalidOperationException("Error retrieving booking volume dashboard data.", ex); }
        }

        public async Task<DashboardDataDto> GetCancellationDashboardAsync(string filter)
        {
            try
            {
                var (from, to, period) = GetDateRange(filter);
                var totalBookings = await _context.Bookings.AsNoTracking()
                    .Where(b => b.CreatedDate >= from && b.CreatedDate <= to)
                    .CountAsync();
                var cancelledBookings = await _context.Bookings.AsNoTracking()
                    .Where(b => b.CreatedDate >= from && b.CreatedDate <= to && b.Status == 3)
                    .CountAsync();
                return new DashboardDataDto { Title = "Cancellations", TotalAmount = cancelledBookings, TotalCount = totalBookings, Period = period, Data = new { CancellationRate = totalBookings > 0 ? Math.Round((decimal)cancelledBookings / totalBookings * 100, 1) : 0 } };
            }
            catch (Exception ex) { throw new InvalidOperationException("Error retrieving cancellation dashboard data.", ex); }
        }

        public async Task<DashboardDataDto> GetAvgBookingValueDashboardAsync(string filter)
        {
            try
            {
                var (from, to, period) = GetDateRange(filter);
                var avg = await _context.Bookings.AsNoTracking()
                    .Where(b => b.CreatedDate >= from && b.CreatedDate <= to)
                    .AverageAsync(b => (decimal?)b.Amount) ?? 0;
                var count = await _context.Bookings.AsNoTracking()
                    .Where(b => b.CreatedDate >= from && b.CreatedDate <= to)
                    .CountAsync();
                return new DashboardDataDto { Title = "Avg Booking Value", TotalAmount = Math.Round(avg, 2), TotalCount = count, Period = period, Data = new { } };
            }
            catch (Exception ex) { throw new InvalidOperationException("Error retrieving avg booking value.", ex); }
        }

        public async Task<DashboardDataDto> GetTopSpendersDashboardAsync(string filter)
        {
            try
            {
                var (from, to, period) = GetDateRange(filter);
                var topSpenders = await _context.Bookings.AsNoTracking()
                    .Where(b => b.CreatedDate >= from && b.CreatedDate <= to)
                    .GroupBy(b => b.UserId)
                    .Select(g => new { UserId = g.Key, TotalSpend = g.Sum(b => b.Amount), BookingCount = g.Count() })
                    .OrderByDescending(x => x.TotalSpend)
                    .Take(5)
                    .ToListAsync();
                var uniqueSpenders = topSpenders.Count;
                var totalSpend = topSpenders.Sum(x => x.TotalSpend);
                return new DashboardDataDto { Title = "Top Spenders", TotalAmount = totalSpend, TotalCount = uniqueSpenders, Period = period, Data = topSpenders };
            }
            catch (Exception ex) { throw new InvalidOperationException("Error retrieving top spenders.", ex); }
        }

        public async Task<DashboardDataDto> GetRevenueByTypeDashboardAsync(string filter)
        {
            try
            {
                var (from, to, period) = GetDateRange(filter);
                var byType = await _context.Bookings.AsNoTracking()
                    .Where(b => b.CreatedDate >= from && b.CreatedDate <= to)
                    .GroupBy(b => b.ItemType)
                    .Select(g => new { ItemType = g.Key, Revenue = g.Sum(b => b.Amount), Count = g.Count() })
                    .OrderByDescending(x => x.Revenue)
                    .ToListAsync();
                var totalRevenue = byType.Sum(x => x.Revenue);
                return new DashboardDataDto { Title = "Revenue by Type", TotalAmount = totalRevenue, TotalCount = byType.Count, Period = period, Data = byType };
            }
            catch (Exception ex) { throw new InvalidOperationException("Error retrieving revenue by type.", ex); }
        }

        public async Task<TrendAnalysisDto> GetSpendPerTravelerTrendAsync()
        {
            try
            {
                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;

                var totalSpend = await _context.Bookings
                    .AsNoTracking()
                    .Where(b => b.CreatedDate.Month == currentMonth && b.CreatedDate.Year == currentYear)
                    .SumAsync(b => b.Amount);

                var totalUsers = await _context.Bookings
                    .AsNoTracking()
                    .Where(b => b.CreatedDate.Month == currentMonth && b.CreatedDate.Year == currentYear)
                    .Select(b => b.UserId)
                    .Distinct()
                    .CountAsync();

                var averageSpend = totalUsers > 0 ? totalSpend / totalUsers : 0;

                return new TrendAnalysisDto
                {
                    Title = "Spend Per Traveler Trend",
                    TrendType = "Average Spend",
                    Period = DateTime.UtcNow,
                    TrendData = new { AverageSpendPerTraveler = averageSpend, TotalTravelers = totalUsers }
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving spend per traveler trend.", ex);
            }
        }

        public async Task<TrendAnalysisDto> GetDestinationTrendAsync()
        {
            try
            {
                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;

                var destinationTrends = await _context.Bookings
                    .AsNoTracking()
                    .Where(b => b.CreatedDate.Month == currentMonth && b.CreatedDate.Year == currentYear)
                    .GroupBy(b => b.ItemType)
                    .Select(g => new { ItemType = g.Key, Count = g.Count(), TotalAmount = g.Sum(b => b.Amount) })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToListAsync();

                return new TrendAnalysisDto
                {
                    Title = "Destination Trend",
                    TrendType = "Top Destinations",
                    Period = DateTime.UtcNow,
                    TrendData = destinationTrends
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving destination trend.", ex);
            }
        }

        private KPIReportResponseDto MapKPIReportToDto(KPIReport report)
        {
            return new KPIReportResponseDto
            {
                KPIReportId = report.KPIReportId,
                Title = report.Title,
                Scope = report.Scope,
                Metrics = report.Metrics,
                GeneratedDate = report.GeneratedDate,
                ReportContent = report.ReportContent
            };
        }

        private static (DateTime from, DateTime to, string period) GetDateRange(string filter)
        {
            var now = DateTime.UtcNow;
            if (filter == "week")
            {
                var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
                return (startOfWeek, startOfWeek.AddDays(7).AddTicks(-1), $"Week of {startOfWeek:yyyy-MM-dd}");
            }
            var from = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var to = from.AddMonths(1).AddTicks(-1);
            return (from, to, $"{now.Year}-{now.Month:D2}");
        }
    }
}
