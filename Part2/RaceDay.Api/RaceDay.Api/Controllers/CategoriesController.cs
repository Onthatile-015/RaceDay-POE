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
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        public CategoriesController(RaceDayDbContext context) => _context = context;

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // PUT /api/categories/{id}  (Organiser, owner of parent event only)
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Update(int id, CategoryDto dto)
        {
            var category = await _context.Categories.Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null) return NotFound();
            if (category.Event!.OrganiserId != CurrentUserId) return Forbid();

            category.Name = dto.Name;
            category.MaxParticipants = dto.MaxParticipants;
            category.EntryFee = dto.EntryFee;

            await _context.SaveChangesAsync();
            return Ok(category);
        }

        // DELETE /api/categories/{id}  (Organiser, owner of parent event only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null) return NotFound();
            if (category.Event!.OrganiserId != CurrentUserId) return Forbid();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST /api/categories/{id}/enrolments  (Participant)
        [HttpPost("{id}/enrolments")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> Enrol(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Enrolments)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null) return NotFound();

            bool alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e => e.ParticipantId == CurrentUserId && e.CategoryId == id);
            if (alreadyEnrolled)
                return Conflict(new { message = "Already enrolled in this category." });

            int currentCount = category.Enrolments!.Count(e => e.Status == "Active");
            if (currentCount >= category.MaxParticipants)
                return Conflict(new { message = "Category is full." });

            var enrolment = new Models.Enrolment
            {
                ParticipantId = CurrentUserId,
                CategoryId = id,
                EnrolmentDate = DateTime.Now,
                Status = "Active"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, enrolment);
        }

    }
}
