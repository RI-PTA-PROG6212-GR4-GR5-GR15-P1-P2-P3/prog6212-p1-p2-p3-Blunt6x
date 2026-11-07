using RaceDay.Api.Models;
namespace RaceDay.Api.Services;
public static class RaceDayRules {
    public const long MaxImageBytes = 5 * 1024 * 1024;
    private static readonly string[] EnrolmentStatuses = { "Confirmed", "Pending", "Cancelled" };
    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private static readonly string[] ImageContentTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };

    public static bool IsValidRole(string? role) => role is Roles.Organiser or Roles.Participant;
    public static bool IsValidEventType(string? type) => type is not null && new[] { "Run", "Walk", "Cycle" }.Contains(type, StringComparer.OrdinalIgnoreCase);
    // A missing minimum or maximum is allowed (open-ended category); supplied values must be >= 0 and ordered.
    public static bool IsValidAgeRange(int? minimum, int? maximum) =>
        (!minimum.HasValue || minimum >= 0) && (!maximum.HasValue || maximum >= 0) && (!minimum.HasValue || !maximum.HasValue || minimum <= maximum);
    public static bool CategoryBelongsToEvent(int categoryEventId, int requestedEventId) => categoryEventId == requestedEventId;

    public static bool IsValidEnrolmentStatus(string? status) => status is not null && EnrolmentStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
    public static string NormaliseEnrolmentStatus(string status) => EnrolmentStatuses.First(s => s.Equals(status.Trim(), StringComparison.OrdinalIgnoreCase));

    // Results may be captured on or after the day of the event.
    public static bool EventHasConcluded(DateTime eventDate, DateTime utcNow) => eventDate.Date <= utcNow.Date;

    public static bool IsAllowedImage(string? fileName, string? contentType, long length) =>
        length > 0 && length <= MaxImageBytes
        && !string.IsNullOrWhiteSpace(fileName) && ImageExtensions.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase)
        && !string.IsNullOrWhiteSpace(contentType) && ImageContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase);
}
