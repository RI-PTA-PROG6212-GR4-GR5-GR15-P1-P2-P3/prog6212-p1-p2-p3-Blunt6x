using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Infrastructure;
using RaceDay.Web.Models.ViewModels;

namespace RaceDay.Web.Controllers;

public class HomeController(IRaceDayApiClient api) : Controller {
    /// <summary>Public landing page: upcoming events with filters. Organisers are sent to their dashboard.</summary>
    public async Task<IActionResult> Index(string? search, string? type, decimal? minDistance, decimal? maxDistance, DateTime? dateFrom, DateTime? dateTo, string sort = "date", bool includePast = false) {
        var user = HttpContext.Session.GetUser();
        if (user?.IsOrganiser == true) return RedirectToAction("Dashboard", "Organiser");

        var vm = new HomeViewModel { Search = search?.Trim(), Type = type, MinDistance = minDistance, MaxDistance = maxDistance, From = dateFrom, To = dateTo, Sort = sort, IncludePast = includePast };
        var result = await api.GetEventsAsync();
        if (!result.Success) {
            ViewData["LoadError"] = result.Error;
            return View(vm);
        }

        var all = result.Data!;
        vm.TotalUpcoming = all.Count(e => e.IsUpcoming);

        IEnumerable<Models.Api.EventDto> q = all;
        if (!includePast) q = q.Where(e => e.IsUpcoming);
        if (!string.IsNullOrWhiteSpace(vm.Search)) q = q.Where(e => e.Name.Contains(vm.Search, StringComparison.OrdinalIgnoreCase) || e.Location.Contains(vm.Search, StringComparison.OrdinalIgnoreCase) || e.Description.Contains(vm.Search, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(type)) q = q.Where(e => e.EventType.Equals(type, StringComparison.OrdinalIgnoreCase));
        if (minDistance.HasValue) q = q.Where(e => e.Distance >= minDistance.Value);
        if (maxDistance.HasValue) q = q.Where(e => e.Distance <= maxDistance.Value);
        if (dateFrom.HasValue) q = q.Where(e => e.Date.Date >= dateFrom.Value.Date);
        if (dateTo.HasValue) q = q.Where(e => e.Date.Date <= dateTo.Value.Date);
        q = sort switch {
            "distance" => q.OrderBy(e => e.Distance).ThenBy(e => e.Date),
            "name" => q.OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase),
            _ => includePast ? q.OrderByDescending(e => e.Date) : q.OrderBy(e => e.Date)
        };
        vm.Events = q.ToList();

        if (user?.IsParticipant == true) {
            var mine = await api.GetMyEnrolmentsAsync();
            if (mine.Success) vm.EnrolledEventIds = mine.Data!.Where(e => !e.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)).Select(e => e.Event.EventId).ToHashSet();
        }
        return View(vm);
    }

    [IgnoreAntiforgeryToken, ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return View("~/Views/Shared/Error.cshtml");
    }

    /// <summary>Friendly pages for 403/404 etc. (status code re-execution).</summary>
    [IgnoreAntiforgeryToken, Route("Home/Status")]
    public IActionResult Status(int code) {
        (string title, string message) = code switch {
            404 => ("Page not found", "The page you're looking for has been moved, deleted, or never existed."),
            403 => ("Access denied", "You don't have permission to view that page."),
            400 => ("Bad request", "We couldn't understand that request."),
            _ => ("Something went wrong", "We hit an unexpected problem. Please try again.")
        };
        ViewData["ErrorCode"] = code.ToString();
        ViewData["ErrorTitle"] = title;
        ViewData["ErrorMessage"] = message;
        return View("~/Views/Shared/Error.cshtml");
    }
}
