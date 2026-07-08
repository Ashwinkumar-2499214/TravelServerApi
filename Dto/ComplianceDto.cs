namespace TravelEaseServer.Dto
{
    public class ComplianceReportRequestDto
    {
        public string Title { get; set; }
    }

    public class ComplianceReportResponseDto
    {
        public long ComplianceReportId { get; set; }
        public string Title { get; set; }
        public string Scope { get; set; }
        public string Metrics { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string ReportContent { get; set; }
    }

    public class ComplianceReportSearchDto
    {
        public string? SearchTerm { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

   public class AuditLogResponseDto
    {
        public long AuditLogId { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public long EntityId { get; set; }
        public object? OldValues { get; set; }
        public object? NewValues { get; set; }
        public DateTime Timestamp { get; set; }
        public string? IpAddress { get; set; }
    }

    public class AuditLogSearchDto
    {
        public long? UserId { get; set; }
        public string? EntityType { get; set; }
        public string? Action { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class RetentionPolicyDto
    {
        public long RetentionPolicyId { get; set; }
        public string DataType { get; set; }
        public int RetentionDays { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

    namespace TravelEaseServer.Dto
    {
        public class AutoReportRequestDto
        {
            public string Title { get; set; } = "Automated Compliance Audit";
            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }
        }
    }
}
