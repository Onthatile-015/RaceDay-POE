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

        // GET /api/users/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var user = await _context.Users.FindAsync(CurrentUserId);
            if (user == null) return NotFound();

            return Ok(new { user.UserId, user.FullName, user.Email, user.Role, user.CreatedAt });
        }

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

        // GET /api/users/me/enrolments  (Participant)
        [HttpGet("me/enrolments")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var enrolments = await _context.Enrolments
                .Where(e => e.ParticipantId == CurrentUserId)
                .Include(e => e.Category)
                .ThenInclude(c => c!.Event)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.EnrolmentDate,
                    e.Status,
                    CategoryName = e.Category!.Name,
                    EventName = e.Category.Event!.Name
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET /api/users/me/results  (Participant)
        [HttpGet("me/results")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            var results = await _context.Results
                .Where(r => r.Enrolment!.ParticipantId == CurrentUserId)
                .Include(r => r.Enrolment)
                .ThenInclude(e => e!.Category)
                .ThenInclude(c => c!.Event)
                .Select(r => new
                {
                    r.ResultId,
                    r.FinishTime,
                    r.Position,
                    CategoryName = r.Enrolment!.Category!.Name,
                    EventName = r.Enrolment.Category.Event!.Name
                })
                .ToListAsync();

            return Ok(results);
        }


    }
}
