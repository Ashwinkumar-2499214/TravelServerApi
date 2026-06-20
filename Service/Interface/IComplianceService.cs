using TravelEaseServer.Dto;
using TravelEaseServer.Dto.TravelEaseServer.Dto;

namespace TravelEaseServer.Service.Interface
{
    public interface IComplianceService
    {
        Task<ComplianceReportResponseDto> CreateReportAsync(ComplianceReportRequestDto reportDto);
        Task<ComplianceReportResponseDto?> GetReportByIdAsync(long reportId);
        Task<IEnumerable<ComplianceReportResponseDto>> GetAllReportsAsync(ComplianceReportSearchDto searchDto);
        Task<bool> DeleteReportAsync(long reportId);
        Task<IEnumerable<AuditLogResponseDto>> GetAuditLogsAsync(AuditLogSearchDto searchDto);
        Task<RetentionPolicyDto?> GetRetentionPolicyAsync(long policyId);
        Task<IEnumerable<RetentionPolicyDto>> GetAllRetentionPoliciesAsync();
        Task<RetentionPolicyDto> UpdateRetentionPolicyAsync(long policyId, RetentionPolicyDto policyDto);
        }
}
