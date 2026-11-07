using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.Web.Models.Api;

namespace RaceDay.Web.Infrastructure;

public class RaceDayApiClient(HttpClient http, IHttpContextAccessor accessor, ILogger<RaceDayApiClient> log) : IRaceDayApiClient {
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private UserSession? CurrentUser => accessor.HttpContext?.Session.GetUser();

    // ---- Auth & profile
    public Task<ApiResult<LoginResponse>> LoginAsync(LoginPayload p) => SendAsync<LoginResponse>(HttpMethod.Post, "api/auth/login", p, authenticated: false);
    public Task<ApiResult> RegisterAsync(RegisterPayload p) => SendAsync(HttpMethod.Post, "api/auth/register", p, authenticated: false);
    public Task<ApiResult<ProfileDto>> GetProfileAsync() => SendAsync<ProfileDto>(HttpMethod.Get, "api/profile");
    public Task<ApiResult<ProfileDto>> UpdateProfileAsync(ProfilePayload p) => SendAsync<ProfileDto>(HttpMethod.Put, "api/profile", p);
    public Task<ApiResult<ProfileDto>> UploadProfilePictureAsync(IFormFile file) => UploadAsync<ProfileDto>("api/profile/picture", file);

    // ---- Events
    public Task<ApiResult<List<EventDto>>> GetEventsAsync() => SendAsync<List<EventDto>>(HttpMethod.Get, "api/events", authenticated: false);
    public Task<ApiResult<EventDto>> GetEventAsync(int id) => SendAsync<EventDto>(HttpMethod.Get, $"api/events/{id}", authenticated: false);
    public Task<ApiResult<List<EventDto>>> GetMyEventsAsync() => SendAsync<List<EventDto>>(HttpMethod.Get, "api/events/mine");
    public Task<ApiResult<EventDto>> CreateEventAsync(EventPayload p) => SendAsync<EventDto>(HttpMethod.Post, "api/events", p);
    public Task<ApiResult<EventDto>> UpdateEventAsync(int id, EventPayload p) => SendAsync<EventDto>(HttpMethod.Put, $"api/events/{id}", p);
    public Task<ApiResult> DeleteEventAsync(int id) => SendAsync(HttpMethod.Delete, $"api/events/{id}");
    public Task<ApiResult<EventDto>> UploadBannerAsync(int eventId, IFormFile file) => UploadAsync<EventDto>($"api/events/{eventId}/banner", file);

    // ---- Categories
    public Task<ApiResult<List<CategoryDto>>> GetCategoriesAsync(int eventId) => SendAsync<List<CategoryDto>>(HttpMethod.Get, $"api/events/{eventId}/categories");
    public Task<ApiResult<CategoryDto>> CreateCategoryAsync(int eventId, CategoryPayload p) => SendAsync<CategoryDto>(HttpMethod.Post, $"api/events/{eventId}/categories", p);
    public Task<ApiResult> DeleteCategoryAsync(int categoryId) => SendAsync(HttpMethod.Delete, $"api/categories/{categoryId}");

    // ---- Enrolments
    public Task<ApiResult> EnrolAsync(EnrolPayload p) => SendAsync(HttpMethod.Post, "api/enrolments", p);
    public Task<ApiResult<List<MyEnrolmentDto>>> GetMyEnrolmentsAsync() => SendAsync<List<MyEnrolmentDto>>(HttpMethod.Get, "api/enrolments/my");
    public Task<ApiResult<List<EventEnrolmentDto>>> GetEventEnrolmentsAsync(int eventId) => SendAsync<List<EventEnrolmentDto>>(HttpMethod.Get, $"api/events/{eventId}/enrolments");
    public Task<ApiResult> UpdateEnrolmentStatusAsync(int enrolmentId, string status) => SendAsync(HttpMethod.Put, $"api/enrolments/{enrolmentId}/status", new StatusPayload(status));

    // ---- Results
    public Task<ApiResult<List<MyResultDto>>> GetMyResultsAsync() => SendAsync<List<MyResultDto>>(HttpMethod.Get, "api/results/my");
    public Task<ApiResult<List<EventResultDto>>> GetEventResultsAsync(int eventId) => SendAsync<List<EventResultDto>>(HttpMethod.Get, $"api/events/{eventId}/results");
    public Task<ApiResult> CreateResultAsync(CreateResultPayload p) => SendAsync(HttpMethod.Post, "api/results", p);
    public Task<ApiResult> UpdateResultAsync(int resultId, UpdateResultPayload p) => SendAsync(HttpMethod.Put, $"api/results/{resultId}", p);

