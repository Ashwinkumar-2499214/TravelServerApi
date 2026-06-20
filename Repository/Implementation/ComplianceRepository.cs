using Microsoft.EntityFrameworkCore;
using TravelEaseServer.Dto;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;

namespace TravelEaseServer.Repository.Implementation
{
    public class ComplianceRepository : IComplianceRepository
    {
        private readonly AppDbContext _context;

        public ComplianceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ComplianceReportResponseDto> CreateReportAsync(ComplianceReport report)
        {
            _context.ComplianceReports.Add(report);
            await _context.SaveChangesAsync();
            return MapComplianceReportToDto(report);
        }

        public async Task<ComplianceReportResponseDto?> GetReportByIdAsync(long reportId)
        {
            var report = await _context.ComplianceReports
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ComplianceReportId == reportId);

            return report != null ? MapComplianceReportToDto(report) : null;
        }

        public async Task<IEnumerable<ComplianceReportResponseDto>> GetAllReportsAsync(ComplianceReportSearchDto searchDto)
        {
            var query = _context.ComplianceReports.AsNoTracking();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
            {
                var term = searchDto.SearchTerm.ToLower();
                query = query.Where(r => r.Title.ToLower().Contains(term) ||
                                         r.Scope.ToLower().Contains(term));
            }

            if (searchDto.FromDate.HasValue)
            {
                query = query.Where(r => r.GeneratedDate >= searchDto.FromDate.Value);
            }

            if (searchDto.ToDate.HasValue)
            {
                query = query.Where(r => r.GeneratedDate <= searchDto.ToDate.Value);
            }

            // Apply pagination
            int skip = (searchDto.PageNumber - 1) * searchDto.PageSize;
            var reports = await query
                .OrderByDescending(r => r.GeneratedDate)
                .Skip(skip)
                .Take(searchDto.PageSize)
                .ToListAsync();

            return reports.Select(MapComplianceReportToDto);
        }
        public async Task<int> GetTotalInvoicesCountAsync(DateTime fromDate, DateTime toDate)
        {
           
            return await _context.Invoices
                .AsNoTracking()
                .Where(i => i.InvoiceDate >= fromDate && i.InvoiceDate <= toDate)
                .CountAsync();
        }

        public async Task<int> GetFinancialDiscrepanciesCountAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.Invoices
                .AsNoTracking()
                .Where(i => i.InvoiceDate >= fromDate && i.InvoiceDate <= toDate)
                .CountAsync(i => _context.Payments

                    .Where(p => p.InvoiceId == i.InvoiceId && p.Status == 1)
                    .Sum(p => p.Amount) != i.Amount);
        }

        public async Task<bool> DeleteReportAsync(long reportId)
        {
            var report = await _context.ComplianceReports.FirstOrDefaultAsync(r => r.ComplianceReportId == reportId);
            if (report == null)
            {
                return false;
            }

            _context.ComplianceReports.Remove(report);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<AuditLogResponseDto>> GetAuditLogsAsync(AuditLogSearchDto searchDto)
        {
            var query = _context.AuditLogs.AsNoTracking();

            // Apply filters
            if (searchDto.UserId.HasValue)
            {
                query = query.Where(a => a.UserId == searchDto.UserId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchDto.EntityType))
            {
                // Production Optimization: EF core natively translates StringComparison or normal comparison efficiently.
                var type = searchDto.EntityType.ToLower();
                query = query.Where(a => a.EntityType.ToLower() == type);
            }

            if (!string.IsNullOrWhiteSpace(searchDto.Action))
            {
                var action = searchDto.Action.ToLower();
                query = query.Where(a => a.Action.ToLower().Contains(action));
            }

            if (searchDto.FromDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= searchDto.FromDate.Value);
            }

            if (searchDto.ToDate.HasValue)
            {
                query = query.Where(a => a.Timestamp <= searchDto.ToDate.Value);
            }

            // Apply pagination
            int skip = (searchDto.PageNumber - 1) * searchDto.PageSize;
            var auditLogs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip(skip)
                .Take(searchDto.PageSize)
                .ToListAsync();

            return auditLogs.Select(MapAuditLogToDto);
        }

        public async Task<RetentionPolicyDto?> GetRetentionPolicyAsync(long policyId)
        {
            var policy = await _context.RetentionPolicies
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.RetentionPolicyId == policyId);

            return policy != null ? MapRetentionPolicyToDto(policy) : null;
        }

        public async Task<IEnumerable<RetentionPolicyDto>> GetAllRetentionPoliciesAsync()
        {
            var policies = await _context.RetentionPolicies
                .AsNoTracking()
                .ToListAsync();

            return policies.Select(MapRetentionPolicyToDto);
        }

        public async Task<RetentionPolicyDto> UpdateRetentionPolicyAsync(RetentionPolicy policy)
        {
            var existingPolicy = await _context.RetentionPolicies
                .FirstOrDefaultAsync(p => p.RetentionPolicyId == policy.RetentionPolicyId);

            if (existingPolicy == null)
            {
                throw new KeyNotFoundException($"Retention policy with ID {policy.RetentionPolicyId} not found.");
            }

            existingPolicy.DataType = policy.DataType;
            existingPolicy.RetentionDays = policy.RetentionDays;
            existingPolicy.Description = policy.Description;
            existingPolicy.IsActive = policy.IsActive;

            // Explicit context.Update() is redundant because EF Core tracks alterations to 'existingPolicy'
            await _context.SaveChangesAsync();

            return MapRetentionPolicyToDto(existingPolicy);
        }

        public async Task<bool> LogAuditEventAsync(AuditLog auditLog)
        {
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetAuditLogsCountAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
                .CountAsync();
        }

        private static ComplianceReportResponseDto MapComplianceReportToDto(ComplianceReport report)

        {
            return new ComplianceReportResponseDto
            {
                ComplianceReportId = report.ComplianceReportId,
                Title = report.Title,
                Scope = report.Scope,
                Metrics = report.Metrics,
                GeneratedDate = report.GeneratedDate,
                ReportContent = report.ReportContent
            };
        }

        private static AuditLogResponseDto MapAuditLogToDto(AuditLog auditLog)
        {
            return new AuditLogResponseDto
            {
                AuditLogId = auditLog.AuditLogId,
                UserId = auditLog.UserId,
                Action = auditLog.Action,
                EntityType = auditLog.EntityType,
                EntityId = auditLog.EntityId,
                OldValues = auditLog.OldValues,
                NewValues = auditLog.NewValues,
                Timestamp = auditLog.Timestamp,
                IpAddress = auditLog.IpAddress
            };
        }

        private static RetentionPolicyDto MapRetentionPolicyToDto(RetentionPolicy policy)
        {
            return new RetentionPolicyDto
            {
                RetentionPolicyId = policy.RetentionPolicyId,
                DataType = policy.DataType,
                RetentionDays = policy.RetentionDays,
                Description = policy.Description,
                IsActive = policy.IsActive
            };
        }
    }
}