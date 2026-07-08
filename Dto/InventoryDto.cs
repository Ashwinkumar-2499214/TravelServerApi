using System.Text.Json.Serialization;
using TravelEaseServer.Enum;

namespace TravelEaseServer.Dto
{
    public class InventoryRequestDto
    {
        [JsonIgnore]
        public long PartnerId { get; set; }
        public string ItemType { get; set; }
        public string Description { get; set; }
        public int Availability { get; set; }
        public decimal Price { get; set; }
        public InventoryStatus Status { get; set; } = InventoryStatus.Available;
    }

    public class InventoryMediaDto
    {
        public long MediaId { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
        public string MediaType { get; set; }
        public DateTime UploadedDate { get; set; }
    }

    public class InventoryResponseDto
    {
        public long InventoryId { get; set; }
        public long PartnerId { get; set; }
        public string ItemType { get; set; }
        public string Description { get; set; }
        public int Availability { get; set; }
        public decimal Price { get; set; }
        public InventoryStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<InventoryMediaDto> Media { get; set; } = new();
    }

    public class InventorySearchDto
    {
        public long? PartnerId { get; set; }
        public string ItemType { get; set; }
        public InventoryStatus? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class InventoryAvailabilityDto
    {
        public long InventoryId { get; set; }
        [JsonPropertyName("availability")]
        public int NewAvailability { get; set; }
        public InventoryStatus? Status { get; set; }
    }
}
