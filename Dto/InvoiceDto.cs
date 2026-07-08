namespace TravelEaseServer.Dto
{
    public class InvoiceRequestDto
    {
        public long BookingId { get; set; }
        public decimal Amount { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime DueDate { get; set; }
        public string Description { get; set; }
    }

    public class InvoiceResponseDto
    {
        public long InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public long BookingId { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string HotelName { get; set; }
        public string RoomType { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfNights { get; set; }
        public string InventoryName { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Amount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class InvoiceSearchDto
    {
        public long? BookingId { get; set; }
        public long? UserId { get; set; }
        public int? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class InvoiceStatusUpdateDto
    {
        public long InvoiceId { get; set; }
        public int NewStatus { get; set; }
    }

    public class InvoiceAdjustmentDto
    {
        public long InvoiceId { get; set; }
        public decimal AdjustmentAmount { get; set; }
        public string Reason { get; set; }
    }
}
