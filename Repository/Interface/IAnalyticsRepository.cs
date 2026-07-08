using TravelEaseServer.Dto;
using TravelEaseServer.Model;

namespace TravelEaseServer.Repository.Interface
{
    public interface IAnalyticsRepository
    {
        Task<KPIReportResponseDto> CreateKPIReportAsync(KPIReport report);
        Task<KPIReportResponseDto> GetKPIReportByIdAsync(long reportId);
        Task<IEnumerable<KPIReportResponseDto>> GetAllKPIReportsAsync(KPIReportSearchDto searchDto);
        Task<bool> DeleteKPIReportAsync(long reportId);
        Task<DashboardDataDto> GetTravelSpendDashboardAsync(string filter);
        Task<DashboardDataDto> GetBookingVolumeDashboardAsync(string filter);
        Task<DashboardDataDto> GetCancellationDashboardAsync(string filter);
        Task<DashboardDataDto> GetAvgBookingValueDashboardAsync(string filter);
        Task<DashboardDataDto> GetTopSpendersDashboardAsync(string filter);
        Task<DashboardDataDto> GetRevenueByTypeDashboardAsync(string filter);
        Task<TrendAnalysisDto> GetSpendPerTravelerTrendAsync();
        Task<TrendAnalysisDto> GetDestinationTrendAsync();
    }
}
