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
        Task<DashboardDataDto> GetTravelSpendDashboardAsync();
        Task<DashboardDataDto> GetBookingVolumeDashboardAsync();
        Task<DashboardDataDto> GetCancellationDashboardAsync();
        Task<TrendAnalysisDto> GetSpendPerTravelerTrendAsync();
        Task<TrendAnalysisDto> GetDestinationTrendAsync();
    }
}
