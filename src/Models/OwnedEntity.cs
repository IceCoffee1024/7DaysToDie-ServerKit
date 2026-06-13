namespace SdtdServerKit.Models
{
    public class OwnedEntity
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public Position LastKnownPosition { get; set; }
        public bool HasLastKnownPosition { get; set; }
    }
}
