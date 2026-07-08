using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers;

[ApiController]
[Route("api/v1/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("kpi-reports")]
    [Authorize(Roles = "Admin,CorporateTravelManager,ComplianceOfficer")]
    public async Task<IActionResult> GetAllKPIReports([FromQuery] KPIReportSearchDto searchDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _analyticsService.GetAllKPIReportsAsync(searchDto);

        return data != null
            ? Ok(new { message = GeneralConstants.OperationSuccess, data })
            : NotFound(new { message = AnalyticsConstants.KPIReportNotFound });
    }

    [HttpPost("kpi-reports")]
    [Authorize(Roles = "Admin,CorporateTravelManager")]
    public async Task<IActionResult> CreateKPIReport([FromBody] KPIReportRequestDto reportDto)
    {
        if (!ModelState.IsValid || reportDto == null)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _analyticsService.CreateKPIReportAsync(reportDto);

        return data != null
            ? Ok(new { message = AnalyticsConstants.KPIReportGeneratedSuccess, data })
            : BadRequest(new { message = GeneralConstants.InvalidInput });
    }

    [HttpGet("kpi-reports/{reportId}")]
    [Authorize(Roles = "Admin,CorporateTravelManager,ComplianceOfficer")]
    public async Task<IActionResult> GetKPIReportById(long reportId)
    {
        if (reportId <= 0)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _analyticsService.GetKPIReportByIdAsync(reportId);

        return data != null
            ? Ok(new { message = GeneralConstants.OperationSuccess, data })
            : NotFound(new { message = AnalyticsConstants.KPIReportNotFound });
    }

    [HttpDelete("kpi-reports/{reportId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteKPIReport(long reportId)
    {
        if (reportId <= 0)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var result = await _analyticsService.DeleteKPIReportAsync(reportId);

        return result
            ? Ok(new { message = AnalyticsConstants.KPIReportDeletedSuccess })
            : NotFound(new { message = AnalyticsConstants.KPIReportNotFound });
    }

    [HttpGet("dashboards/travel-spend")]
    [Authorize(Roles = "Admin,FinanceOfficer,CorporateTravelManager")]
    public async Task<IActionResult> GetTravelSpendDashboard([FromQuery] DashboardFilterDto filter)
    {
        var data = await _analyticsService.GetTravelSpendDashboardAsync(filter.Filter ?? "month");
        return data != null ? Ok(new { message = AnalyticsConstants.DashboardDataRetrievedSuccess, data }) : NotFound(new { message = GeneralConstants.InvalidInput });
    }

    [HttpGet("dashboards/booking-volume")]
    [Authorize(Roles = "Admin,TravelAgent,CorporateTravelManager")]
    public async Task<IActionResult> GetBookingVolumeDashboard([FromQuery] DashboardFilterDto filter)
    {
        var data = await _analyticsService.GetBookingVolumeDashboardAsync(filter.Filter ?? "month");
        return data != null ? Ok(new { message = AnalyticsConstants.DashboardDataRetrievedSuccess, data }) : NotFound(new { message = GeneralConstants.InvalidInput });
    }

    [HttpGet("dashboards/cancellations")]
    [Authorize(Roles = "Admin,TravelAgent,CorporateTravelManager,ComplianceOfficer")]
    public async Task<IActionResult> GetCancellationDashboard([FromQuery] DashboardFilterDto filter)
    {
        var data = await _analyticsService.GetCancellationDashboardAsync(filter.Filter ?? "month");
        return data != null ? Ok(new { message = AnalyticsConstants.DashboardDataRetrievedSuccess, data }) : NotFound(new { message = GeneralConstants.InvalidInput });
    }

    [HttpGet("dashboards/avg-booking-value")]
    [Authorize(Roles = "Admin,FinanceOfficer,CorporateTravelManager")]
    public async Task<IActionResult> GetAvgBookingValueDashboard([FromQuery] DashboardFilterDto filter)
    {
        var data = await _analyticsService.GetAvgBookingValueDashboardAsync(filter.Filter ?? "month");
        return data != null ? Ok(new { message = AnalyticsConstants.DashboardDataRetrievedSuccess, data }) : NotFound(new { message = GeneralConstants.InvalidInput });
    }

    [HttpGet("dashboards/top-spenders")]
    [Authorize(Roles = "Admin,FinanceOfficer,CorporateTravelManager")]
    public async Task<IActionResult> GetTopSpendersDashboard([FromQuery] DashboardFilterDto filter)
    {
        var data = await _analyticsService.GetTopSpendersDashboardAsync(filter.Filter ?? "month");
        return data != null ? Ok(new { message = AnalyticsConstants.DashboardDataRetrievedSuccess, data }) : NotFound(new { message = GeneralConstants.InvalidInput });
    }

    [HttpGet("dashboards/revenue-by-type")]
    [Authorize(Roles = "Admin,FinanceOfficer,CorporateTravelManager")]
    public async Task<IActionResult> GetRevenueByTypeDashboard([FromQuery] DashboardFilterDto filter)
    {
        var data = await _analyticsService.GetRevenueByTypeDashboardAsync(filter.Filter ?? "month");
        return data != null ? Ok(new { message = AnalyticsConstants.DashboardDataRetrievedSuccess, data }) : NotFound(new { message = GeneralConstants.InvalidInput });
    }

    [HttpGet("trends/spend-per-traveler")]
    [Authorize(Roles = "Admin,FinanceOfficer,CorporateTravelManager")]
    public async Task<IActionResult> GetSpendPerTravelerTrend()
    {
        var data = await _analyticsService.GetSpendPerTravelerTrendAsync();

        return data != null
            ? Ok(new { message = AnalyticsConstants.TrendAnalysisSuccess, data })
            : NotFound(new { message = GeneralConstants.InvalidInput });
    }

    [HttpGet("trends/destinations")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDestinationTrend()
    {
        var data = await _analyticsService.GetDestinationTrendAsync();

        return data != null
            ? Ok(new { message = AnalyticsConstants.TrendAnalysisSuccess, data })
            : NotFound(new { message = GeneralConstants.InvalidInput });
    }
}