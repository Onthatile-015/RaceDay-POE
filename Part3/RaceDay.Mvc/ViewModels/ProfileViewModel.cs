using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.ViewModels
{
    public class ProfileViewModel
    {
        [Required, StringLength(100), Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = string.Empty;

        // display only
        public string? ProfilePictureUrl { get; set; }
    }
}

