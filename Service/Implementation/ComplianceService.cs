using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Service.Implementation
{
    public class ComplianceService : IComplianceService
    {
        private readonly IComplianceRepository _complianceRepository;

        public ComplianceService(IComplianceRepository complianceRepository)
        {
            _complianceRepository = complianceRepository;
        }

        public async Task<ComplianceReportResponseDto> CreateReportAsync(ComplianceReportRequestDto reportDto)
        {
            try
            {
                var report = new ComplianceReport
                {
                    Title = reportDto.Title,
                    Scope = reportDto.Scope,
                    Metrics = reportDto.Metrics,
                    GeneratedDate = DateTime.UtcNow,
                    ReportContent = reportDto.ReportContent
                };

                return await _complianceRepository.CreateReportAsync(report);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ComplianceConstants.ReportGeneratedSuccess, ex);
            }
        }

        public async Task<ComplianceReportResponseDto> GetReportByIdAsync(long reportId)
        {
            return await _complianceRepository.GetReportByIdAsync(reportId);
        }

        public async Task<IEnumerable<ComplianceReportResponseDto>> GetAllReportsAsync(ComplianceReportSearchDto searchDto)
        {
            return await _complianceRepository.GetAllReportsAsync(searchDto);
        }

        public async Task<bool> DeleteReportAsync(long reportId)
        {
            return await _complianceRepository.DeleteReportAsync(reportId);
        }

        public async Task<IEnumerable<AuditLogResponseDto>> GetAuditLogsAsync(AuditLogSearchDto searchDto)
        {
            return await _complianceRepository.GetAuditLogsAsync(searchDto);
        }

        public async Task<RetentionPolicyDto> GetRetentionPolicyAsync(long policyId)
        {
            return await _complianceRepository.GetRetentionPolicyAsync(policyId);
        }

        public async Task<IEnumerable<RetentionPolicyDto>> GetAllRetentionPoliciesAsync()
        {
            return await _complianceRepository.GetAllRetentionPoliciesAsync();
        }

        public async Task<RetentionPolicyDto> UpdateRetentionPolicyAsync(long policyId, RetentionPolicyDto policyDto)
        {
            try
            {
                var policy = new RetentionPolicy
                {
                    RetentionPolicyId = policyId,
                    DataType = policyDto.DataType,
                    RetentionDays = policyDto.RetentionDays,
                    Description = policyDto.Description,
                    IsActive = policyDto.IsActive,
                    ModifiedDate = DateTime.UtcNow
                };

                return await _complianceRepository.UpdateRetentionPolicyAsync(policy);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ComplianceConstants.PolicyUpdateSuccess, ex);
            }
        }
    }
}
