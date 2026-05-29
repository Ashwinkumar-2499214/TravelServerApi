namespace TravelEaseServer.Dto
{
    public class ItineraryRequestDto
    {
        public long UserId { get; set; }
        public required string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ItineraryResponseDto
    {
        public long ItineraryId { get; set; }
        public long UserId { get; set; }
        public required string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public ICollection<BookingResponseDto>? Bookings { get; set; }
    }

    public class ItinerarySearchDto
    {
        public long? UserId { get; set; }
        public int? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ItineraryStatusUpdateDto
    {
        public long ItineraryId { get; set; }
        public int NewStatus { get; set; }
    }

    public class ItineraryBookingAddDto
    {
        public long ItineraryId { get; set; }
        public long BookingId { get; set; }
    }

    public class ItineraryExportDto
    {
        public long ItineraryId { get; set; }
        public required string Format { get; set; }
    }

    public class ItineraryShareDto
    {
        public long ItineraryId { get; set; }
        public required string RecipientEmail { get; set; }
        public required string Message { get; set; }
    }
}
