using System.ComponentModel.DataAnnotations;

namespace TravelEaseServer.Dto
{
    public class KPIReportRequestDto
    {
        [Required]
        public string Title { get; set; }
        public string? Scope { get; set; }
        public string? Metrics { get; set; }
        public string? ReportContent { get; set; }
    }

    public class KPIReportResponseDto
    {
        public long KPIReportId { get; set; }
        public string Title { get; set; }
        public string Scope { get; set; }
        public string Metrics { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string ReportContent { get; set; }
    }

    public class KPIReportSearchDto
    {
        public string? SearchTerm { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class DashboardDataDto
    {
        public string Title { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalCount { get; set; }
        public string Period { get; set; }
        public object Data { get; set; }
    }

    public class DashboardFilterDto
    {
        public string? Filter { get; set; } = "month";
    }

    public class TrendAnalysisDto
    {
        public string Title { get; set; }
        public string TrendType { get; set; }
        public DateTime Period { get; set; }
        public object TrendData { get; set; }
    }
}
