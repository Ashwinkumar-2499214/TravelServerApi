using Microsoft.AspNetCore.Authorization;
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
            _complianceService = complianceService ?? throw new ArgumentNullException(nameof(complianceService));
        }

        [HttpGet("reports")]
        [Authorize(Roles = "ComplianceOfficer,Admin")]
        public async Task<IActionResult> GetAllReports([FromQuery] ComplianceReportSearchDto searchDto)
        {
            if (!ModelState.IsValid || searchDto == null)
            {
                return BadRequest(new { message = GeneralConstants.InvalidInput });
            }

            var reports = await _complianceService.GetAllReportsAsync(searchDto);
            return Ok(new { message = GeneralConstants.OperationSuccess, data = reports });
        }

        [HttpPost("reports")]
        [Authorize(Roles = "ComplianceOfficer,Admin")]
        public async Task<IActionResult> CreateReport([FromBody] ComplianceReportRequestDto reportDto)
        {
            if (!ModelState.IsValid || reportDto == null)
            {
                return BadRequest(new { message = GeneralConstants.InvalidInput });
            }

            var createdReport = await _complianceService.CreateReportAsync(reportDto);
            return Ok(new { message = ComplianceConstants.ReportGeneratedSuccess, data = createdReport });
        }

        [HttpGet("reports/{reportId}")]
        [Authorize(Roles = "ComplianceOfficer,Admin")]
        public async Task<IActionResult> GetReportById(long reportId)
        {
            if (reportId <= 0)
            {
                return BadRequest(new { message = GeneralConstants.InvalidInput });
            }

            var report = await _complianceService.GetReportByIdAsync(reportId);
            
            return report != null 
                ? Ok(new { message = GeneralConstants.OperationSuccess, data = report }) 
                : NotFound(new { message = ComplianceConstants.ReportNotFound });
        }

        [HttpDelete("reports/{reportId}")]
        [Authorize(Roles = "ComplianceOfficer,Admin")]
        public async Task<IActionResult> DeleteReport(long reportId)
        {
            if (reportId <= 0)
            {
                return BadRequest(new { message = GeneralConstants.InvalidInput });
            }

            var isDeleted = await _complianceService.DeleteReportAsync(reportId);
            
            return isDeleted 
                ? Ok(new { message = ComplianceConstants.ReportDeletedSuccess }) 
                : NotFound(new { message = ComplianceConstants.ReportNotFound });
        }

        [HttpGet("audit-logs")]
        [Authorize(Roles = "ComplianceOfficer,Admin")]
        public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogSearchDto searchDto)
        {
            if (!ModelState.IsValid || searchDto == null)
            {
                return BadRequest(new { message = GeneralConstants.InvalidInput });
            }

            var logs = await _complianceService.GetAuditLogsAsync(searchDto);
            return Ok(new { message = GeneralConstants.OperationSuccess, data = logs });
        }

        [HttpGet("policies")]
        [Authorize(Roles = "ComplianceOfficer,Admin,FinanceOfficer")] 
        public async Task<IActionResult> GetAllPolicies()
        {
            var policies = await _complianceService.GetAllRetentionPoliciesAsync();
            return Ok(new { message = GeneralConstants.OperationSuccess, data = policies });
        }

        [HttpPut("policies/{policyId}")]
        [Authorize(Roles = "ComplianceOfficer,Admin")]
        public async Task<IActionResult> UpdatePolicy(long policyId, [FromBody] RetentionPolicyDto policyDto)
        {
            if (!ModelState.IsValid || policyDto == null || policyId <= 0)
            {
                return BadRequest(new { message = GeneralConstants.InvalidInput });
            }

            var updatedPolicy = await _complianceService.UpdateRetentionPolicyAsync(policyId, policyDto);
            
            return updatedPolicy != null 
                ? Ok(new { message = ComplianceConstants.PolicyUpdateSuccess, data = updatedPolicy }) 
                : NotFound(new { message = GeneralConstants.InvalidInput });
        }
    }
}