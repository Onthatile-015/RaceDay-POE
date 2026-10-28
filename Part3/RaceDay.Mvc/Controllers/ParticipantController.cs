using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers
{
    [Authorize(Roles = "Participant")]
    public class ParticipantController : ApiAwareController
    {
        private readonly RaceDayApiClient _api;
        public ParticipantController(RaceDayApiClient api) => _api = api;

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {
            try
            {
                var response = await _api.EnrolAsync(categoryId);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "You're enrolled! Your entry has been recorded.";
                    return RedirectToAction(nameof(Enrolments));
                }
                else if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    var msg = await response.Content.ReadFromJsonAsync<ApiMessage>();
                    TempData["Error"] = msg?.Message ?? "You can't enrol in that category.";
                }
                else
                    response.EnsureSuccessStatusCode(); // 401/403/404 are handled below

                return RedirectToAction("Details", "Home", new { id = eventId });
            }
            catch (HttpRequestException ex) { return await HandleApiErrorAsync(ex); }
        }


        public async Task<IActionResult> Enrolments()
        {
            try { return View(await _api.GetMyEnrolmentsAsync()); }
            catch (HttpRequestException ex) { return await HandleApiErrorAsync(ex); }
        }

        public async Task<IActionResult> Results()
        {
            try { return View(await _api.GetMyResultsAsync()); }
            catch (HttpRequestException ex) { return await HandleApiErrorAsync(ex); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(int id)
        {
            try
            {
                var response = await _api.CancelEnrolmentAsync(id);

                if (response.IsSuccessStatusCode)
                    TempData["Success"] = "Your enrolment has been withdrawn.";
                else if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    var msg = await response.Content.ReadFromJsonAsync<ApiMessage>();
                    TempData["Error"] = msg?.Message ?? "That enrolment can't be withdrawn.";
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                    TempData["Error"] = "That enrolment no longer exists.";
                else
                    response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex) { return await HandleApiErrorAsync(ex); }


            return RedirectToAction(nameof(Enrolments));
        }

    }
}
