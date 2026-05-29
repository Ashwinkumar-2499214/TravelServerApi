using System.ComponentModel.DataAnnotations;

namespace TravelEaseServer.Model
{
    public class Notification
    {
        [Key]
        public long NotificationId { get; set; }
        public long UserId { get; set; }
        public string Message { get; set; }
        public int Category { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ReadDate { get; set; }

        public virtual User User { get; set; }
    }
}
