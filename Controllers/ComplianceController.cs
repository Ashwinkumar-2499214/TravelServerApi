using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers
{
    [ApiController]
    [Route("api/v1/compliance")]
    public class ComplianceController : ControllerBase
    {
        private readonly IComplianceService _complianceService;

        public ComplianceController(IComplianceService complianceService)
        {
            _complianceService = complianceService;
        }

        [HttpGet("reports")]
        public async Task<IActionResult> GetAllReports([FromQuery] ComplianceReportSearchDto searchDto)
        {
            return (ModelState.IsValid && searchDto != null)
                ? Ok(new { message = GeneralConstants.OperationSuccess, data = await _complianceService.GetAllReportsAsync(searchDto) })
                : BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        [HttpPost("reports")]
        public async Task<IActionResult> CreateReport([FromBody] ComplianceReportRequestDto reportDto)
        {
            return (ModelState.IsValid && reportDto != null)
                ? Ok(new { message = ComplianceConstants.ReportGeneratedSuccess, data = await _complianceService.CreateReportAsync(reportDto) })
                : BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        [HttpGet("reports/{reportId}")]
        public async Task<IActionResult> GetReportById(long reportId)
        {
            return Ok(new { message = GeneralConstants.OperationSuccess, data = await _complianceService.GetReportByIdAsync(reportId) });
        }

        [HttpDelete("reports/{reportId}")]
        public async Task<IActionResult> DeleteReport(long reportId)
        {
            return await _complianceService.DeleteReportAsync(reportId)
                ? Ok(new { message = ComplianceConstants.ReportDeletedSuccess })
                : BadRequest(new { message = ComplianceConstants.ReportNotFound });
        }

        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogSearchDto searchDto)
        {
            return (ModelState.IsValid && searchDto != null)
                ? Ok(new { message = GeneralConstants.OperationSuccess, data = await _complianceService.GetAuditLogsAsync(searchDto) })
                : BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        [HttpGet("policies")]
        public async Task<IActionResult> GetAllPolicies()
        {
            return Ok(new { message = GeneralConstants.OperationSuccess, data = await _complianceService.GetAllRetentionPoliciesAsync() });
        }

        [HttpPut("policies/{policyId}")]
        public async Task<IActionResult> UpdatePolicy(long policyId, [FromBody] RetentionPolicyDto policyDto)
        {
            return (ModelState.IsValid && policyDto != null)
                ? Ok(new { message = ComplianceConstants.PolicyUpdateSuccess, data = await _complianceService.UpdateRetentionPolicyAsync(policyId, policyDto) })
                : BadRequest(new { message = GeneralConstants.InvalidInput });
        }
    }
}