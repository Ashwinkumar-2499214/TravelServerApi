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
                var title = reportDto.Title ?? string.Empty;
                var titleLower = title.ToLower();

                string scope, metrics, reportContent;

                if (titleLower.Contains("spend") || titleLower.Contains("travel spend"))
                {
                    scope = "Monthly travel expenditure across all bookings and travelers.";
                    metrics = "Total spend, average spend per traveler, number of bookings, currency breakdown.";
                    reportContent = "This report summarises total travel spend for the current period. It includes a breakdown of spend by booking type, average cost per traveler, and highlights any significant deviations from the previous period.";
                }
                else if (titleLower.Contains("booking") || titleLower.Contains("performance"))
                {
                    scope = "Booking activity and confirmation rates for the current reporting period.";
                    metrics = "Total bookings, confirmed bookings, confirmation rate percentage, booking volume trend.";
                    reportContent = "This report covers booking performance metrics including total volume, confirmation rates, and status distribution. It identifies patterns in booking behaviour and highlights periods of high or low activity.";
                }
                else if (titleLower.Contains("cancellation") || titleLower.Contains("rate"))
                {
                    scope = "Cancellation activity and rate analysis for the current reporting period.";
                    metrics = "Total cancellations, cancellation rate, comparison to total bookings, trend over time.";
                    reportContent = "This report analyses cancellation trends including total cancelled bookings, the cancellation rate as a percentage of all bookings, and identifies any patterns or anomalies that may require operational attention.";
                }
                else
                {
                    scope = "General analytics overview for the current reporting period.";
                    metrics = "Key performance indicators across bookings, spend, and traveler activity.";
                    reportContent = "This general KPI report provides an overview of system-wide analytics including booking volumes, travel spend, and traveler engagement metrics for the selected period.";
                }

                var report = new KPIReport
                {
                    Title = title,
                    Scope = scope,
                    Metrics = metrics,
                    GeneratedDate = DateTime.UtcNow,
                    ReportContent = reportContent
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

        public async Task<DashboardDataDto> GetTravelSpendDashboardAsync(string filter)
            => await _analyticsRepository.GetTravelSpendDashboardAsync(filter);

        public async Task<DashboardDataDto> GetBookingVolumeDashboardAsync(string filter)
            => await _analyticsRepository.GetBookingVolumeDashboardAsync(filter);

        public async Task<DashboardDataDto> GetCancellationDashboardAsync(string filter)
            => await _analyticsRepository.GetCancellationDashboardAsync(filter);

        public async Task<DashboardDataDto> GetAvgBookingValueDashboardAsync(string filter)
            => await _analyticsRepository.GetAvgBookingValueDashboardAsync(filter);

        public async Task<DashboardDataDto> GetTopSpendersDashboardAsync(string filter)
            => await _analyticsRepository.GetTopSpendersDashboardAsync(filter);

        public async Task<DashboardDataDto> GetRevenueByTypeDashboardAsync(string filter)
            => await _analyticsRepository.GetRevenueByTypeDashboardAsync(filter);

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
