namespace TravelEaseServer.Dto
{
    public class ReservationRequestDto
    {
        public long BookingId { get; set; }
        public required string Details { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ReservationResponseDto
    {
        public long ReservationId { get; set; }
        public long BookingId { get; set; }
        public required string Details { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ReservationSearchDto
    {
        public long? BookingId { get; set; }
        public int? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ReservationStatusUpdateDto
    {
        public long ReservationId { get; set; }
        public int NewStatus { get; set; }
    }
}
