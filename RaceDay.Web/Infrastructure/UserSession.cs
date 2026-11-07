using System.Text.Json;
namespace RaceDay.Web.Infrastructure;

/// <summary>What the MVC app remembers about the signed-in user for the lifetime of their session.</summary>
public record UserSession(string Token, int UserId, string FirstName, string LastName, string Email, string Role, string? ProfilePictureUrl) {
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Initials => $"{(FirstName.Length > 0 ? FirstName[0] : '?')}{(LastName.Length > 0 ? LastName[0] : ' ')}".Trim().ToUpperInvariant();
    public bool IsOrganiser => Role == Roles.Organiser;
    public bool IsParticipant => Role == Roles.Participant;
}

public static class Roles {
    public const string Organiser = "Organiser";
    public const string Participant = "Participant";
}

public static class SessionExtensions {
    private const string Key = "RaceDay.User";
    public static UserSession? GetUser(this ISession session) {
        var json = session.GetString(Key);
        if (string.IsNullOrEmpty(json)) return null;
        try { return JsonSerializer.Deserialize<UserSession>(json); } catch (JsonException) { return null; }
    }
    public static void SetUser(this ISession session, UserSession user) => session.SetString(Key, JsonSerializer.Serialize(user));
}
