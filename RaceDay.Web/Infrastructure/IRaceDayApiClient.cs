using RaceDay.Web.Models.Api;
namespace RaceDay.Web.Infrastructure;

/// <summary>The only way the MVC app reads or writes data: every call goes to the RaceDay REST API (including image uploads).</summary>
public interface IRaceDayApiClient {
    // Auth & profile
    Task<ApiResult<LoginResponse>> LoginAsync(LoginPayload payload);
    Task<ApiResult> RegisterAsync(RegisterPayload payload);
    Task<ApiResult<ProfileDto>> GetProfileAsync();
    Task<ApiResult<ProfileDto>> UpdateProfileAsync(ProfilePayload payload);
    Task<ApiResult<ProfileDto>> UploadProfilePictureAsync(IFormFile file);

    // Events
    Task<ApiResult<List<EventDto>>> GetEventsAsync();
    Task<ApiResult<EventDto>> GetEventAsync(int id);
    Task<ApiResult<List<EventDto>>> GetMyEventsAsync();
    Task<ApiResult<EventDto>> CreateEventAsync(EventPayload payload);
    Task<ApiResult<EventDto>> UpdateEventAsync(int id, EventPayload payload);
    Task<ApiResult> DeleteEventAsync(int id);
    Task<ApiResult<EventDto>> UploadBannerAsync(int eventId, IFormFile file);

    // Categories
    Task<ApiResult<List<CategoryDto>>> GetCategoriesAsync(int eventId);
    Task<ApiResult<CategoryDto>> CreateCategoryAsync(int eventId, CategoryPayload payload);
    Task<ApiResult> DeleteCategoryAsync(int categoryId);

    // Enrolments
    Task<ApiResult> EnrolAsync(EnrolPayload payload);
    Task<ApiResult<List<MyEnrolmentDto>>> GetMyEnrolmentsAsync();
    Task<ApiResult<List<EventEnrolmentDto>>> GetEventEnrolmentsAsync(int eventId);
    Task<ApiResult> UpdateEnrolmentStatusAsync(int enrolmentId, string status);

    // Results
    Task<ApiResult<List<MyResultDto>>> GetMyResultsAsync();
    Task<ApiResult<List<EventResultDto>>> GetEventResultsAsync(int eventId);
    Task<ApiResult> CreateResultAsync(CreateResultPayload payload);
    Task<ApiResult> UpdateResultAsync(int resultId, UpdateResultPayload payload);
}
