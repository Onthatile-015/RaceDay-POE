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

        /// <summary>Cancels the logged-in Participant's own enrolment.</summary>
        /// <response code="204">Enrolment cancelled.</response>
        /// <response code="403">Not your enrolment.</response>
        // DELETE /api/enrolments/{id}  (Participant, own only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> Cancel(int id)
        {
            var enrolment = await _context.Enrolments.FindAsync(id);
            if (enrolment == null) return NotFound();
            if (enrolment.ParticipantId != CurrentUserId) return Forbid();
            bool hasResult = await _context.Results.AnyAsync(r => r.EnrolmentId == id);
            if (hasResult)
                return Conflict(new { message = "This entry already has a result and can't be withdrawn." });

            _context.Enrolments.Remove(enrolment);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Captures a result for an enrolment, by the Organiser who owns the related event.</summary>
        /// <response code="201">Result captured.</response>
        /// <response code="409">A result already exists for this enrolment.</response>
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

            enrolment.Status = "Completed";
            _context.Results.Add(result);

            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>Returns the captured result for a specific enrolment.</summary>
        /// <response code="200">Returns the result.</response>
        /// <response code="404">No result captured yet.</response>
        // GET /api/enrolments/{id}/results  (Any logged in)
        [HttpGet("{id}/results")]
        public async Task<IActionResult> GetResult(int id)
        {
            var result = await _context.Results.FirstOrDefaultAsync(r => r.EnrolmentId == id);
            if (result == null) return NotFound();

            return Ok(new { result.ResultId, result.FinishTime, result.Position });
        }

        /// <summary>Updates the captured result for an enrolment, by the Organiser who owns the event.</summary>
        /// <response code="200">Result updated.</response>
        /// <response code="403">You do not own this event.</response>
        /// <response code="404">Enrolment or result not found.</response>
        [HttpPut("{id}/results")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateResult(int id, ResultDto dto)
        {
            var enrolment = await _context.Enrolments
                .Include(e => e.Category).ThenInclude(c => c!.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == id);
            if (enrolment == null) return NotFound();
            if (enrolment.Category!.Event!.OrganiserId != CurrentUserId) return Forbid();

            var result = await _context.Results.FirstOrDefaultAsync(r => r.EnrolmentId == id);
            if (result == null) return NotFound();

            result.FinishTime = dto.FinishTime;
            result.Position = dto.Position;
            await _context.SaveChangesAsync();

            return Ok(new { result.ResultId, result.FinishTime, result.Position });
        }

    }
}
