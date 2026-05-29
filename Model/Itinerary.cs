using System.ComponentModel.DataAnnotations;

namespace TravelEaseServer.Model
{
    public class Itinerary
    {
        [Key]
        public long ItineraryId { get; set; }
        public long UserId { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<ItineraryBooking> ItineraryBookings { get; set; } = new List<ItineraryBooking>();
    }
}
