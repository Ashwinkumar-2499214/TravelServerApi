namespace TravelEaseServer.Dto
{
    public class InventoryRequestDto
    {
        public long PartnerId { get; set; }
        public string ItemType { get; set; }
        public string Description { get; set; }
        public int Availability { get; set; }
        public decimal Price { get; set; }
    }

    public class InventoryResponseDto
    {
        public long InventoryId { get; set; }
        public long PartnerId { get; set; }
        public string ItemType { get; set; }
        public string Description { get; set; }
        public int Availability { get; set; }
        public decimal Price { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class InventorySearchDto
    {
        public long? PartnerId { get; set; }
        public string ItemType { get; set; }
        public int? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class InventoryAvailabilityDto
    {
        public long InventoryId { get; set; }
        public int NewAvailability { get; set; }
        public int Status { get; set; }
    }
}
