using System.Security.Claims;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
namespace RaceDay.Api.Controllers;
[ApiController, Route("api/events")]
public class EventsController(RaceDayDbContext db, IBlobStorageService blobs) : ControllerBase {
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private static bool ValidType(string t) => RaceDayRules.IsValidEventType(t);
    private static string Canonical(string t) { var x = t.Trim(); return x.Substring(0, 1).ToUpperInvariant() + x[1..].ToLowerInvariant(); }
    private static object View(RaceEvent e) => new { e.EventId, e.OrganiserId, e.Name, e.Description, e.Date, e.Location, e.Distance, e.EventType, e.BannerUrl };

    /// <summary>List all events. Available to both roles.</summary>
    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> GetAll() => Ok((await db.Events.AsNoTracking().OrderBy(x => x.Date).ToListAsync()).Select(View));

    /// <summary>List the logged-in organiser's events with enrolment, category and result counts (dashboard summary).</summary>
    [HttpGet("mine"), Authorize(Roles = Roles.Organiser)]
    public async Task<IActionResult> Mine() => Ok(await db.Events.AsNoTracking()
        .Where(x => x.OrganiserId == CurrentUserId).OrderBy(x => x.Date)
        .Select(x => new {
            x.EventId, x.OrganiserId, x.Name, x.Description, x.Date, x.Location, x.Distance, x.EventType, x.BannerUrl,
            EnrolmentCount = x.Enrolments.Count(e => e.Status != "Cancelled"),
            CategoryCount = x.Categories.Count,
            ResultCount = x.Enrolments.Count(e => e.Result != null)
        }).ToListAsync());

    /// <summary>Get one event.</summary>
    [HttpGet("{id:int}"), AllowAnonymous]
    public async Task<IActionResult> Get(int id) { var e = await db.Events.AsNoTracking().SingleOrDefaultAsync(x => x.EventId == id); return e is null ? NotFound() : Ok(View(e)); }

    /// <summary>Create an event. Organiser only.</summary>
    [HttpPost, Authorize(Roles = Roles.Organiser)]
    public async Task<IActionResult> Create(EventRequest request) {
        if (!ValidType(request.EventType)) return BadRequest(new { message = "EventType must be Run, Walk, or Cycle." });
        if (request.Distance <= 0) return BadRequest(new { message = "Distance must be greater than zero." });
        var e = new RaceEvent { OrganiserId = CurrentUserId, Name = request.Name.Trim(), Description = request.Description ?? "", Date = request.Date, Location = request.Location.Trim(), Distance = request.Distance, EventType = Canonical(request.EventType) };
        db.Events.Add(e); await db.SaveChangesAsync(); return CreatedAtAction(nameof(Get), new { id = e.EventId }, View(e));
    }

    /// <summary>Update an event owned by the logged-in organiser.</summary>
    [HttpPut("{id:int}"), Authorize(Roles = Roles.Organiser)]
    public async Task<IActionResult> Update(int id, EventRequest request) {
        var e = await db.Events.FindAsync(id); if (e is null) return NotFound(); if (e.OrganiserId != CurrentUserId) return Forbid();
        if (!ValidType(request.EventType) || request.Distance <= 0) return BadRequest(new { message = "Use event type Run, Walk, or Cycle and a positive distance." });
        e.Name = request.Name.Trim(); e.Description = request.Description ?? ""; e.Date = request.Date; e.Location = request.Location.Trim(); e.Distance = request.Distance; e.EventType = Canonical(request.EventType);
        await db.SaveChangesAsync(); return Ok(View(e));
    }

    /// <summary>Upload or replace the banner image of an event owned by the logged-in organiser (stored in Azure Blob Storage).</summary>
    [HttpPost("{id:int}/banner"), Authorize(Roles = Roles.Organiser), RequestSizeLimit(RaceDayRules.MaxImageBytes + 1024 * 1024)]
    public async Task<IActionResult> UploadBanner(int id, IFormFile file, CancellationToken ct) {
        var e = await db.Events.FindAsync(id); if (e is null) return NotFound(new { message = "Event not found." }); if (e.OrganiserId != CurrentUserId) return Forbid();
        if (file is null || !RaceDayRules.IsAllowedImage(file.FileName, file.ContentType, file.Length)) return BadRequest(new { message = "Upload a JPG, PNG, GIF or WebP image of 5 MB or less." });
        string url;
        try { url = await blobs.UploadImageAsync(file, "banners", ct); }
        catch (BlobStorageNotConfiguredException ex) { return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message }); }
        catch (RequestFailedException) { return StatusCode(StatusCodes.Status502BadGateway, new { message = "The image could not be saved to cloud storage. Please try again." }); }
        var previous = e.BannerUrl;
        e.BannerUrl = url; await db.SaveChangesAsync();
        await blobs.DeleteByUrlAsync(previous, ct);
        return Ok(View(e));
    }

    /// <summary>Delete an event owned by the logged-in organiser.</summary>
    [HttpDelete("{id:int}"), Authorize(Roles = Roles.Organiser)]
    public async Task<IActionResult> Delete(int id) {
        var e = await db.Events.FindAsync(id); if (e is null) return NotFound(); if (e.OrganiserId != CurrentUserId) return Forbid();
        if (await db.Enrolments.AnyAsync(x => x.EventId == id)) return Conflict(new { message = "Cannot delete an event with enrolments." });
        var banner = e.BannerUrl;
        db.Events.Remove(e); await db.SaveChangesAsync();
        await blobs.DeleteByUrlAsync(banner);
        return NoContent();
    }
}
