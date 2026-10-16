using RaceDay.Api.Models;
namespace RaceDay.Api.Services;
public static class RaceDayRules {
    public static bool IsValidRole(string? role) => role is Roles.Organiser or Roles.Participant;
    public static bool IsValidEventType(string? type) => type is not null && new[] { "Run", "Walk", "Cycle" }.Contains(type, StringComparer.OrdinalIgnoreCase);
    public static bool IsValidAgeRange(int? minimum, int? maximum) => minimum >= 0 && maximum >= 0 && (!minimum.HasValue || !maximum.HasValue || minimum <= maximum);
    public static bool CategoryBelongsToEvent(int categoryEventId, int requestedEventId) => categoryEventId == requestedEventId;
}
