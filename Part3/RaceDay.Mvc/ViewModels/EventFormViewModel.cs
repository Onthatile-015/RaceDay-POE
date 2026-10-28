using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.ViewModels
{
    public class EventFormViewModel
    {
        public int? EventId { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required, Display(Name = "Date and time"), DataType(DataType.DateTime)]
        public DateTime EventDate { get; set; } = DateTime.Today.AddDays(30).AddHours(7);

        [Required, Range(0.1, 500, ErrorMessage = "Distance must be between 0.1 and 500 km."), Display(Name = "Distance (km)")]
        public decimal? DistanceKm { get; set; }

        [Required, Display(Name = "Event type")]
        public string EventType { get; set; } = "Run";

        [Required, StringLength(200), Display(Name = "Street address / location")]
        public string Address { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string City { get; set; } = string.Empty;

        public string Status { get; set; } = "Upcoming";
        public string? BannerImageUrl { get; set; }
    }
}
