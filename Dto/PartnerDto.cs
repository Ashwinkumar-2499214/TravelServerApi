namespace TravelEaseServer.Dto
{
    public class PartnerRequestDto
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string Address { get; set; }
    }

    public class PartnerResponseDto
    {
        public long PartnerId { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string Address { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

    }

    public class PartnerSearchDto
    {
        public string SearchTerm { get; set; }
        public int? Type { get; set; }
        public int? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PartnerStatusUpdateDto
    {
        public long PartnerId { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public int NewStatus { get; set; }
    }
}
