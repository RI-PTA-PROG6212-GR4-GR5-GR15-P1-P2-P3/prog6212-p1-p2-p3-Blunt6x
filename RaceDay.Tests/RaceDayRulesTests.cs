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
    [InlineData(10, 20, true)] [InlineData(null, 20, true)] [InlineData(20, null, true)] [InlineData(20, 10, false)] [InlineData(-1, 10, false)]
    public void AgeRangeMustBeValid(int? minimum, int? maximum, bool expected) => Assert.Equal(expected, RaceDayRules.IsValidAgeRange(minimum, maximum));
    [Fact]
    public void CategoryMustBelongToSelectedEvent() {
        Assert.True(RaceDayRules.CategoryBelongsToEvent(3, 3));
        Assert.False(RaceDayRules.CategoryBelongsToEvent(3, 4));
    }
}
