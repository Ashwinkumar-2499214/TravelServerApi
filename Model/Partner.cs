using System.ComponentModel.DataAnnotations;

namespace TravelEaseServer.Model
{
    public class Partner
    {
        [Key]
        public long PartnerId { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string Address { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
