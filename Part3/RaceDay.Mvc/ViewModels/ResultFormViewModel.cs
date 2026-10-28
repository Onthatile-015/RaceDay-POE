using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.ViewModels
{
    public class ResultFormViewModel
    {
        public int EnrolmentId { get; set; }

        [Required]
        public TimeSpan? FinishTime { get; set; }

        [Required, Range(1, 100000)]
        public int? Position { get; set; }
    }
}
