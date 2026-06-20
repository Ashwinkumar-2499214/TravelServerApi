using TravelEaseServer.Dto;
using TravelEaseServer.Model;

namespace TravelEaseServer.Repository.Interface
{
    public interface IComplianceRepository
    {
        Task<ComplianceReportResponseDto> CreateReportAsync(ComplianceReport report);
        Task<ComplianceReportResponseDto> GetReportByIdAsync(long reportId);
        Task<IEnumerable<ComplianceReportResponseDto>> GetAllReportsAsync(ComplianceReportSearchDto searchDto);
        Task<bool> DeleteReportAsync(long reportId);
        Task<IEnumerable<AuditLogResponseDto>> GetAuditLogsAsync(AuditLogSearchDto searchDto);
        Task<RetentionPolicyDto> GetRetentionPolicyAsync(long policyId);
        Task<IEnumerable<RetentionPolicyDto>> GetAllRetentionPoliciesAsync();
        Task<int> GetAuditLogsCountAsync(DateTime startDate, DateTime endDate);
        Task<RetentionPolicyDto> UpdateRetentionPolicyAsync(RetentionPolicy policy);
        Task<bool> LogAuditEventAsync(AuditLog auditLog);
        Task<int> GetTotalInvoicesCountAsync(DateTime fromDate, DateTime toDate);
        Task<int> GetFinancialDiscrepanciesCountAsync(DateTime fromDate, DateTime toDate);
    }
}