    // ---- Plumbing
    private async Task<ApiResult> SendAsync(HttpMethod method, string path, object? body = null, bool authenticated = true) {
        var (response, error) = await ExecuteAsync(() => BuildRequest(method, path, body, authenticated), authenticated);
        if (response is null) return ApiResult.Fail(error!);
        using (response) return response.IsSuccessStatusCode ? ApiResult.Ok(response.StatusCode) : ApiResult.Fail(await ReadErrorAsync(response), response.StatusCode);
    }

    private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string path, object? body = null, bool authenticated = true) {
        var (response, error) = await ExecuteAsync(() => BuildRequest(method, path, body, authenticated), authenticated);
        if (response is null) return ApiResult<T>.Fail(error!);
        using (response) return await ReadAsync<T>(response);
    }

    private async Task<ApiResult<T>> UploadAsync<T>(string path, IFormFile file) {
        var (response, error) = await ExecuteAsync(() => {
            var content = new MultipartFormDataContent();
            var stream = new StreamContent(file.OpenReadStream());
            stream.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
            content.Add(stream, "file", Path.GetFileName(file.FileName));
            return BuildRequest(HttpMethod.Post, path, null, true, content);
        }, authenticated: true);
        if (response is null) return ApiResult<T>.Fail(error!);
        using (response) return await ReadAsync<T>(response);
    }

    private HttpRequestMessage BuildRequest(HttpMethod method, string path, object? body, bool authenticated, HttpContent? content = null) {
        var request = new HttpRequestMessage(method, path);
        if (authenticated && CurrentUser is { } user) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);
        if (content is not null) request.Content = content;
        else if (body is not null) request.Content = JsonContent.Create(body, options: Json);
        return request;
    }

    /// <summary>Sends the request. Network failures become a friendly error; an expired token raises <see cref="SessionExpiredException"/>.</summary>
    private async Task<(HttpResponseMessage? Response, string? Error)> ExecuteAsync(Func<HttpRequestMessage> build, bool authenticated) {
        try {
            var response = await http.SendAsync(build());
            if (authenticated && response.StatusCode == HttpStatusCode.Unauthorized) { response.Dispose(); throw new SessionExpiredException(); }
            return (response, null);
        } catch (HttpRequestException ex) {
            log.LogError(ex, "RaceDay API request failed");
            return (null, "We couldn't reach the RaceDay service. Please try again in a moment.");
        } catch (TaskCanceledException ex) {
            log.LogError(ex, "RaceDay API request timed out");
            return (null, "The RaceDay service took too long to respond. Please try again.");
        }
    }

    private async Task<ApiResult<T>> ReadAsync<T>(HttpResponseMessage response) {
        if (!response.IsSuccessStatusCode) return ApiResult<T>.Fail(await ReadErrorAsync(response), response.StatusCode);
        try {
            var data = await response.Content.ReadFromJsonAsync<T>(Json);
            return data is null ? ApiResult<T>.Fail("The RaceDay service returned an unexpected response.", response.StatusCode) : ApiResult<T>.Ok(data, response.StatusCode);
        } catch (JsonException ex) {
            log.LogError(ex, "Could not parse API response");
            return ApiResult<T>.Fail("The RaceDay service returned an unexpected response.", response.StatusCode);
        }
    }

    /// <summary>Turns API error bodies ({message} or validation problem details) into one friendly sentence. Raw errors never reach the user.</summary>
    private static async Task<string> ReadErrorAsync(HttpResponseMessage response) {
        try {
            var text = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(text)) {
                using var doc = JsonDocument.Parse(text);
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Object) {
                    if (root.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(m.GetString())) return m.GetString()!;
                    if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object) {
                        var first = errors.EnumerateObject().SelectMany(p => p.Value.ValueKind == JsonValueKind.Array ? p.Value.EnumerateArray().Select(v => v.GetString()) : Enumerable.Empty<string?>()).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
                        if (first is not null) return first;
                    }
                }
            }
        } catch (JsonException) { /* fall through to status-based message */ }
        return response.StatusCode switch {
            HttpStatusCode.BadRequest => "Some of the details you entered aren't valid. Please check them and try again.",
            HttpStatusCode.Forbidden => "You don't have permission to do that.",
            HttpStatusCode.NotFound => "We couldn't find what you were looking for.",
            HttpStatusCode.Conflict => "That conflicts with existing information.",
            HttpStatusCode.ServiceUnavailable or HttpStatusCode.BadGateway => "A required service is temporarily unavailable. Please try again later.",
            _ => "Something went wrong while talking to the RaceDay service. Please try again."
        };
    }
}
