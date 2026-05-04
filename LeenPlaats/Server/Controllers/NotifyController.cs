using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Controllers;

[ApiController]
[Route("api/notify")]
[Authorize]
public class NotifyController : ControllerBase
{
    private readonly AppDbContext _db;

    public NotifyController(AppDbContext db) => _db = db;

    [HttpPost("{adId:guid}")]
    public async Task<IActionResult> Subscribe(Guid adId)
    {
        var userId = GetCurrentUserId();

        if (!await _db.Ads.AnyAsync(a => a.Id == adId))
            return NotFound();

        if (await _db.NotifySubscriptions.AnyAsync(n => n.AdId == adId && n.UserId == userId))
            return Conflict("Je ontvangt al meldingen voor deze advertentie.");

        _db.NotifySubscriptions.Add(new NotifySubscription
        {
            Id = Guid.NewGuid(),
            AdId = adId,
            UserId = userId
        });

        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{adId:guid}")]
    public async Task<IActionResult> Unsubscribe(Guid adId)
    {
        var userId = GetCurrentUserId();

        var subscription = await _db.NotifySubscriptions
            .FirstOrDefaultAsync(n => n.AdId == adId && n.UserId == userId);

        if (subscription is null) return NotFound();

        _db.NotifySubscriptions.Remove(subscription);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
