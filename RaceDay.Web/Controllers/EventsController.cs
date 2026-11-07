using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Infrastructure;
using RaceDay.Web.Models.Api;
using RaceDay.Web.Models.ViewModels;

namespace RaceDay.Web.Controllers;

[SessionAuthorize]
public class EventsController(IRaceDayApiClient api) : Controller {
    /// <summary>Full event information and available categories, with the enrol form for participants.</summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id) {
        var ev = await api.GetEventAsync(id);
        if (!ev.Success) {
            if (ev.StatusCode == System.Net.HttpStatusCode.NotFound) return NotFound();
            TempData["Error"] = ev.Error;
            return RedirectToAction("Index", "Home");
        }

        var vm = new EventDetailsViewModel { Event = ev.Data! };
        var categories = await api.GetCategoriesAsync(id);
        if (categories.Success) vm.Categories = categories.Data!; else ViewData["CategoryError"] = categories.Error;

        if (HttpContext.Session.GetUser()!.IsParticipant) {
            var mine = await api.GetMyEnrolmentsAsync();
            if (mine.Success) vm.MyEnrolment = mine.Data!.FirstOrDefault(e => e.Event.EventId == id);
        }
        return View(vm);
    }

    [HttpPost, SessionAuthorize(Roles.Participant)]
    public async Task<IActionResult> Enrol(int eventId, int categoryId) {
        if (categoryId <= 0) {
            TempData["Error"] = "Please choose a category before enrolling.";
            return RedirectToAction(nameof(Details), new { id = eventId });
        }
        var result = await api.EnrolAsync(new EnrolPayload(eventId, categoryId));
        if (!result.Success) {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Details), new { id = eventId });
        }
        TempData["Success"] = "You're enrolled! Your place has been recorded.";
        return RedirectToAction("MyEnrolments", "Participant");
    }
}
