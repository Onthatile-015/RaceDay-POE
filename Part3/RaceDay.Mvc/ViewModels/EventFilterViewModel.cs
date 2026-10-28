using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.ViewModels
{
    public class EventFilterViewModel
    {
        [Display(Name = "Search")] public string? Search { get; set; }
        [Display(Name = "Event type")] public string? EventType { get; set; }
        public string? City { get; set; }
        [Display(Name = "From"), DataType(DataType.Date)] public DateTime? From { get; set; }
        [Display(Name = "To"), DataType(DataType.Date)] public DateTime? To { get; set; }
        [Display(Name = "Max distance (km)")] public decimal? MaxDistanceKm { get; set; }
    }
}
