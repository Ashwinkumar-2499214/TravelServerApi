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
            try
            {
                _context.ComplianceReports.Add(report);
                await _context.SaveChangesAsync();
                return MapComplianceReportToDto(report);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Error creating compliance report in database.", ex);
            }
        }

        public async Task<ComplianceReportResponseDto> GetReportByIdAsync(long reportId)
        {
            try
            {
                var report = await _context.ComplianceReports
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.ComplianceReportId == reportId);

                return report != null ? MapComplianceReportToDto(report) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving compliance report with ID {reportId}.", ex);
            }
        }

        public async Task<IEnumerable<ComplianceReportResponseDto>> GetAllReportsAsync(ComplianceReportSearchDto searchDto)
        {
            try
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
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving compliance reports.", ex);
            }
        }

        public async Task<bool> DeleteReportAsync(long reportId)
        {
            try
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
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error deleting compliance report with ID {reportId}.", ex);
            }
        }

        public async Task<IEnumerable<AuditLogResponseDto>> GetAuditLogsAsync(AuditLogSearchDto searchDto)
        {
            try
            {
                var query = _context.AuditLogs.AsNoTracking();

                // Apply filters
                if (searchDto.UserId.HasValue)
                {
                    query = query.Where(a => a.UserId == searchDto.UserId.Value);
                }

                if (!string.IsNullOrWhiteSpace(searchDto.EntityType))
                {
                    query = query.Where(a => a.EntityType.ToLower() == searchDto.EntityType.ToLower());
                }

                if (!string.IsNullOrWhiteSpace(searchDto.Action))
                {
                    query = query.Where(a => a.Action.ToLower().Contains(searchDto.Action.ToLower()));
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
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving audit logs.", ex);
            }
        }

        public async Task<RetentionPolicyDto> GetRetentionPolicyAsync(long policyId)
        {
            try
            {
                var policy = await _context.RetentionPolicies
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.RetentionPolicyId == policyId);

                return policy != null ? MapRetentionPolicyToDto(policy) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving retention policy with ID {policyId}.", ex);
            }
        }

        public async Task<IEnumerable<RetentionPolicyDto>> GetAllRetentionPoliciesAsync()
        {
            try
            {
                var policies = await _context.RetentionPolicies
                    .AsNoTracking()
                    .ToListAsync();

                return policies.Select(MapRetentionPolicyToDto);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving retention policies.", ex);
            }
        }

        public async Task<RetentionPolicyDto> UpdateRetentionPolicyAsync(RetentionPolicy policy)
        {
            try
            {
                var existingPolicy = await _context.RetentionPolicies.FirstOrDefaultAsync(p => p.RetentionPolicyId == policy.RetentionPolicyId);
                if (existingPolicy == null)
                {
                    throw new KeyNotFoundException($"Retention policy with ID {policy.RetentionPolicyId} not found.");
                }

                existingPolicy.DataType = policy.DataType;
                existingPolicy.RetentionDays = policy.RetentionDays;
                existingPolicy.Description = policy.Description;
                existingPolicy.IsActive = policy.IsActive;

                _context.RetentionPolicies.Update(existingPolicy);
                await _context.SaveChangesAsync();

                return MapRetentionPolicyToDto(existingPolicy);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error updating retention policy with ID {policy.RetentionPolicyId}.", ex);
            }
        }

        public async Task<bool> LogAuditEventAsync(AuditLog auditLog)
        {
            try
            {
                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Error logging audit event.", ex);
            }
        }

        private ComplianceReportResponseDto MapComplianceReportToDto(ComplianceReport report)
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

        private AuditLogResponseDto MapAuditLogToDto(AuditLog auditLog)
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

        private RetentionPolicyDto MapRetentionPolicyToDto(RetentionPolicy policy)
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
