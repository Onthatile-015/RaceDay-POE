using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    public class EventDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public decimal DistanceKm { get; set; }

        public string? Status { get; set; } // optional on create, used on update

    }
}
