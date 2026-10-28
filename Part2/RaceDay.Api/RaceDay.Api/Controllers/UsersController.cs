using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using System.Security.Claims;

namespace RaceDay.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        public UsersController(RaceDayDbContext context) => _context = context;

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>Returns the profile of the currently logged-in user.</summary>
        /// <response code="200">Returns the user profile.</response>
        /// <response code="401">Not authenticated.</response>
        // GET /api/users/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var user = await _context.Users.FindAsync(CurrentUserId);
            if (user == null) return NotFound();

            return Ok(new { user.UserId, user.FullName, user.Email, user.Role, user.CreatedAt, user.ProfilePictureUrl });
        }

        /// <summary>Updates the logged-in user's own profile details.</summary>
        /// <remarks>Expects fullName and email in the request body.</remarks>
        /// <response code="200">Profile updated.</response>
        /// <response code="409">Email already in use by another account.</response>
        // PUT /api/users/me
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile(UpdateProfileDto dto)
        {
            var user = await _context.Users.FindAsync(CurrentUserId);
            if (user == null) return NotFound();

            bool emailTaken = await _context.Users
                .AnyAsync(u => u.Email == dto.Email && u.UserId != CurrentUserId);
            if (emailTaken) return Conflict(new { message = "Email already in use." });

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            await _context.SaveChangesAsync();

            return Ok(new { user.UserId, user.FullName, user.Email, user.Role });
        }

        /// <summary>Lists all enrolments belonging to the logged-in Participant.</summary>
        /// <response code="200">Returns the participant's enrolments.</response>
        // GET /api/users/me/enrolments  (Participant)
        [HttpGet("me/enrolments")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyEnrolments()
        {
                             var enrolments = await _context.Enrolments
                    .Where(e => e.ParticipantId == CurrentUserId)
                     .OrderByDescending(e => e.EnrolmentDate)
                      .Select(e => new
               {
                     e.EnrolmentId, e.EnrolmentDate, e.Status,
                  CategoryName = e.Category!.Name,
                     EventId = e.Category.EventId,
                   EventName = e.Category.Event!.Name,
                  EventDate = e.Category.Event.EventDate,
                EventType = e.Category.Event.EventType
                                                            })
                        .ToListAsync();

                          return Ok(enrolments);
        }
        /// <summary>Returns the logged-in Participant's full personal result history.</summary>
        /// <response code="200">Returns the participant's results.</response>
        // GET /api/users/me/results  (Participant)
        [HttpGet("me/results")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            var results = await _context.Results
                .Where(r => r.Enrolment!.ParticipantId == CurrentUserId)
                .OrderByDescending(r => r.Enrolment!.Category!.Event!.EventDate)
                .Select(r => new
        {
                 r.ResultId,
                 r.FinishTime,
                 r.Position,
                 CategoryName = r.Enrolment!.Category!.Name,
                 EventName = r.Enrolment.Category.Event!.Name,
                 EventDate = r.Enrolment.Category.Event.EventDate,
                 TotalFinishers = _context.Results.Count(x => x.Enrolment!.CategoryId == r.Enrolment.CategoryId)
                         })
                  .ToListAsync();

            return Ok(results);
        }


    }
}
