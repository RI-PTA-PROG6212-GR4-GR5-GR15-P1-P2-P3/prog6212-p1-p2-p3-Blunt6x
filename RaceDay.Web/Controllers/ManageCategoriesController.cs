using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Infrastructure;
using RaceDay.Web.Models.Api;
using RaceDay.Web.Models.ViewModels;

namespace RaceDay.Web.Controllers;

public class ManageCategoriesController(IRaceDayApiClient api) : OrganiserControllerBase(api) {
    [HttpGet]
    public async Task<IActionResult> Index(int? eventId) => View(await BuildPageAsync(eventId, new CategoryFormViewModel { EventId = eventId ?? 0 }));

    [HttpPost]
    public async Task<IActionResult> Create(CategoryFormViewModel form) {
        if (form.MinimumAge.HasValue && form.MaximumAge.HasValue && form.MinimumAge > form.MaximumAge)
            ModelState.AddModelError(nameof(form.MaximumAge), "Maximum age can't be lower than the minimum age.");
        if (!ModelState.IsValid) return View("Index", await BuildPageAsync(form.EventId, form));

        var result = await Api.CreateCategoryAsync(form.EventId, new CategoryPayload(form.Name.Trim(), form.Description?.Trim(), form.MinimumAge, form.MaximumAge));
        if (!result.Success) {
            ModelState.AddModelError(string.Empty, result.Error ?? "We couldn't add that category.");
            return View("Index", await BuildPageAsync(form.EventId, form));
        }
        TempData["Success"] = $"Category “{result.Data!.Name}” added.";
        return RedirectToAction(nameof(Index), new { eventId = form.EventId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int categoryId, int eventId) {
        var result = await Api.DeleteCategoryAsync(categoryId);
        if (result.Success) TempData["Success"] = "Category removed."; else TempData["Error"] = result.Error;
        return RedirectToAction(nameof(Index), new { eventId });
    }

    private async Task<CategoriesPageViewModel> BuildPageAsync(int? eventId, CategoryFormViewModel form) {
        var vm = new CategoriesPageViewModel { Form = form };
        var error = await LoadScopeAsync(vm, eventId);
        if (error is not null) { ViewData["LoadError"] = error; return vm; }
        if (vm.Selected is null) return vm;

        vm.Form.EventId = vm.Selected.EventId;
        var categories = await Api.GetCategoriesAsync(vm.Selected.EventId);
        if (categories.Success) vm.Categories = categories.Data!; else ViewData["LoadError"] = categories.Error;
        return vm;
    }
}
