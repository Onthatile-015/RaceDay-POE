using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;
using System.Net;
using System.Security.Claims;
using RaceDay.Mvc.Models;

namespace RaceDay.Mvc.Controllers
{
    [Authorize(Roles = "Organiser")]
    public class OrganiserController : ApiAwareController
    {
        private readonly RaceDayApiClient _api;
        public OrganiserController(RaceDayApiClient api) => _api = api;

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var events = await _api.GetMyEventsAsync();
                var upcoming = events.Where(e => e.EventDate >= DateTime.Today)
                                     .OrderBy(e => e.EventDate).ToList();

                return View(new DashboardViewModel
                {
                    Events = events,
                    TotalEvents = events.Count,
                    TotalEnrolments = events.Sum(e => e.TotalEnrolments),
                    UpcomingCount = upcoming.Count,
                    NextEvent = upcoming.FirstOrDefault()
                });
            }
            catch (HttpRequestException ex) { return await HandleApiErrorAsync(ex); }



        }


        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public async Task<IActionResult> Events()
        {
            try { return View(await _api.GetMyEventsAsync()); }
            catch (HttpRequestException ex) { return await HandleApiErrorAsync(ex); }
        }

        [HttpGet]
        public IActionResult CreateEvent() => View("EventForm", new EventFormViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(EventFormViewModel model, IFormFile? bannerFile)
        {
            if (!ModelState.IsValid) return View("EventForm", model);
            try
            {
                var response = await _api.CreateEventAsync(model);
                response.EnsureSuccessStatusCode();

                var created = await response.Content.ReadFromJsonAsync<EventSummary>();
                var uploadError = created == null ? null : await TryUploadBannerAsync(created.EventId, bannerFile);

                TempData["Success"] = "Event created.";
                if (uploadError != null)
                    TempData["Error"] = $"{uploadError} You can add a banner later by editing the event.";
                return RedirectToAction(nameof(Events));
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                ModelState.AddModelError(string.Empty, "The event details were not accepted. Please check them and try again.");
                return View("EventForm", model);
            }
            catch (HttpRequestException ex) { return await HandleApiErrorAsync(ex); }
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)

        {
            try
            {
                var ev = await _api.GetEventAsync(id);
                if (ev == null) return View("NotFound");
                if (ev.OrganiserId != CurrentUserId) return RedirectToAction("AccessDenied", "Account");

                var venue = ev.Venues.FirstOrDefault();
                return View("EventForm", new EventFormViewModel
                {
                    EventId = ev.EventId,
                    Name = ev.Name,
                    Description = ev.Description,
                    EventDate = ev.EventDate,
                    DistanceKm = ev.DistanceKm,
                    EventType = ev.EventType,
                    Address = venue?.Address ?? string.Empty,
                    City = venue?.City ?? string.Empty,
                    Status = ev.Status,
                    BannerImageUrl = ev.BannerImageUrl
                });
            }
            catch (HttpRequestException ex) { return await HandleApiErrorAsync(ex); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(int id, EventFormViewModel model, IFormFile? bannerFile)
        {
            model.EventId = id;
            if (!ModelState.IsValid) return View("EventForm", model);
            try
            {
                var response = await _api.UpdateEventAsync(id, model);
                response.EnsureSuccessStatusCode();

                var uploadError = await TryUploadBannerAsync(id, bannerFile);

                TempData["Success"] = "Event updated.";
                if (uploadError != null) TempData["Error"] = uploadError;
                return RedirectToAction(nameof(Events));
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                ModelState.AddModelError(string.Empty, "The event details were not accepted. Please check them and try again.");
                return View("EventForm", model);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return View("NotFound");
            }
            catch (HttpRequestException ex) { return await HandleApiErrorAsync(ex); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            try
            {
                var response = await _api.DeleteEventAsync(id);
                response.EnsureSuccessStatusCode();
                TempData["Success"] = "Event deleted.";
                return RedirectToAction(nameof(Events));
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = "That event no longer exists.";
                return RedirectToAction(nameof(Events));
            }
            catch (HttpRequestException ex) { return await HandleApiErrorAsync(ex); }
        }



        private async Task<IActionResult> CategoriesPageAsync(int eventId, CategoryFormViewModel? form = null)
        {
            var ev = await _api.GetEventAsync(eventId);
            if (ev == null) return View("NotFound");
            if (ev.OrganiserId != CurrentUserId) return RedirectToAction("AccessDenied", "Account");

            return View("Categories", new CategoriesViewModel
            {
                EventId = ev.EventId,
                EventName = ev.Name,
                Categories = ev.Categories,
                NewCategory = form ?? new CategoryFormViewModel()
            });
        }

        [HttpGet]
        public async Task<IActionResult> Categories(int id)
        {
            try { return await CategoriesPageAsync(id); }
            catch (HttpRequestException ex) { return await HandleApiErrorAsync(ex); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(int id, [Bind(Prefix = "NewCategory")] CategoryFormViewModel newCategory)
        {
            try
            {
                if (!ModelState.IsValid) return await CategoriesPageAsync(id, newCategory);

                var response = await _api.CreateCategoryAsync(id, newCategory);
                response.EnsureSuccessStatusCode();
                TempData["Success"] = "Category added.";

                return RedirectToAction(nameof(Categories), new { id });
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                TempData["Error"] = "The category details were not accepted. Please check them and try again.";
                return RedirectToAction(nameof(Categories), new { id });
            }
            catch (HttpRequestException ex) { return await HandleApiErrorAsync(ex); }
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int eventId, int id)
        {
            try
            {
                var ev = await _api.GetEventAsync(eventId);
                if (ev == null) return View("NotFound");
                if (ev.OrganiserId != CurrentUserId) return RedirectToAction("AccessDenied", "Account");

                var cat = ev.Categories.FirstOrDefault(c => c.CategoryId == id);
                if (cat == null) return View("NotFound");

                ViewData["EventId"] = eventId;
                ViewData["CategoryId"] = id;
                ViewData["EventName"] = ev.Name;
                return View(new CategoryFormViewModel
                {
                    Name = cat.Name,
                    MaxParticipants = cat.MaxParticipants,
                    EntryFee = cat.EntryFee
                });
            }
            catch (HttpRequestException ex) { return await HandleApiErrorAsync(ex); }
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int eventId, int id, CategoryFormViewModel model)
        {
            ViewData["EventId"] = eventId;
            ViewData["CategoryId"] = id;
            if (!ModelState.IsValid) return View(model);
            try
            {
                var response = await _api.UpdateCategoryAsync(id, model);
                response.EnsureSuccessStatusCode();
                TempData["Success"] = "Category updated.";
                return RedirectToAction(nameof(Categories), new { id = eventId });
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                ModelState.AddModelError(string.Empty, "The category details were not accepted. Please check them and try again.");
                return View(model);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return View("NotFound");
            }
            catch (HttpRequestException ex) { return await HandleApiErrorAsync(ex); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int eventId, int id)
        {
            try
            {
                var response = await _api.DeleteCategoryAsync(id);

                response.EnsureSuccessStatusCode();
                TempData["Success"] = "Category deleted.";
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                TempData["Error"] = "This category already has enrolments, so it can't be deleted.";
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = "That category no longer exists.";
            }
            catch (HttpRequestException ex) { return await HandleApiErrorAsync(ex); }

            return RedirectToAction(nameof(Categories), new { id = eventId });
        }

        private async Task<IActionResult> EventEnrolmentsPageAsync(int id, string viewName)
        {
            var ev = await _api.GetEventAsync(id);
            if (ev == null) return View("NotFound");
            if (ev.OrganiserId != CurrentUserId) return RedirectToAction("AccessDenied", "Account");

            var rows = await _api.GetEventEnrolmentsAsync(id);
            return View(viewName, new EventEnrolmentViewModel
            {
                EventId = ev.EventId,
                EventName = ev.Name,
                EventDate = ev.EventDate,
                Enrolments = rows
            });
        }

        public async Task<IActionResult> Enrolments(int id)
        {
            try { return await EventEnrolmentsPageAsync(id, "Enrolments"); }
            catch (HttpRequestException ex) { return await HandleApiErrorAsync(ex); }
        }

        public async Task<IActionResult> Results(int id)
        {
            try { return await EventEnrolmentsPageAsync(id, "Results"); }
            catch (HttpRequestException ex) { return await HandleApiErrorAsync(ex); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveResult(int eventId, ResultFormViewModel model, bool hasResult)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please enter a valid finish time and a finishing position.";
                return RedirectToAction(nameof(Results), new { id = eventId });
            }
            try
            {
                var response = hasResult
                    ? await _api.UpdateResultAsync(model.EnrolmentId, model)
                    : await _api.CaptureResultAsync(model.EnrolmentId, model);
                response.EnsureSuccessStatusCode();
                TempData["Success"] = "Result saved and published.";
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                TempData["Error"] = "A result has already been captured for that entry. Refresh and edit it instead.";
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                TempData["Error"] = "The result was not accepted. Please check the values and try again.";
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = "That entry no longer exists.";
            }
            catch (HttpRequestException ex) { return await HandleApiErrorAsync(ex); }

            return RedirectToAction(nameof(Results), new { id = eventId });
        }

        private async Task<string?> TryUploadBannerAsync(int eventId, IFormFile? bannerFile)
        {
            if (bannerFile == null || bannerFile.Length == 0) return null;

            var response = await _api.UploadBannerAsync(eventId, bannerFile);
            if (response.IsSuccessStatusCode) return null;

            return response.StatusCode switch
            {
                HttpStatusCode.BadRequest => await ReadApiMessageAsync(response) ?? "That image wasn't accepted.",
                HttpStatusCode.ServiceUnavailable => "Image storage isn't available right now.",
                _ => "The banner couldn't be uploaded."
            };
        }
    }
}
