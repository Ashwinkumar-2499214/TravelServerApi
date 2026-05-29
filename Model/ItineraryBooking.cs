using System.ComponentModel.DataAnnotations;

namespace TravelEaseServer.Model
{
    public class ItineraryBooking
    {
        [Key]
        public long ItineraryBookingId { get; set; }
        public long ItineraryId { get; set; }
        public long BookingId { get; set; }
        public DateTime AddedDate { get; set; }

        public virtual Itinerary Itinerary { get; set; }
        public virtual Booking Booking { get; set; }
    }
}
