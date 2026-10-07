using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using System.Security.Claims;

namespace RaceDay.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        public EnrolmentsController(RaceDayDbContext context) => _context = context;

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // DELETE /api/enrolments/{id}  (Participant, own only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> Cancel(int id)
        {
            var enrolment = await _context.Enrolments.FindAsync(id);
            if (enrolment == null) return NotFound();
            if (enrolment.ParticipantId != CurrentUserId) return Forbid();

            _context.Enrolments.Remove(enrolment);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST /api/enrolments/{id}/results  (Organiser)
        [HttpPost("{id}/results")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CaptureResult(int id, ResultDto dto)
        {
            var enrolment = await _context.Enrolments
                .Include(e => e.Category)
                .ThenInclude(c => c!.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == id);

            if (enrolment == null) return NotFound();
            if (enrolment.Category!.Event!.OrganiserId != CurrentUserId) return Forbid();

            bool alreadyCaptured = await _context.Results.AnyAsync(r => r.EnrolmentId == id);
            if (alreadyCaptured) return Conflict(new { message = "Result already captured for this enrolment." });

            var result = new Models.Result
            {
                EnrolmentId = id,
                FinishTime = dto.FinishTime,
                Position = dto.Position,
                CapturedByOrganiserId = CurrentUserId
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, result);
        }

        // GET /api/enrolments/{id}/results  (Any logged in)
        [HttpGet("{id}/results")]
        public async Task<IActionResult> GetResult(int id)
        {
            var result = await _context.Results.FirstOrDefaultAsync(r => r.EnrolmentId == id);
            if (result == null) return NotFound();

            return Ok(new { result.ResultId, result.FinishTime, result.Position });
        }


    }
}
