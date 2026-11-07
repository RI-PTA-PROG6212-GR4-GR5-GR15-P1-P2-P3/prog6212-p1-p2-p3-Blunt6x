using System.Globalization;
namespace RaceDay.Web.Infrastructure;

/// <summary>Presentation-only helpers (formatting and CSS class mapping). No business rules live here.</summary>
public static class UiFormat {
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("en-ZA");

    public static string Ordinal(int n) {
        var suffix = (n % 100) is >= 11 and <= 13 ? "th" : (n % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" };
        return $"{n}{suffix}";
    }

    /// <summary>e.g. "47th out of 312 finishers".</summary>
    public static string PositionOf(int position, int total) => total >= position
        ? $"{Ordinal(position)} out of {total} finisher{(total == 1 ? "" : "s")}"
        : Ordinal(position);

    public static string Duration(TimeSpan t) => $"{(int)t.TotalHours}:{t.Minutes:00}:{t.Seconds:00}";
    public static string Distance(decimal km) => $"{km.ToString("0.##", CultureInfo.InvariantCulture)} km";
    public static string LongDate(DateTime d) => d.ToString("ddd, d MMMM yyyy", Culture);
    public static string ShortDate(DateTime d) => d.ToString("d MMM yyyy", Culture);
    public static string DateAndTime(DateTime d) => d.ToString("d MMM yyyy, HH:mm", Culture);

    public static string StatusClass(string? status) => (status ?? "").ToLowerInvariant() switch {
        "confirmed" => "status-confirmed",
        "pending" => "status-pending",
        "cancelled" => "status-cancelled",
        _ => "status-other"
    };

    public static string EventTypeIcon(string? type) => (type ?? "").ToLowerInvariant() switch { "run" => "🏃", "walk" => "🥾", "cycle" => "🚴", _ => "🏁" };

    public static string AgeRange(int? min, int? max) => (min, max) switch {
        (null, null) => "All ages",
        ({ } a, null) => $"{a}+ years",
        (null, { } b) => $"Up to {b} years",
        ({ } a, { } b) => $"{a}–{b} years"
    };

    public static string Countdown(DateTime date) {
        var days = (date.Date - DateTime.Today).Days;
        return days switch { 0 => "Today", 1 => "Tomorrow", > 1 => $"In {days} days", _ => $"{-days} day{(days == -1 ? "" : "s")} ago" };
    }
}
