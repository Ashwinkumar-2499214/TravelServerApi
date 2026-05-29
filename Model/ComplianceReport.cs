using System.ComponentModel.DataAnnotations;

namespace TravelEaseServer.Model
{
    public class ComplianceReport
    {
        [Key]
        public long ComplianceReportId { get; set; }
        public string Title { get; set; }
        public string Scope { get; set; }
        public string Metrics { get; set; }
        public DateTime GeneratedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ReportContent { get; set; }
    }
}
