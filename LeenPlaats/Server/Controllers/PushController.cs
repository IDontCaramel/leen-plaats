using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs;
using Server.Models;

namespace Server.Controllers;

[ApiController]
[Route("api/push")]
public class PushController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public PushController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpGet("vapid-public-key")]
    public IActionResult GetVapidPublicKey()
    {
        var key = _config["Vapid:PublicKey"]
            ?? throw new InvalidOperationException("Vapid:PublicKey not configured.");
        return Ok(key);
    }

    [HttpPost("subscribe")]
    [Authorize]
    public async Task<IActionResult> Subscribe(RegisterPushDto dto)
    {
        var userId = GetCurrentUserId();
        var exists = await _db.UserPushSubscriptions
            .AnyAsync(s => s.UserId == userId && s.Endpoint == dto.Endpoint);

        if (!exists)
        {
            _db.UserPushSubscriptions.Add(new UserPushSubscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Endpoint = dto.Endpoint,
                P256DH = dto.P256DH,
                Auth = dto.Auth,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        return Ok();
    }

    [HttpDelete("unsubscribe")]
    [Authorize]
    public async Task<IActionResult> Unsubscribe(UnregisterPushDto dto)
    {
        var userId = GetCurrentUserId();
        var sub = await _db.UserPushSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Endpoint == dto.Endpoint);

        if (sub is not null)
        {
            _db.UserPushSubscriptions.Remove(sub);
            await _db.SaveChangesAsync();
        }

        return NoContent();
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
