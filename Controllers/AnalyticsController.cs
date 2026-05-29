using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers
{
    [ApiController]
    [Route("api/v1/analytics")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("kpi-reports")]
        public async Task<IActionResult> GetAllKPIReports([FromQuery] KPIReportSearchDto searchDto)
        {
            return ModelState.IsValid && searchDto != null
                ? Ok(new { message = GeneralConstants.OperationSuccess, data = await _analyticsService.GetAllKPIReportsAsync(searchDto) })
                : BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        [HttpPost("kpi-reports")]
        public async Task<IActionResult> CreateKPIReport([FromBody] KPIReportRequestDto reportDto)
        {
            return ModelState.IsValid && reportDto != null
                ? Ok(new { message = AnalyticsConstants.KPIReportGeneratedSuccess, data = await _analyticsService.CreateKPIReportAsync(reportDto) })
                : BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        [HttpGet("kpi-reports/{reportId}")]
        public async Task<IActionResult> GetKPIReportById(long reportId)
        {
            var report = await _analyticsService.GetKPIReportByIdAsync(reportId);
            return Ok(new { message = GeneralConstants.OperationSuccess, data = report });
        }

        [HttpDelete("kpi-reports/{reportId}")]
        public async Task<IActionResult> DeleteKPIReport(long reportId)
        {
            var result = await _analyticsService.DeleteKPIReportAsync(reportId);
            return result
                ? Ok(new { message = AnalyticsConstants.KPIReportDeletedSuccess })
                : BadRequest(new { message = AnalyticsConstants.KPIReportNotFound });
        }

        [HttpGet("dashboards/travel-spend")]
        public async Task<IActionResult> GetTravelSpendDashboard()
        {
            var dashboard = await _analyticsService.GetTravelSpendDashboardAsync();
            return Ok(new { message = AnalyticsConstants.DashboardDataRetrievedSuccess, data = dashboard });
        }

        [HttpGet("dashboards/booking-volume")]
        public async Task<IActionResult> GetBookingVolumeDashboard()
        {
            var dashboard = await _analyticsService.GetBookingVolumeDashboardAsync();
            return Ok(new { message = AnalyticsConstants.DashboardDataRetrievedSuccess, data = dashboard });
        }

        [HttpGet("dashboards/cancellations")]
        public async Task<IActionResult> GetCancellationDashboard()
        {
            var dashboard = await _analyticsService.GetCancellationDashboardAsync();
            return Ok(new { message = AnalyticsConstants.DashboardDataRetrievedSuccess, data = dashboard });
        }

        [HttpGet("trends/spend-per-traveler")]
        public async Task<IActionResult> GetSpendPerTravelerTrend()
        {
            var trend = await _analyticsService.GetSpendPerTravelerTrendAsync();
            return Ok(new { message = AnalyticsConstants.TrendAnalysisSuccess, data = trend });
        }

        [HttpGet("trends/destinations")]
        public async Task<IActionResult> GetDestinationTrend()
        {
            var trend = await _analyticsService.GetDestinationTrendAsync();
            return Ok(new { message = AnalyticsConstants.TrendAnalysisSuccess, data = trend });
        }
    }
}
