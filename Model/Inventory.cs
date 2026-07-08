using System.ComponentModel.DataAnnotations;

namespace TravelEaseServer.Model
{
    public class Inventory
    {
        [Key]
        public long InventoryId { get; set; }
        public long PartnerId { get; set; }
        public string ItemType { get; set; }
        public string Description { get; set; }
        public int Availability { get; set; }
        public decimal Price { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual Partner Partner { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<InventoryMedia> Media { get; set; } = new List<InventoryMedia>();
    }
}
