using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Infrastructure;
using RaceDay.Web.Models.ViewModels;

namespace RaceDay.Web.Controllers;

public class ManageEnrolmentsController(IRaceDayApiClient api) : OrganiserControllerBase(api) {
    /// <summary>Everyone enrolled for one event, with their category and enrolment status.</summary>
    [HttpGet]
    public async Task<IActionResult> Index(int? eventId, string? status) {
        var vm = new EnrolmentsPageViewModel { StatusFilter = status };
        var error = await LoadScopeAsync(vm, eventId);
        if (error is not null) { ViewData["LoadError"] = error; return View(vm); }
        if (vm.Selected is null) return View(vm);

        var enrolments = await Api.GetEventEnrolmentsAsync(vm.Selected.EventId);
        if (!enrolments.Success) { ViewData["LoadError"] = enrolments.Error; return View(vm); }
        vm.Enrolments = enrolments.Data!;
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int enrolmentId, int eventId, string status) {
        var result = await Api.UpdateEnrolmentStatusAsync(enrolmentId, status);
        if (result.Success) TempData["Success"] = $"Enrolment marked as {status}."; else TempData["Error"] = result.Error;
        return RedirectToAction(nameof(Index), new { eventId });
    }
}
