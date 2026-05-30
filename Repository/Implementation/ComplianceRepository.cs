using Microsoft.EntityFrameworkCore;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;

namespace TravelEaseServer.Repository.Implementation;

public class ComplianceRepository : IComplianceRepository
{
    private readonly AppDbContext _context;

    public ComplianceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ComplianceReport> CreateReportAsync(ComplianceReport report)
    {
        _context.ComplianceReports.Add(report);
        await _context.SaveChangesAsync();
        return report;
    }

    public async Task<ComplianceReport?> GetReportByIdAsync(long reportId)
    {
        return await _context.ComplianceReports
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ComplianceReportId == reportId);
    }

    public async Task<IEnumerable<ComplianceReport>> GetAllReportsAsync(string? searchTerm, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize)
    {
        var query = _context.ComplianceReports.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(r => r.Title.ToLower().Contains(term) || r.Scope.ToLower().Contains(term));
        }

        if (fromDate.HasValue)
        {
            query = query.Where(r => r.GeneratedDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(r => r.GeneratedDate <= toDate.Value);
        }

        int skip = (pageNumber - 1) * pageSize;

        return await query
            .OrderByDescending(r => r.GeneratedDate)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
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

    public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(long? userId, string? entityType, string? action, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize)
    {
        var query = _context.AuditLogs.AsNoTracking();

        if (userId.HasValue)
        {
            query = query.Where(a => a.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            var typeLower = entityType.ToLower();
            query = query.Where(a => a.EntityType.ToLower() == typeLower);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            var actionLower = action.ToLower();
            query = query.Where(a => a.Action.ToLower().Contains(actionLower));
        }

        if (fromDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= toDate.Value);
        }

        int skip = (pageNumber - 1) * pageSize;

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<RetentionPolicy?> GetRetentionPolicyAsync(long policyId)
    {
        return await _context.RetentionPolicies
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.RetentionPolicyId == policyId);
    }

    public async Task<IEnumerable<RetentionPolicy>> GetAllRetentionPoliciesAsync()
    {
        return await _context.RetentionPolicies
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<RetentionPolicy?> UpdateRetentionPolicyAsync(RetentionPolicy policy)
    {
        var existingPolicy = await _context.RetentionPolicies.FirstOrDefaultAsync(p => p.RetentionPolicyId == policy.RetentionPolicyId);
        if (existingPolicy == null)
        {
            return null;
        }

        existingPolicy.DataType = policy.DataType;
        existingPolicy.RetentionDays = policy.RetentionDays;
        existingPolicy.Description = policy.Description;
        existingPolicy.IsActive = policy.IsActive;

        _context.RetentionPolicies.Update(existingPolicy);
        await _context.SaveChangesAsync();

        return existingPolicy;
    }

    public async Task<bool> LogAuditEventAsync(AuditLog auditLog)
    {
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
        return true;
    }
}