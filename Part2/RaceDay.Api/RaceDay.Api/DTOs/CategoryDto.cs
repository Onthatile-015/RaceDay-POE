using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    public class CategoryDto
    {
        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        public int MaxParticipants { get; set; } = 100;

        public decimal EntryFee { get; set; } = 0;

    }
}
