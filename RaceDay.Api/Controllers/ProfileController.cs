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
[ApiController, Authorize, Route("api/profile")]
public class ProfileController(RaceDayDbContext db, IBlobStorageService blobs) : ControllerBase {
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private static object View(UserAccount u) => new { u.UserId, u.FirstName, u.LastName, u.Email, u.Role, u.ProfilePictureUrl };

    /// <summary>Get the authenticated user's own profile.</summary>
    [HttpGet]
    public async Task<IActionResult> Get() { var u = await db.Users.FindAsync(CurrentUserId); return u is null ? NotFound() : Ok(View(u)); }

    /// <summary>Update the authenticated user's own profile.</summary>
    [HttpPut]
    public async Task<IActionResult> Update(UpdateProfileRequest request) {
        var u = await db.Users.FindAsync(CurrentUserId); if (u is null) return NotFound();
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email && x.UserId != u.UserId)) return Conflict(new { message = "Email is already in use." });
        u.FirstName = request.FirstName.Trim(); u.LastName = request.LastName.Trim(); u.Email = email;
        await db.SaveChangesAsync(); return Ok(View(u));
    }

    /// <summary>Upload or replace the authenticated user's profile picture (stored in Azure Blob Storage).</summary>
    [HttpPost("picture"), RequestSizeLimit(RaceDayRules.MaxImageBytes + 1024 * 1024)]
    public async Task<IActionResult> UploadPicture(IFormFile file, CancellationToken ct) {
        var u = await db.Users.FindAsync(CurrentUserId); if (u is null) return NotFound();
        if (file is null || !RaceDayRules.IsAllowedImage(file.FileName, file.ContentType, file.Length)) return BadRequest(new { message = "Upload a JPG, PNG, GIF or WebP image of 5 MB or less." });
        string url;
        try { url = await blobs.UploadImageAsync(file, "profiles", ct); }
        catch (BlobStorageNotConfiguredException ex) { return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message }); }
        catch (RequestFailedException) { return StatusCode(StatusCodes.Status502BadGateway, new { message = "The image could not be saved to cloud storage. Please try again." }); }
        var previous = u.ProfilePictureUrl;
        u.ProfilePictureUrl = url; await db.SaveChangesAsync();
        await blobs.DeleteByUrlAsync(previous, ct);
        return Ok(View(u));
    }
}
