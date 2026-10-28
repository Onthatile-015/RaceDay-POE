using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.ViewModels
{
    public class CategoryFormViewModel
    {
        [Required, StringLength(50), Display(Name = "Category name")]
        public string Name { get; set; } = string.Empty;

        [Required, Range(1, 100000), Display(Name = "Maximum participants")]
        public int? MaxParticipants { get; set; } = 100;

        [Required, Range(0, 100000), Display(Name = "Entry fee (R)")]
        public decimal? EntryFee { get; set; } = 0;
    }
}
