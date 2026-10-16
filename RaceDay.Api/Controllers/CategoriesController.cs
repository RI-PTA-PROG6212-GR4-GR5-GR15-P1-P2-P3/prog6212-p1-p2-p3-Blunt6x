using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
namespace RaceDay.Api.Controllers;
[ApiController, Authorize, Route("api")]
public class CategoriesController(RaceDayDbContext db) : ControllerBase {
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private async Task<bool> OwnsEvent(int eventId) => await db.Events.AnyAsync(x => x.EventId == eventId && x.OrganiserId == CurrentUserId);
    /// <summary>List categories for an event. Both authenticated roles may view them.</summary>
    [HttpGet("events/{eventId:int}/categories")]
    public async Task<IActionResult> GetForEvent(int eventId) {
        if (!await db.Events.AnyAsync(x => x.EventId == eventId)) return NotFound(new { message = "Event not found." });
        return Ok(await db.Categories.AsNoTracking().Where(x => x.EventId == eventId).Select(x => new { x.CategoryId, x.EventId, x.Name, x.Description, x.MinimumAge, x.MaximumAge }).ToListAsync());
    }
    /// <summary>Create a category for an event owned by the organiser.</summary>
    [HttpPost("events/{eventId:int}/categories"), Authorize(Roles = Roles.Organiser)]
    public async Task<IActionResult> Create(int eventId, CategoryRequest request) {
        if (!await OwnsEvent(eventId)) return NotFound(new { message = "Event not found or not managed by this organiser." });
        if (!RaceDay.Api.Services.RaceDayRules.IsValidAgeRange(request.MinimumAge, request.MaximumAge)) return BadRequest(new { message = "Age range is invalid." });
        var c = new Category { EventId = eventId, Name = request.Name.Trim(), Description = request.Description, MinimumAge = request.MinimumAge, MaximumAge = request.MaximumAge };
        db.Categories.Add(c); await db.SaveChangesAsync(); return Created($"/api/categories/{c.CategoryId}", new { c.CategoryId, c.EventId, c.Name, c.Description, c.MinimumAge, c.MaximumAge });
    }
    /// <summary>Update a category belonging to an event managed by this organiser.</summary>
    [HttpPut("categories/{id:int}"), Authorize(Roles = Roles.Organiser)]
    public async Task<IActionResult> Update(int id, CategoryRequest request) {
        var c = await db.Categories.Include(x => x.Event).SingleOrDefaultAsync(x => x.CategoryId == id); if (c is null) return NotFound(); if (c.Event!.OrganiserId != CurrentUserId) return Forbid();
        if (!RaceDay.Api.Services.RaceDayRules.IsValidAgeRange(request.MinimumAge, request.MaximumAge)) return BadRequest(new { message = "Age range is invalid." });
        c.Name = request.Name.Trim(); c.Description = request.Description; c.MinimumAge = request.MinimumAge; c.MaximumAge = request.MaximumAge; await db.SaveChangesAsync();
        return Ok(new { c.CategoryId, c.EventId, c.Name, c.Description, c.MinimumAge, c.MaximumAge });
    }
    /// <summary>Delete a category belonging to an event managed by this organiser.</summary>
    [HttpDelete("categories/{id:int}"), Authorize(Roles = Roles.Organiser)]
    public async Task<IActionResult> Delete(int id) {
        var c = await db.Categories.Include(x => x.Event).SingleOrDefaultAsync(x => x.CategoryId == id); if (c is null) return NotFound(); if (c.Event!.OrganiserId != CurrentUserId) return Forbid();
        if (await db.Enrolments.AnyAsync(x => x.CategoryId == id)) return Conflict(new { message = "Cannot delete a category with enrolments." });
        db.Categories.Remove(c); await db.SaveChangesAsync(); return NoContent();
    }
}
