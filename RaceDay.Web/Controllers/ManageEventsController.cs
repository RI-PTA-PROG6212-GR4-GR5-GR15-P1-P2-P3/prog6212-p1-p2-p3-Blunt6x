using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Infrastructure;
using RaceDay.Web.Models.Api;
using RaceDay.Web.Models.ViewModels;

namespace RaceDay.Web.Controllers;

public class ManageEventsController(IRaceDayApiClient api) : OrganiserControllerBase(api) {
    [HttpGet]
    public async Task<IActionResult> Index() {
        var result = await Api.GetMyEventsAsync();
        if (!result.Success) { ViewData["LoadError"] = result.Error; return View(new List<EventDto>()); }
        return View(result.Data!.OrderByDescending(e => e.Date).ToList());
    }

    [HttpGet]
    public IActionResult Create() => View("Form", new EventFormViewModel { Date = DateTime.Today.AddDays(30).AddHours(7) });

    [HttpPost]
    public async Task<IActionResult> Create(EventFormViewModel model) {
        if (model.Banner is not null && !ImageUpload.IsValid(model.Banner)) ModelState.AddModelError(nameof(model.Banner), ImageUpload.ErrorMessage);
        if (!ModelState.IsValid) return View("Form", model);

        var created = await Api.CreateEventAsync(ToPayload(model));
        if (!created.Success) {
            ModelState.AddModelError(string.Empty, created.Error ?? "We couldn't create the event.");
            return View("Form", model);
        }

        var eventId = created.Data!.EventId;
        if (model.Banner is { Length: > 0 }) {
            var upload = await Api.UploadBannerAsync(eventId, model.Banner);
            if (!upload.Success) {
                TempData["Warning"] = $"Event created, but the banner couldn't be uploaded: {upload.Error} You can try again below.";
                return RedirectToAction(nameof(Edit), new { id = eventId });
            }
        }
        TempData["Success"] = $"“{created.Data.Name}” has been created. Add categories so participants can enrol.";
        return RedirectToAction("Index", "ManageCategories", new { eventId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id) {
        var ev = await Api.GetEventAsync(id);
        if (!ev.Success) return ev.StatusCode == System.Net.HttpStatusCode.NotFound ? NotFound() : FailTo(ev.Error);
        if (ev.Data!.OrganiserId != CurrentUser.UserId) return RedirectToAction("AccessDenied", "Account");
        return View("Form", ToForm(ev.Data));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, EventFormViewModel model) {
        model.EventId = id;
        if (model.Banner is not null && !ImageUpload.IsValid(model.Banner)) ModelState.AddModelError(nameof(model.Banner), ImageUpload.ErrorMessage);
        if (!ModelState.IsValid) return View("Form", model);

        var updated = await Api.UpdateEventAsync(id, ToPayload(model));
        if (!updated.Success) {
            ModelState.AddModelError(string.Empty, updated.Error ?? "We couldn't save your changes.");
            model.ExistingBannerUrl = (await Api.GetEventAsync(id)).Data?.BannerUrl ?? model.ExistingBannerUrl;
            return View("Form", model);
        }

        if (model.Banner is { Length: > 0 }) {
            var upload = await Api.UploadBannerAsync(id, model.Banner);
            if (!upload.Success) {
                TempData["Warning"] = $"Event details saved, but the banner couldn't be uploaded: {upload.Error}";
                return RedirectToAction(nameof(Edit), new { id });
            }
        }
        TempData["Success"] = "Event updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id) {
        var result = await Api.DeleteEventAsync(id);
        if (result.Success) TempData["Success"] = "Event deleted."; else TempData["Error"] = result.Error;
        return RedirectToAction(nameof(Index));
    }

    private IActionResult FailTo(string? error) { TempData["Error"] = error; return RedirectToAction(nameof(Index)); }

    private static EventPayload ToPayload(EventFormViewModel m) => new(m.Name.Trim(), m.Description?.Trim() ?? "", m.Date!.Value, m.Location.Trim(), m.Distance!.Value, m.EventType);

    private static EventFormViewModel ToForm(EventDto e) => new() {
        EventId = e.EventId, Name = e.Name, Description = e.Description, Date = e.Date, Location = e.Location,
        Distance = e.Distance, EventType = e.EventType, ExistingBannerUrl = e.BannerUrl
    };
}
