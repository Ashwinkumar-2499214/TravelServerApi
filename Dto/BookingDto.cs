namespace TravelEaseServer.Dto
{
    public class BookingRequestDto
    {
        public long UserId { get; set; }
        public long PartnerId { get; set; }
        public long InventoryId { get; set; }
        public string ItemType { get; set; }
        public decimal Amount { get; set; }
    }

    public class BookingResponseDto
    {
        public long BookingId { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public long PartnerId { get; set; }
        public long InventoryId { get; set; }
        public string ItemType { get; set; }
        public DateTime BookingDate { get; set; }
        public int Status { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class BookingSearchDto
    {
        public long? UserId { get; set; }
        public long? PartnerId { get; set; }
        public int? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class BookingStatusUpdateDto
    {
        public long BookingId { get; set; }
        public int NewStatus { get; set; }
    }
}
