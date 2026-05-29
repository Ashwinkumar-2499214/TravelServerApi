using System.ComponentModel.DataAnnotations;

namespace TravelEaseServer.Model
{
    public class AuditLog
    {
        [Key]
        public long AuditLogId { get; set; }
        public long UserId { get; set; }
        public required string Action { get; set; }
        public required string EntityType { get; set; }
        public long EntityId { get; set; }
        public required string OldValues { get; set; }
        public required string NewValues { get; set; }
        public DateTime Timestamp { get; set; }
        public required string IpAddress { get; set; }

        public virtual required User User { get; set; }
    }
}
