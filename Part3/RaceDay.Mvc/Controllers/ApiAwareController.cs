using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;

namespace RaceDay.Mvc.Controllers
{
    public abstract class ApiAwareController : Controller
    {
        protected async Task<IActionResult> HandleApiErrorAsync(HttpRequestException ex)
        {
            if (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["Error"] = "Your session has expired. Please log in again.";
                return RedirectToAction("Login", "Account");
            }
            if (ex.StatusCode == HttpStatusCode.Forbidden)
                return RedirectToAction("AccessDenied", "Account");

            return View("ApiError");
        }
        protected static async Task<string?> ReadApiMessageAsync(HttpResponseMessage response)
        {
            try { return (await response.Content.ReadFromJsonAsync<ApiMessage>())?.Message; }
            catch { return null; }
        }
    }
}
