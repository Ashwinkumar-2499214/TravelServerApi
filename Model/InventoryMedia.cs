using System.ComponentModel.DataAnnotations;

namespace TravelEaseServer.Model
{
    public class InventoryMedia
    {
        [Key]
        public long MediaId { get; set; }
        public long InventoryId { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
        public string MediaType { get; set; } // "image" or "video"
        public DateTime UploadedDate { get; set; }

        public virtual Inventory Inventory { get; set; }
    }
}
