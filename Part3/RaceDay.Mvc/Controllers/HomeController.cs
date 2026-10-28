using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers
{
    public class HomeController : ApiAwareController
    {
        private readonly RaceDayApiClient _api;
        public HomeController(RaceDayApiClient api) => _api = api;

        public async Task<IActionResult> Index([FromQuery] EventFilterViewModel filter)
        {
            try
            {
                var events = await _api.GetEventsAsync(filter);
                return View(new HomeViewModel
                {
                    Filter = filter,
                    Events = events.Where(e => e.Status != "Cancelled" && e.Status != "Completed").ToList()
                });
            }
            catch (HttpRequestException ex) { return await HandleApiErrorAsync(ex); }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var ev = await _api.GetEventAsync(id);
                return ev == null ? View("NotFound") : View(ev);
            }
            catch (HttpRequestException ex) { return await HandleApiErrorAsync(ex); }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}