using System.ComponentModel.DataAnnotations;

namespace TravelEaseServer.Model
{
    public class Invoice
    {
        [Key]
        public long InvoiceId { get; set; }
        public long BookingId { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Amount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual Booking Booking { get; set; }
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
