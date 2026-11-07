using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Infrastructure;

namespace RaceDay.Web.Controllers;

[SessionAuthorize(Roles.Participant)]
public class ParticipantController(IRaceDayApiClient api) : Controller {
    /// <summary>All events the participant has enrolled for, with colour-coded status.</summary>
    [HttpGet]
    public async Task<IActionResult> MyEnrolments() {
        var result = await api.GetMyEnrolmentsAsync();
        if (!result.Success) { ViewData["LoadError"] = result.Error; return View(new List<Models.Api.MyEnrolmentDto>()); }
        return View(result.Data!.OrderBy(e => e.Event.Date).ToList());
    }

    /// <summary>Personal race history across completed events (results captured by organisers).</summary>
    [HttpGet]
    public async Task<IActionResult> MyResults() {
        var result = await api.GetMyResultsAsync();
        if (!result.Success) { ViewData["LoadError"] = result.Error; return View(new List<Models.Api.MyResultDto>()); }
        return View(result.Data!.OrderByDescending(r => r.Event.Date).ToList());
    }
}
