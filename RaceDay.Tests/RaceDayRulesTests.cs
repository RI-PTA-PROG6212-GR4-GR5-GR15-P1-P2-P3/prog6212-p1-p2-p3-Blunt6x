using RaceDay.Api.Models;
using RaceDay.Api.Services;
using Xunit;
namespace RaceDay.Tests;
public class RaceDayRulesTests {
    [Theory]
    [InlineData("Organiser", true)] [InlineData("Participant", true)] [InlineData("Admin", false)] [InlineData("", false)]
    public void OnlySupportedRolesAreAccepted(string role, bool expected) => Assert.Equal(expected, RaceDayRules.IsValidRole(role));
    [Theory]
    [InlineData("Run", true)] [InlineData("walk", true)] [InlineData("CYCLE", true)] [InlineData("Swim", false)] [InlineData("", false)]
    public void EventTypeMustBeRunWalkOrCycle(string type, bool expected) => Assert.Equal(expected, RaceDayRules.IsValidEventType(type));
    [Theory]
    [InlineData(10, 20, true)] [InlineData(null, 20, true)] [InlineData(20, null, true)] [InlineData(null, null, true)] [InlineData(20, 10, false)] [InlineData(-1, 10, false)]
    public void AgeRangeMustBeValid(int? minimum, int? maximum, bool expected) => Assert.Equal(expected, RaceDayRules.IsValidAgeRange(minimum, maximum));
    [Fact]
    public void CategoryMustBelongToSelectedEvent() {
        Assert.True(RaceDayRules.CategoryBelongsToEvent(3, 3));
        Assert.False(RaceDayRules.CategoryBelongsToEvent(3, 4));
    }

    [Theory]
    [InlineData("Confirmed", true)] [InlineData("pending", true)] [InlineData("Cancelled", true)] [InlineData("Registered", false)] [InlineData("", false)] [InlineData(null, false)]
    public void EnrolmentStatusMustBeKnown(string? status, bool expected) => Assert.Equal(expected, RaceDayRules.IsValidEnrolmentStatus(status));

    [Fact]
    public void EnrolmentStatusIsNormalisedToCanonicalCasing() => Assert.Equal("Pending", RaceDayRules.NormaliseEnrolmentStatus(" pENDING "));

    [Fact]
    public void ResultsCanOnlyBeCapturedOnOrAfterEventDay() {
        var now = new DateTime(2026, 11, 7, 10, 0, 0, DateTimeKind.Utc);
        Assert.True(RaceDayRules.EventHasConcluded(new DateTime(2026, 11, 6), now));
        Assert.True(RaceDayRules.EventHasConcluded(new DateTime(2026, 11, 7, 18, 0, 0), now));
        Assert.False(RaceDayRules.EventHasConcluded(new DateTime(2026, 11, 8), now));
    }

    [Theory]
    [InlineData("banner.jpg", "image/jpeg", 1000, true)]
    [InlineData("pic.PNG", "image/png", 1000, true)]
    [InlineData("pic.webp", "image/webp", 1000, true)]
    [InlineData("script.exe", "image/png", 1000, false)]
    [InlineData("banner.jpg", "text/html", 1000, false)]
    [InlineData("banner.jpg", "image/jpeg", 0, false)]
    [InlineData("banner.jpg", "image/jpeg", 6 * 1024 * 1024, false)]
    [InlineData(null, "image/jpeg", 1000, false)]
    public void OnlySmallImageFilesAreAccepted(string? name, string? contentType, long length, bool expected) =>
        Assert.Equal(expected, RaceDayRules.IsAllowedImage(name, contentType, length));
}
