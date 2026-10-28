using RaceDay.Mvc.Models;

namespace RaceDay.Mvc.ViewModels
{
    public class DashboardViewModel
    {
        public List<EventSummary> Events { get; set; } = new();
        public int TotalEvents { get; set; }
        public int TotalEnrolments { get; set; }
        public int UpcomingCount { get; set; }
        public EventSummary? NextEvent { get; set; }
    }
}