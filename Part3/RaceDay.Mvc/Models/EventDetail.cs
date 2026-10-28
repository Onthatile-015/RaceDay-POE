namespace RaceDay.Mvc.Models
{
    public class EventDetail
    {
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public decimal DistanceKm { get; set; }
        public string Status { get; set; } = string.Empty;
        public string EventType { get; set; } = "Run";
        public string? BannerImageUrl { get; set; }
        public int OrganiserId { get; set; }
        public List<CategoryInfo> Categories { get; set; } = new();
        public List<VenueInfo> Venues { get; set; } = new();
    }
    public class CategoryInfo
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int MaxParticipants { get; set; }
        public decimal EntryFee { get; set; }
    }
    public class VenueInfo
    {
        public int VenueId { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }
}
