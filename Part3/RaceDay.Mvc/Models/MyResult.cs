namespace RaceDay.Mvc.Models
{
    public class MyResult
    {
        public int ResultId { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public int TotalFinishers { get; set; }
    }
}
