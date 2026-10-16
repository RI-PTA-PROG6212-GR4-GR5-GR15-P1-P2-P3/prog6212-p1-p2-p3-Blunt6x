using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
namespace RaceDay.Api.Controllers;
[ApiController, Authorize, Route("api")]
public class ResultsController(RaceDayDbContext db) : ControllerBase {
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    /// <summary>Record a result for an enrolment in an event managed by the organiser.</summary>
    [HttpPost("results"), Authorize(Roles = Roles.Organiser)]
    public async Task<IActionResult> Create(CreateResultRequest request) {
        if (request.FinishTime < TimeSpan.Zero || request.Position < 1) return BadRequest(new { message = "Finish time must be non-negative and position must be at least 1." });
        var enrolment = await db.Enrolments.Include(x => x.Event).SingleOrDefaultAsync(x => x.EnrolmentId == request.EnrolmentId);
        if (enrolment is null) return NotFound(new { message = "Enrolment not found." }); if (enrolment.Event!.OrganiserId != CurrentUserId) return Forbid();
        if (await db.Results.AnyAsync(x => x.EnrolmentId == request.EnrolmentId)) return Conflict(new { message = "A result already exists for this enrolment. Use PUT to update it." });
        var result = new RaceResult { EnrolmentId = request.EnrolmentId, FinishTime = request.FinishTime, Position = request.Position, RecordedAt = DateTime.UtcNow };
        db.Results.Add(result); await db.SaveChangesAsync(); return Created($"/api/results/my", new { result.ResultId, result.EnrolmentId, result.FinishTime, result.Position, result.RecordedAt });
    }
    /// <summary>Update a result for an event managed by the organiser.</summary>
    [HttpPut("results/{id:int}"), Authorize(Roles = Roles.Organiser)]
    public async Task<IActionResult> Update(int id, UpdateResultRequest request) {
        var result = await db.Results.Include(x => x.Enrolment!).ThenInclude(x => x.Event).SingleOrDefaultAsync(x => x.ResultId == id); if (result is null) return NotFound();
        if (result.Enrolment!.Event!.OrganiserId != CurrentUserId) return Forbid(); if (request.FinishTime < TimeSpan.Zero || request.Position < 1) return BadRequest(new { message = "Finish time must be non-negative and position must be at least 1." });
        result.FinishTime = request.FinishTime; result.Position = request.Position; result.RecordedAt = DateTime.UtcNow; await db.SaveChangesAsync(); return Ok(new { result.ResultId, result.EnrolmentId, result.FinishTime, result.Position, result.RecordedAt });
    }
    /// <summary>Participants can view their own results only.</summary>
    [HttpGet("results/my"), Authorize(Roles = Roles.Participant)]
    public async Task<IActionResult> Mine() => Ok(await db.Results.AsNoTracking().Include(x => x.Enrolment!).ThenInclude(x => x.Event).Include(x => x.Enrolment!).ThenInclude(x => x.Category).Where(x => x.Enrolment!.ParticipantId == CurrentUserId).OrderBy(x => x.Position).Select(x => new { x.ResultId, x.FinishTime, x.Position, x.RecordedAt, Event = new { x.Enrolment!.Event!.EventId, x.Enrolment.Event.Name, x.Enrolment.Event.Date }, Category = new { x.Enrolment!.Category!.CategoryId, x.Enrolment.Category.Name } }).ToListAsync());
    /// <summary>Organiser views results for an event they manage.</summary>
    [HttpGet("events/{eventId:int}/results"), Authorize(Roles = Roles.Organiser)]
    public async Task<IActionResult> ForEvent(int eventId) {
        if (!await db.Events.AnyAsync(x => x.EventId == eventId && x.OrganiserId == CurrentUserId)) return NotFound(new { message = "Event not found or not managed by this organiser." });
        return Ok(await db.Results.AsNoTracking().Include(x => x.Enrolment!).ThenInclude(x => x.Participant).Include(x => x.Enrolment!).ThenInclude(x => x.Category).Where(x => x.Enrolment!.EventId == eventId).OrderBy(x => x.Position).Select(x => new { x.ResultId, x.FinishTime, x.Position, Participant = new { x.Enrolment!.Participant!.UserId, x.Enrolment.Participant.FirstName, x.Enrolment.Participant.LastName }, Category = new { x.Enrolment!.Category!.CategoryId, x.Enrolment.Category.Name } }).ToListAsync());
    }
}
