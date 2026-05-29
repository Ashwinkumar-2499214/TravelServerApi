using System.ComponentModel.DataAnnotations;

namespace TravelEaseServer.Model
{
    public class User
    {
        [Key]
        public long UserId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public required string PasswordHash { get; set; }
        public int Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
