using System.ComponentModel.DataAnnotations;

namespace TravelEaseServer.Model
{
    public class RetentionPolicy
    {
        [Key]
        public long RetentionPolicyId { get; set; }
        public string DataType { get; set; }
        public int RetentionDays { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
