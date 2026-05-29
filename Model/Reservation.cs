using System.ComponentModel.DataAnnotations;

namespace TravelEaseServer.Model
{
    public class Reservation
    {
        [Key]
        public long ReservationId { get; set; }
        public long BookingId { get; set; }
        public string Details { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual Booking Booking { get; set; }
    }
}
