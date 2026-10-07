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
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        public EventsController(RaceDayDbContext context) => _context = context;

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET /api/events  (public)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var events = await _context.Events
                .Select(e => new { e.EventId, e.Name, e.EventDate, e.DistanceKm, e.Status })
                .ToListAsync();
            return Ok(events);
        }

        // GET /api/events/{id}  (public)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Categories)
                .Include(e => e.Venues)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null) return NotFound();

            return Ok(new
            {
                ev.EventId,
                ev.Name,
                ev.Description,
                ev.EventDate,
                ev.DistanceKm,
                ev.Status,
                ev.OrganiserId,
                Categories = ev.Categories!.Select(c => new { c.CategoryId, c.Name, c.MaxParticipants, c.EntryFee }),
                Venues = ev.Venues!.Select(v => new { v.VenueId, v.Address, v.City })
            });
        }

        // POST /api/events  (Organiser)
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Create(EventDto dto)
        {
            var ev = new Models.Event
            {
                OrganiserId = CurrentUserId,
                Name = dto.Name,
                Description = dto.Description,
                EventDate = dto.EventDate,
                DistanceKm = dto.DistanceKm,
                Status = "Upcoming"
            };

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, ev);
        }

        // PUT /api/events/{id}  (Organiser, owner only)
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Update(int id, EventDto dto)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != CurrentUserId) return Forbid();

            ev.Name = dto.Name;
            ev.Description = dto.Description;
            ev.EventDate = dto.EventDate;
            ev.DistanceKm = dto.DistanceKm;
            if (!string.IsNullOrWhiteSpace(dto.Status)) ev.Status = dto.Status;

            await _context.SaveChangesAsync();
            return Ok(ev);
        }

        // DELETE /api/events/{id}  (Organiser, owner only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != CurrentUserId) return Forbid();

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET /api/events/{id}/categories  (public)
        [HttpGet("{id}/categories")]
        public async Task<IActionResult> GetCategories(int id)
        {
            bool eventExists = await _context.Events.AnyAsync(e => e.EventId == id);
            if (!eventExists) return NotFound();

            var categories = await _context.Categories
                .Where(c => c.EventId == id)
                .Select(c => new { c.CategoryId, c.Name, c.MaxParticipants, c.EntryFee })
                .ToListAsync();

            return Ok(categories);
        }

        // POST /api/events/{id}/categories  (Organiser, owner only)
        [HttpPost("{id}/categories")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateCategory(int id, CategoryDto dto)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != CurrentUserId) return Forbid();

            var category = new Models.Category
            {
                EventId = id,
                Name = dto.Name,
                MaxParticipants = dto.MaxParticipants,
                EntryFee = dto.EntryFee
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, category);
        }

        // GET /api/events/{id}/enrolments  (Organiser, owner only)
        [HttpGet("{id}/enrolments")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEventEnrolments(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != CurrentUserId) return Forbid();

            var enrolments = await _context.Enrolments
                .Where(en => en.Category!.EventId == id)
                .Include(en => en.Participant)
                .Include(en => en.Category)
                .Select(en => new
                {
                    en.EnrolmentId,
                    en.EnrolmentDate,
                    en.Status,
                    ParticipantName = en.Participant!.FullName,
                    CategoryName = en.Category!.Name
                })
                .ToListAsync();

            return Ok(enrolments);

        }

        }
    }
