namespace RaceDay.Mvc.Models
{
    public class EventSummary
    {
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public decimal DistanceKm { get; set; }
        public string Status { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string? BannerImageUrl { get; set; }
        public string? City { get; set; }
        public int TotalEnrolments { get; set; }

    }
}
