using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        public int OrganiserId { get; set; }

        [ForeignKey("OrganiserId")]
        public User? Organiser { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required, Column(TypeName = "decimal(5,2)")]
        public decimal DistanceKm { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Upcoming";

        public ICollection<Category>? Categories { get; set; }
        public ICollection<Venue>? Venues { get; set; }

    }
}
