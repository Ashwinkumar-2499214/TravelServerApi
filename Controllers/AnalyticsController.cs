using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers;

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
        if (!ModelState.IsValid || searchDto == null)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _analyticsService.GetAllKPIReportsAsync(searchDto);

        return data != null
            ? Ok(new { message = GeneralConstants.OperationSuccess, data })
            : NotFound(new { message = AnalyticsConstants.KPIReportNotFound });
    }

    [HttpPost("kpi-reports")]
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
    public async Task<IActionResult> GetTravelSpendDashboard()
    {
        var data = await _analyticsService.GetTravelSpendDashboardAsync();

        return data != null
            ? Ok(new { message = AnalyticsConstants.DashboardDataRetrievedSuccess, data })
            : NotFound(new { message = GeneralConstants.InvalidInput });
    }

    [HttpGet("dashboards/booking-volume")]
    public async Task<IActionResult> GetBookingVolumeDashboard()
    {
        var data = await _analyticsService.GetBookingVolumeDashboardAsync();

        return data != null
            ? Ok(new { message = AnalyticsConstants.DashboardDataRetrievedSuccess, data })
            : NotFound(new { message = GeneralConstants.InvalidInput });
    }

    [HttpGet("dashboards/cancellations")]
    public async Task<IActionResult> GetCancellationDashboard()
    {
        var data = await _analyticsService.GetCancellationDashboardAsync();

        return data != null
            ? Ok(new { message = AnalyticsConstants.DashboardDataRetrievedSuccess, data })
            : NotFound(new { message = GeneralConstants.InvalidInput });
    }

    [HttpGet("trends/spend-per-traveler")]
    public async Task<IActionResult> GetSpendPerTravelerTrend()
    {
        var data = await _analyticsService.GetSpendPerTravelerTrendAsync();

        return data != null
            ? Ok(new { message = AnalyticsConstants.TrendAnalysisSuccess, data })
            : NotFound(new { message = GeneralConstants.InvalidInput });
    }

    [HttpGet("trends/destinations")]
    public async Task<IActionResult> GetDestinationTrend()
    {
        var data = await _analyticsService.GetDestinationTrendAsync();

        return data != null
            ? Ok(new { message = AnalyticsConstants.TrendAnalysisSuccess, data })
            : NotFound(new { message = GeneralConstants.InvalidInput });
    }
}