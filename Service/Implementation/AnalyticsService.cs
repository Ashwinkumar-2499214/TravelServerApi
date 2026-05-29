using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Service.Implementation
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsRepository _analyticsRepository;

        public AnalyticsService(IAnalyticsRepository analyticsRepository)
        {
            _analyticsRepository = analyticsRepository;
        }

        public async Task<KPIReportResponseDto> CreateKPIReportAsync(KPIReportRequestDto reportDto)
        {
            try
            {
                var report = new KPIReport
                {
                    Title = reportDto.Title,
                    Scope = reportDto.Scope,
                    Metrics = reportDto.Metrics,
                    GeneratedDate = DateTime.UtcNow,
                    ReportContent = reportDto.ReportContent
                };

                return await _analyticsRepository.CreateKPIReportAsync(report);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(AnalyticsConstants.KPIReportGeneratedSuccess, ex);
            }
        }

        public async Task<KPIReportResponseDto> GetKPIReportByIdAsync(long reportId)
        {
            return await _analyticsRepository.GetKPIReportByIdAsync(reportId);
        }

        public async Task<IEnumerable<KPIReportResponseDto>> GetAllKPIReportsAsync(KPIReportSearchDto searchDto)
        {
            return await _analyticsRepository.GetAllKPIReportsAsync(searchDto);
        }

        public async Task<bool> DeleteKPIReportAsync(long reportId)
        {
            return await _analyticsRepository.DeleteKPIReportAsync(reportId);
        }

        public async Task<DashboardDataDto> GetTravelSpendDashboardAsync()
        {
            return await _analyticsRepository.GetTravelSpendDashboardAsync();
        }

        public async Task<DashboardDataDto> GetBookingVolumeDashboardAsync()
        {
            return await _analyticsRepository.GetBookingVolumeDashboardAsync();
        }

        public async Task<DashboardDataDto> GetCancellationDashboardAsync()
        {
            return await _analyticsRepository.GetCancellationDashboardAsync();
        }

        public async Task<TrendAnalysisDto> GetSpendPerTravelerTrendAsync()
        {
            return await _analyticsRepository.GetSpendPerTravelerTrendAsync();
        }

        public async Task<TrendAnalysisDto> GetDestinationTrendAsync()
        {
            return await _analyticsRepository.GetDestinationTrendAsync();
        }
    }
}
