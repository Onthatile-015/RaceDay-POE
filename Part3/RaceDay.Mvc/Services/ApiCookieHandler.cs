namespace RaceDay.Mvc.Services
{
    // Attaches the API's session cookie (kept in this app's session) to every API request.
    public class ApiCookieHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _accessor;
        public ApiCookieHandler(IHttpContextAccessor accessor) => _accessor = accessor;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var cookie = _accessor.HttpContext?.Session.GetString("ApiAuthCookie");
            if (!string.IsNullOrEmpty(cookie))
                request.Headers.Add("Cookie", $"RaceDay.Auth={cookie}");

            return base.SendAsync(request, cancellationToken);
        }
    }
}
