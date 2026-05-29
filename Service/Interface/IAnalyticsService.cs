using TravelEaseServer.Dto;

namespace TravelEaseServer.Service.Interface
{
    public interface IAnalyticsService
    {
        Task<KPIReportResponseDto> CreateKPIReportAsync(KPIReportRequestDto reportDto);
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
