namespace TravelEaseServer.Dto
{
    public class PaymentRequestDto
    {
        public long UserId { get; set; }
        public long InvoiceId { get; set; }
        public long BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public int Method { get; set; }
        public string TransactionReference { get; set; }
        public string GatewayProvider { get; set; }
        public string BillingName { get; set; }
        public string BillingEmail { get; set; }
        public string BillingPhone { get; set; }
        public string Notes { get; set; }
    }

    public class PaymentResponseDto
    {
        public long PaymentId { get; set; }
        public long UserId { get; set; }
        public long InvoiceId { get; set; }
        public long BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
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
    }

    public class PaymentSearchDto
    {
        public long? InvoiceId { get; set; }
        public long? BookingId { get; set; }
        public int? Status { get; set; }
        public int? Method { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PaymentStatusUpdateDto
    {
        public long PaymentId { get; set; }
        public int NewStatus { get; set; }
    }

    public class PaymentRefundDto
    {
        public long PaymentId { get; set; }
        public decimal RefundAmount { get; set; }
        public string Reason { get; set; }
    }
}
