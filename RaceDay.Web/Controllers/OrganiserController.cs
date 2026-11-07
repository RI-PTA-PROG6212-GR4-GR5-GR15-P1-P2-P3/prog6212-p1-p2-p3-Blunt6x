using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Infrastructure;
using RaceDay.Web.Models.ViewModels;

namespace RaceDay.Web.Controllers;

public class OrganiserController(IRaceDayApiClient api) : OrganiserControllerBase(api) {
    /// <summary>Summary of the events the organiser created, enrolments per event and upcoming dates.</summary>
    [HttpGet]
    public async Task<IActionResult> Dashboard() {
        var vm = new DashboardViewModel { OrganiserName = CurrentUser.FirstName };
        var result = await Api.GetMyEventsAsync();
        if (!result.Success) ViewData["LoadError"] = result.Error; else vm.Events = result.Data!;
        return View(vm);
    }
}
