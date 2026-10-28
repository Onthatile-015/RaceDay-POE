using RaceDay.Mvc.Models;

namespace RaceDay.Mvc.ViewModels
{
    public class EventEnrolmentViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public List<EventEnrolment> Enrolments { get; set; } = new();
    }
}
