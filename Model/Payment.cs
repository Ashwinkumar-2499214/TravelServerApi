using System.ComponentModel.DataAnnotations;

namespace TravelEaseServer.Model
{
    public class Payment
    {
        [Key]
        public long PaymentId { get; set; }
        public long InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public int Method { get; set; }
        public int Status { get; set; }
        public string TransactionReference { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual Invoice Invoice { get; set; }
    }
}
