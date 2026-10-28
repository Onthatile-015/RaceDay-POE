namespace RaceDay.Mvc.Models
{
    public static class StatusDisplay
    {
        // Enrolments are recorded immediately, so "Active" is shown as Confirmed.
        public static string Label(string status) => status switch
        {
            "Active" => "Confirmed",
            _ => status
        };

        public static string CssClass(string status) => status switch
        {
            "Active" or "Confirmed" => "status-confirmed",
            "Completed" => "status-completed",
            "Withdrawn" or "Cancelled" => "status-withdrawn",
            _ => "status-pending"
        };
    }
}