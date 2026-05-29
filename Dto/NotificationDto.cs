namespace TravelEaseServer.Dto
{
    public class NotificationRequestDto
    {
        public long UserId { get; set; }
        public string Message { get; set; }
        public int Category { get; set; }
    }

    public class NotificationResponseDto
    {
        public long NotificationId { get; set; }
        public long UserId { get; set; }
        public string Message { get; set; }
        public int Category { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ReadDate { get; set; }
    }

    public class NotificationSearchDto
    {
        public long? UserId { get; set; }
        public int? Category { get; set; }
        public int? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
