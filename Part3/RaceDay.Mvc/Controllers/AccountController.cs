using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers
{
    public class AccountController : ApiAwareController
    {
        private readonly IHttpClientFactory _factory;
        private readonly RaceDayApiClient _api;
        public AccountController(IHttpClientFactory factory, RaceDayApiClient api)
        {
            _factory = factory;
            _api = api;
        }

        [HttpGet]
        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                var client = _factory.CreateClient("RaceDayApi");
                var response = await client.PostAsJsonAsync("api/auth/register",
                    new { model.FullName, model.Email, model.Password, model.Role });

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Account created. Please log in.";
                    return RedirectToAction(nameof(Login));

                }

                ModelState.AddModelError(string.Empty, response.StatusCode == HttpStatusCode.Conflict
                    ? "That email address is already registered."
                    : "We couldn't create your account. Please check your details and try again.");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "The service is unavailable right now. Please try again shortly.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);
            try
            {
                var client = _factory.CreateClient("RaceDayApi");
                var response = await client.PostAsJsonAsync("api/auth/login", new { model.Email, model.Password });

                if (!response.IsSuccessStatusCode)
                {

                    ModelState.AddModelError(string.Empty, response.StatusCode == HttpStatusCode.Unauthorized
                        ? "Invalid email or password."
                        : "We couldn't log you in. Please try again.");
                    return View(model);
                }

                var apiCookie = ExtractApiCookie(response);
                var user = await response.Content.ReadFromJsonAsync<ApiLoginResult>();
                if (apiCookie == null || user == null)
                {
                    ModelState.AddModelError(string.Empty, "We couldn't log you in. Please try again.");
                    return View(model);
                }

                HttpContext.Session.SetString("ApiAuthCookie", apiCookie);

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new(ClaimTypes.Name, user.FullName),
                    new(ClaimTypes.Role, user.Role)
                };
                var principal = new ClaimsPrincipal(
                    new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return user.Role == "Organiser"
                      ? RedirectToAction("Dashboard", "Organiser")
                      : RedirectToAction("Index", "Home");
            }
            catch (HttpRequestException)

            {
                ModelState.AddModelError(string.Empty, "The service is unavailable right now. Please try again shortly.");
                return View(model);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try { await _factory.CreateClient("RaceDayApi").PostAsync("api/auth/logout", null); }
            catch (HttpRequestException) { }

            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        public IActionResult AccessDenied() => View();

        private static string? ExtractApiCookie(HttpResponseMessage response)
        {
            const string prefix = "RaceDay.Auth=";
            if (!response.Headers.TryGetValues("Set-Cookie", out var values)) return null;
            var raw = values.FirstOrDefault(v => v.StartsWith(prefix));
            return raw?.Substring(prefix.Length).Split(';')[0];
        }


        [Authorize, HttpGet]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var p = await _api.GetMyProfileAsync();
                if (p == null) return View("NotFound");
                return View(new ProfileViewModel
                {
                    FullName = p.FullName,
                    Email = p.Email,
                    ProfilePictureUrl = p.ProfilePictureUrl
                });
            }
            catch (HttpRequestException ex) { return await HandleApiErrorAsync(ex); }
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var response = await _api.UpdateMyProfileAsync(model);

                    if (response.IsSuccessStatusCode)
                    {
                        // Refresh the name shown in the menu by re-issuing the sign-in with updated claims
                        var claims = User.Claims.Where(c => c.Type != ClaimTypes.Name).ToList();
                        claims.Add(new Claim(ClaimTypes.Name, model.FullName.Trim()));
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));


                        TempData["Success"] = "Your profile has been updated.";
                        return RedirectToAction(nameof(Profile));
                    }

                    if (response.StatusCode == HttpStatusCode.Conflict)
                        ModelState.AddModelError(nameof(model.Email), "That email address is already used by another account.");
                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                        ModelState.AddModelError(string.Empty, "Your details were not accepted. Please check them and try again.");
                    else
                        response.EnsureSuccessStatusCode();
                }

                model.ProfilePictureUrl = (await _api.GetMyProfileAsync())?.ProfilePictureUrl;
                return View(model);
            }
            catch (HttpRequestException ex) { return await HandleApiErrorAsync(ex); }
        }
    
    [Authorize, HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPicture(IFormFile? picture)
        {
            if (picture == null || picture.Length == 0)
            {
                TempData["Error"] = "Please choose an image to upload.";
                return RedirectToAction(nameof(Profile));
            }
            try
            {
                var response = await _api.UploadProfilePictureAsync(picture);

                if (response.IsSuccessStatusCode)
                    TempData["Success"] = "Profile picture updated.";
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                    TempData["Error"] = await ReadApiMessageAsync(response) ?? "That image wasn't accepted.";
                else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                    TempData["Error"] = "Image storage isn't available right now.";
                else
                    response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex) { return await HandleApiErrorAsync(ex); }

            return RedirectToAction(nameof(Profile));
        }

    }

    public record ApiLoginResult(int UserId, string FullName, string Role);
}