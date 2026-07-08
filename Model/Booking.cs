using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelEaseServer.Model
{
    public class Booking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long BookingId { get; set; }
        public long UserId { get; set; }
        public long PartnerId { get; set; }
        public long InventoryId { get; set; }
        public string ItemType { get; set; }

        // Hotel booking fields
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public int NumberOfRooms { get; set; }
        public string RoomType { get; set; }
        public string SpecialRequests { get; set; }

        public DateTime BookingDate { get; set; }
        public int Status { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual User User { get; set; }
        public virtual Partner Partner { get; set; }
        public virtual Inventory Inventory { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public virtual ICollection<ItineraryBooking> ItineraryBookings { get; set; } = new List<ItineraryBooking>();
    }
}
