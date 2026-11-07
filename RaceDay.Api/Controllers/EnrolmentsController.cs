using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
namespace RaceDay.Api.Controllers;
[ApiController, Authorize, Route("api")]
public class EnrolmentsController(RaceDayDbContext db) : ControllerBase {
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Enrol the logged-in participant in an event and category. The enrolment is recorded immediately as Confirmed.</summary>
    [HttpPost("enrolments"), Authorize(Roles = Roles.Participant)]
    public async Task<IActionResult> Create(CreateEnrolmentRequest request) {
        var ev = await db.Events.FindAsync(request.EventId); if (ev is null) return NotFound(new { message = "Event not found." });
        var cat = await db.Categories.FindAsync(request.CategoryId); if (cat is null || !RaceDayRules.CategoryBelongsToEvent(cat.EventId, request.EventId)) return BadRequest(new { message = "The selected category does not belong to this event." });
        if (ev.Date.Date < DateTime.UtcNow.Date) return BadRequest(new { message = "You cannot enrol in an event that has already taken place." });
        if (await db.Enrolments.AnyAsync(x => x.ParticipantId == CurrentUserId && x.EventId == request.EventId)) return Conflict(new { message = "You are already enrolled in this event." });
        var e = new Enrolment { ParticipantId = CurrentUserId, EventId = request.EventId, CategoryId = request.CategoryId, EnrolmentDate = DateTime.UtcNow, Status = "Confirmed" };
        db.Enrolments.Add(e); await db.SaveChangesAsync(); return Created($"/api/enrolments/my", new { e.EnrolmentId, e.ParticipantId, e.EventId, e.CategoryId, e.EnrolmentDate, e.Status });
    }

    /// <summary>List the logged-in participant's enrolments.</summary>
    [HttpGet("enrolments/my"), Authorize(Roles = Roles.Participant)]
    public async Task<IActionResult> Mine() => Ok(await db.Enrolments.AsNoTracking().Where(x => x.ParticipantId == CurrentUserId).Include(x => x.Event).Include(x => x.Category).OrderByDescending(x => x.EnrolmentDate).Select(x => new { x.EnrolmentId, x.EnrolmentDate, x.Status, Event = new { x.Event!.EventId, x.Event.Name, x.Event.Date, x.Event.Location, x.Event.Distance, x.Event.EventType, x.Event.BannerUrl }, Category = new { x.Category!.CategoryId, x.Category.Name } }).ToListAsync());

    /// <summary>Organiser views enrolments for an event they manage.</summary>
    [HttpGet("events/{eventId:int}/enrolments"), Authorize(Roles = Roles.Organiser)]
    public async Task<IActionResult> ForEvent(int eventId) {
        if (!await db.Events.AnyAsync(x => x.EventId == eventId && x.OrganiserId == CurrentUserId)) return NotFound(new { message = "Event not found or not managed by this organiser." });
        return Ok(await db.Enrolments.AsNoTracking().Where(x => x.EventId == eventId).Include(x => x.Participant).Include(x => x.Category).OrderBy(x => x.EnrolmentDate).Select(x => new { x.EnrolmentId, x.EnrolmentDate, x.Status, Participant = new { x.Participant!.UserId, x.Participant.FirstName, x.Participant.LastName, x.Participant.Email, x.Participant.ProfilePictureUrl }, Category = new { x.Category!.CategoryId, x.Category.Name } }).ToListAsync());
    }

    /// <summary>Organiser sets the status (Confirmed, Pending or Cancelled) of an enrolment for an event they manage.</summary>
    [HttpPut("enrolments/{id:int}/status"), Authorize(Roles = Roles.Organiser)]
    public async Task<IActionResult> UpdateStatus(int id, UpdateEnrolmentStatusRequest request) {
        if (!RaceDayRules.IsValidEnrolmentStatus(request.Status)) return BadRequest(new { message = "Status must be Confirmed, Pending or Cancelled." });
        var e = await db.Enrolments.Include(x => x.Event).SingleOrDefaultAsync(x => x.EnrolmentId == id);
        if (e is null) return NotFound(new { message = "Enrolment not found." }); if (e.Event!.OrganiserId != CurrentUserId) return Forbid();
        e.Status = RaceDayRules.NormaliseEnrolmentStatus(request.Status); await db.SaveChangesAsync();
        return Ok(new { e.EnrolmentId, e.Status });
    }
}
