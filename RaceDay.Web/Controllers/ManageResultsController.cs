using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Infrastructure;
using RaceDay.Web.Models.Api;
using RaceDay.Web.Models.ViewModels;

namespace RaceDay.Web.Controllers;

public class ManageResultsController(IRaceDayApiClient api) : OrganiserControllerBase(api) {
    /// <summary>Results entry: finish time and position for each enrolled participant. Saved results are visible to participants straight away.</summary>
    [HttpGet]
    public async Task<IActionResult> Index(int? eventId) {
        var vm = new ResultsPageViewModel();
        var error = await LoadScopeAsync(vm, eventId);
        if (error is not null) { ViewData["LoadError"] = error; return View(vm); }
        if (vm.Selected is null) return View(vm);

        vm.EventConcluded = vm.Selected.Date.Date <= DateTime.Today;
        var enrolments = await Api.GetEventEnrolmentsAsync(vm.Selected.EventId);
        var results = await Api.GetEventResultsAsync(vm.Selected.EventId);
        if (!enrolments.Success || !results.Success) { ViewData["LoadError"] = enrolments.Error ?? results.Error; return View(vm); }

        var byEnrolment = results.Data!.ToDictionary(r => r.EnrolmentId);
        vm.Rows = enrolments.Data!
            .Where(e => !e.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) || byEnrolment.ContainsKey(e.EnrolmentId))
            .Select(e => {
                byEnrolment.TryGetValue(e.EnrolmentId, out var r);
                return new ResultRowViewModel {
                    EnrolmentId = e.EnrolmentId, ParticipantName = e.Participant.FullName, CategoryName = e.Category.Name, Status = e.Status,
                    ResultId = r?.ResultId, FinishTime = r is null ? null : UiFormat.Duration(r.FinishTime), Position = r?.Position
                };
            })
            .OrderBy(r => r.Position ?? int.MaxValue).ThenBy(r => r.ParticipantName)
            .ToList();
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Save(SaveResultViewModel model) {
        if (!ModelState.IsValid || !UiFormat.TryParseDuration(model.FinishTime, out var time)) {
            TempData["Error"] = "Enter the finish time as hh:mm:ss (or mm:ss) and a finishing position of 1 or higher.";
            return RedirectToAction(nameof(Index), new { eventId = model.EventId });
        }

        var result = model.ResultId.HasValue
            ? await Api.UpdateResultAsync(model.ResultId.Value, new UpdateResultPayload(time, model.Position!.Value))
            : await Api.CreateResultAsync(new CreateResultPayload(model.EnrolmentId, time, model.Position!.Value));

        if (result.Success) TempData["Success"] = "Result saved and published to the participant."; else TempData["Error"] = result.Error;
        return RedirectToAction(nameof(Index), new { eventId = model.EventId });
    }
}
