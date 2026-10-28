using RaceDay.Mvc.Models;

namespace RaceDay.Mvc.ViewModels
{
    public class HomeViewModel
    {
        public EventFilterViewModel Filter { get; set; } = new();
        public List<EventSummary> Events { get; set; } = new();
    }
}
