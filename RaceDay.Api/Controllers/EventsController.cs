using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
namespace RaceDay.Api.Controllers;
[ApiController, Route("api/events")]
public class EventsController(RaceDayDbContext db) : ControllerBase {
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private static bool ValidType(string t) => RaceDay.Api.Services.RaceDayRules.IsValidEventType(t);
    private static object View(RaceEvent e) => new { e.EventId, e.OrganiserId, e.Name, e.Description, e.Date, e.Location, e.Distance, e.EventType };
    /// <summary>List all events. Available to both roles.</summary>
    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> GetAll() => Ok((await db.Events.AsNoTracking().OrderBy(x => x.Date).ToListAsync()).Select(View));
    /// <summary>Get one event.</summary>
    [HttpGet("{id:int}"), AllowAnonymous]
    public async Task<IActionResult> Get(int id) { var e = await db.Events.AsNoTracking().SingleOrDefaultAsync(x => x.EventId == id); return e is null ? NotFound() : Ok(View(e)); }
    /// <summary>Create an event. Organiser only.</summary>
    [HttpPost, Authorize(Roles = Roles.Organiser)]
    public async Task<IActionResult> Create(EventRequest request) {
        if (!ValidType(request.EventType)) return BadRequest(new { message = "EventType must be Run, Walk, or Cycle." });
        if (request.Distance <= 0) return BadRequest(new { message = "Distance must be greater than zero." });
        var e = new RaceEvent { OrganiserId = CurrentUserId, Name = request.Name.Trim(), Description = request.Description ?? "", Date = request.Date, Location = request.Location.Trim(), Distance = request.Distance, EventType = request.EventType.Trim().Substring(0,1).ToUpperInvariant() + request.EventType.Trim()[1..].ToLowerInvariant() };
        db.Events.Add(e); await db.SaveChangesAsync(); return CreatedAtAction(nameof(Get), new { id = e.EventId }, View(e));
    }
    /// <summary>Update an event owned by the logged-in organiser.</summary>
    [HttpPut("{id:int}"), Authorize(Roles = Roles.Organiser)]
    public async Task<IActionResult> Update(int id, EventRequest request) {
        var e = await db.Events.FindAsync(id); if (e is null) return NotFound(); if (e.OrganiserId != CurrentUserId) return Forbid();
        if (!ValidType(request.EventType) || request.Distance <= 0) return BadRequest(new { message = "Use event type Run, Walk, or Cycle and a positive distance." });
        e.Name = request.Name.Trim(); e.Description = request.Description ?? ""; e.Date = request.Date; e.Location = request.Location.Trim(); e.Distance = request.Distance; e.EventType = request.EventType.Trim().Substring(0,1).ToUpperInvariant() + request.EventType.Trim()[1..].ToLowerInvariant();
        await db.SaveChangesAsync(); return Ok(View(e));
    }
    /// <summary>Delete an event owned by the logged-in organiser.</summary>
    [HttpDelete("{id:int}"), Authorize(Roles = Roles.Organiser)]
    public async Task<IActionResult> Delete(int id) {
        var e = await db.Events.FindAsync(id); if (e is null) return NotFound(); if (e.OrganiserId != CurrentUserId) return Forbid();
        if (await db.Enrolments.AnyAsync(x => x.EventId == id)) return Conflict(new { message = "Cannot delete an event with enrolments." });
        db.Events.Remove(e); await db.SaveChangesAsync(); return NoContent();
    }
}
