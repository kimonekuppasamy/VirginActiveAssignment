using VirginActiveAssignment.Models.Enums;

namespace VirginActiveAssignment.Models
{
    public class RockModel
    {
        public Guid RockId { get; set; }
        public int MemberId { get; set; }
        public string Title { get; set; }
        public RockCategoryEnum Category { get; set; }
        public DateTimeOffset DueDate { get; set; }
        public RockStatusEnum Status { get; set; } 
        public string? Note { get; set; } = string.Empty;

    }

    public class RockRequestModel
    {
        public int MemberId { get; set; }
        public string Title { get; set; }
        public RockCategoryEnum Category { get; set; }
        public DateTimeOffset DueDate { get; set; }
        public string? Note { get; set; } = string.Empty;
    }
}
