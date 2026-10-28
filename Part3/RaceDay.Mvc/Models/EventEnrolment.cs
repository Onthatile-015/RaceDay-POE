namespace RaceDay.Mvc.Models
{
    public class EventEnrolment
    {
        public int EnrolmentId { get; set; }
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ParticipantName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int? ResultId { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }
    }
}