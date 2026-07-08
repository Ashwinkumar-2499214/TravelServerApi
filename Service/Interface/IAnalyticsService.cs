using TravelEaseServer.Dto;

namespace TravelEaseServer.Service.Interface
{
    public interface IAnalyticsService
    {
        Task<KPIReportResponseDto> CreateKPIReportAsync(KPIReportRequestDto reportDto);
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
