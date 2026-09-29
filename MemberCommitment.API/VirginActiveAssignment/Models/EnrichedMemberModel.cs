namespace VirginActiveAssignment.Models
{
    public class EnrichedMemberModel
    {
        public MemberModel Member { get; set; }
        public List<RockModel>? Rocks { get; set; }
        public bool EnrichmentDataAvailable { get; set; }
    }
}
