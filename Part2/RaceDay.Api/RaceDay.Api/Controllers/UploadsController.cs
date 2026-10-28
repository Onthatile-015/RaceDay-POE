using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Data;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class UploadsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly BlobStorageService _blobs;

        public UploadsController(RaceDayDbContext context, BlobStorageService blobs)
        {
            _context = context;
            _blobs = blobs;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>Uploads a banner image for an event owned by the logged-in Organiser and stores it in Azure Blob Storage.</summary>
        /// <remarks>Send the image as multipart form data in a field named "file". JPG, PNG, WEBP or GIF, up to 5 MB.</remarks>
        /// <response code="200">Banner saved. Returns the image URL.</response>
        /// <response code="400">The file is missing, too large or not an image.</response>
        /// <response code="403">You do not own this event.</response>
        /// <response code="404">Event not found.</response>
        /// <response code="503">Image storage is not configured.</response>
        [HttpPost("events/{id}/banner")]
        [Authorize(Roles = "Organiser")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadBanner(int id, IFormFile file)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != CurrentUserId) return Forbid();

            var error = BlobStorageService.Validate(file);
            if (error != null) return BadRequest(new { message = error });
            if (!_blobs.IsConfigured)
                return StatusCode(503, new { message = "Image storage isn't available right now." });

            try
            {
                ev.BannerImageUrl = await _blobs.UploadImageAsync(file, "event-banners");
                await _context.SaveChangesAsync();
                return Ok(new { ev.BannerImageUrl });
            }
            catch (Azure.RequestFailedException)
            {
                return StatusCode(502, new { message = "The image couldn't be saved. Please try again." });
            }
        }

        /// <summary>Uploads a profile picture for the logged-in user and stores it in Azure Blob Storage.</summary>
        /// <remarks>Send the image as multipart form data in a field named "file". JPG, PNG, WEBP or GIF, up to 5 MB.</remarks>
        /// <response code="200">Picture saved. Returns the image URL.</response>
        /// <response code="400">The file is missing, too large or not an image.</response>
        /// <response code="503">Image storage is not configured.</response>
        [HttpPost("users/me/picture")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            var user = await _context.Users.FindAsync(CurrentUserId);
            if (user == null) return NotFound();

            var error = BlobStorageService.Validate(file);
            if (error != null) return BadRequest(new { message = error });
            if (!_blobs.IsConfigured)
                return StatusCode(503, new { message = "Image storage isn't available right now." });

            try
            {
                user.ProfilePictureUrl = await _blobs.UploadImageAsync(file, "profile-pictures");
                await _context.SaveChangesAsync();
                return Ok(new { user.ProfilePictureUrl });
            }
            catch (Azure.RequestFailedException)
            {
                return StatusCode(502, new { message = "The image couldn't be saved. Please try again." });
            }
        }
    }
}
