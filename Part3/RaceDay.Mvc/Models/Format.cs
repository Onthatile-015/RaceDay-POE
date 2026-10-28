namespace RaceDay.Mvc.Models
{
    public static class Format
    {
        public static string Ordinal(int n)
        {
            var suffix = (n % 100) is 11 or 12 or 13
                ? "th"
                : (n % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" };
            return $"{n}{suffix}";
        }
    }
}
