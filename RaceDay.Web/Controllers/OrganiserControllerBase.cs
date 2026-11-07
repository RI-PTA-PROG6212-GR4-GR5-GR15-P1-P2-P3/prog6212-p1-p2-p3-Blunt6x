using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Infrastructure;
using RaceDay.Web.Models.ViewModels;

namespace RaceDay.Web.Controllers;

/// <summary>Base for every organiser-only controller: session role check plus shared "pick an event" plumbing.</summary>
[SessionAuthorize(Roles.Organiser)]
public abstract class OrganiserControllerBase(IRaceDayApiClient api) : Controller {
    protected IRaceDayApiClient Api => api;
    protected UserSession CurrentUser => HttpContext.Session.GetUser()!;

    /// <summary>Loads the organiser's events into the view model and selects <paramref name="eventId"/> if it is one of them. Returns an error message on failure.</summary>
    protected async Task<string?> LoadScopeAsync(EventScopedViewModel vm, int? eventId) {
        var events = await api.GetMyEventsAsync();
        if (!events.Success) return events.Error;
        vm.MyEvents = events.Data!;
        if (eventId.HasValue) vm.Selected = vm.MyEvents.FirstOrDefault(e => e.EventId == eventId.Value);
        return null;
    }
}
