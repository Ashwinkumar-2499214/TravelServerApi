using System.ComponentModel.DataAnnotations;

namespace TravelEaseServer.Model
{
    public class Payment
    {
        [Key]
        public long PaymentId { get; set; }
        public long InvoiceId { get; set; }
        public long BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public DateTime PaymentDate { get; set; }
        public int Method { get; set; }
        public int Status { get; set; }
        public string TransactionReference { get; set; }
        public string GatewayProvider { get; set; }
        public string BillingName { get; set; }
        public string BillingEmail { get; set; }
        public string BillingPhone { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual Invoice Invoice { get; set; }
    }
}
