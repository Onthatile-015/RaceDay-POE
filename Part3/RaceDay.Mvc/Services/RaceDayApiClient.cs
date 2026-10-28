using RaceDay.Mvc.Models;
using RaceDay.Mvc.ViewModels;
using System.Net;
using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;

namespace RaceDay.Mvc.Services
{
    public class RaceDayApiClient
    {
        private readonly HttpClient _http;
        public RaceDayApiClient(IHttpClientFactory factory) => _http = factory.CreateClient("RaceDayApi");

        public async Task<List<EventSummary>> GetMyEventsAsync()
            => await _http.GetFromJsonAsync<List<EventSummary>>("api/events/mine") ?? new();



        public async Task<EventDetail?> GetEventAsync(int id)
        {
            var response = await _http.GetAsync($"api/events/{id}");
            if (response.StatusCode == HttpStatusCode.NotFound) return null;
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<EventDetail>();
        }

        public Task<HttpResponseMessage> CreateEventAsync(EventFormViewModel m)
            => _http.PostAsJsonAsync("api/events", ToDto(m));

        public Task<HttpResponseMessage> UpdateEventAsync(int id, EventFormViewModel m)
            => _http.PutAsJsonAsync($"api/events/{id}", ToDto(m));

        public Task<HttpResponseMessage> DeleteEventAsync(int id)
            => _http.DeleteAsync($"api/events/{id}");

        private static object ToDto(EventFormViewModel m) => new
        {
            m.Name,
            m.Description,
            m.EventDate,
            m.DistanceKm,
            m.EventType,
            m.Address,
            m.City,
            m.Status
        };

        public Task<HttpResponseMessage> CreateCategoryAsync(int eventId, CategoryFormViewModel m)
            => _http.PostAsJsonAsync($"api/events/{eventId}/categories", new { m.Name, m.MaxParticipants, m.EntryFee });

        public Task<HttpResponseMessage> UpdateCategoryAsync(int id, CategoryFormViewModel m)
            => _http.PutAsJsonAsync($"api/categories/{id}", new { m.Name, m.MaxParticipants, m.EntryFee });

        public Task<HttpResponseMessage> DeleteCategoryAsync(int id)
            => _http.DeleteAsync($"api/categories/{id}");


        public async Task<List<EventEnrolment>> GetEventEnrolmentsAsync(int eventId)
            => await _http.GetFromJsonAsync<List<EventEnrolment>>($"api/events/{eventId}/enrolments") ?? new();

        public Task<HttpResponseMessage> CaptureResultAsync(int enrolmentId, ResultFormViewModel m)
            => _http.PostAsJsonAsync($"api/enrolments/{enrolmentId}/results", new { m.FinishTime, m.Position });

        public Task<HttpResponseMessage> UpdateResultAsync(int enrolmentId, ResultFormViewModel m)
            => _http.PutAsJsonAsync($"api/enrolments/{enrolmentId}/results", new { m.FinishTime, m.Position });

        public async Task<List<EventSummary>> GetEventsAsync(EventFilterViewModel f)
        {
            var query = new Dictionary<string, string?>();
            if (!string.IsNullOrWhiteSpace(f.Search)) query["search"] = f.Search;
            if (!string.IsNullOrWhiteSpace(f.EventType)) query["eventType"] = f.EventType;
            if (!string.IsNullOrWhiteSpace(f.City)) query["city"] = f.City;
            query["from"] = (f.From ?? DateTime.Today).ToString("yyyy-MM-dd");
            if (f.To.HasValue) query["to"] = f.To.Value.Date.AddDays(1).AddSeconds(-1).ToString("yyyy-MM-ddTHH:mm:ss");
            if (f.MaxDistanceKm.HasValue) query["maxDistanceKm"] = f.MaxDistanceKm.Value.ToString(CultureInfo.InvariantCulture);

            return await _http.GetFromJsonAsync<List<EventSummary>>(QueryHelpers.AddQueryString("api/events", query)) ?? new();
        }

        public Task<HttpResponseMessage> EnrolAsync(int categoryId)
            => _http.PostAsync($"api/categories/{categoryId}/enrolments", null);

        public async Task<List<MyEnrolment>> GetMyEnrolmentsAsync()
            => await _http.GetFromJsonAsync<List<MyEnrolment>>("api/users/me/enrolments") ?? new();

        public async Task<List<MyResult>> GetMyResultsAsync()
            => await _http.GetFromJsonAsync<List<MyResult>>("api/users/me/results") ?? new();

        public Task<HttpResponseMessage> CancelEnrolmentAsync(int enrolmentId)
            => _http.DeleteAsync($"api/enrolments/{enrolmentId}");

        public async Task<ProfileInfo?> GetMyProfileAsync()
            => await _http.GetFromJsonAsync<ProfileInfo>("api/users/me");

        public Task<HttpResponseMessage> UpdateMyProfileAsync(ProfileViewModel m)
            => _http.PutAsJsonAsync("api/users/me", new { m.FullName, m.Email });

        public Task<HttpResponseMessage> UploadBannerAsync(int eventId, IFormFile file)
    => PostFileAsync($"api/events/{eventId}/banner", file);

        public Task<HttpResponseMessage> UploadProfilePictureAsync(IFormFile file)
            => PostFileAsync("api/users/me/picture", file);

        private async Task<HttpResponseMessage> PostFileAsync(string url, IFormFile file)
        {
            using var content = new MultipartFormDataContent();
            await using var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.FileName);
            return await _http.PostAsync(url, content);
        }

    }
}
