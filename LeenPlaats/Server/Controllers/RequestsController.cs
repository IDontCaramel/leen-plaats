using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs;
using Server.Models;

namespace Server.Controllers;

[ApiController]
[Route("api/requests")]
[Authorize]
public class RequestsController : ControllerBase
{
    private readonly AppDbContext _db;

    public RequestsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetMyRequests()
    {
        var userId = GetCurrentUserId();
        var requests = await _db.LendRequests
            .Include(r => r.Ad)
            .Include(r => r.Requester)
            .Where(r => r.RequesterId == userId || r.Ad.OwnerId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => MapToDto(r))
            .ToListAsync();

        return Ok(requests);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRequest(CreateLendRequestDto dto)
    {
        var userId = GetCurrentUserId();

        var ad = await _db.Ads.FirstOrDefaultAsync(a => a.Id == dto.AdId);
        if (ad is null) return NotFound();
        if (ad.OwnerId == userId) return BadRequest("Je kunt niet je eigen advertentie lenen.");

        var alreadyPending = await _db.LendRequests.AnyAsync(
            r => r.AdId == dto.AdId && r.RequesterId == userId && r.Status == LendRequestStatus.Pending);
        if (alreadyPending) return Conflict("Je hebt al een openstaand verzoek voor deze advertentie.");

        var request = new LendRequest
        {
            Id = Guid.NewGuid(),
            AdId = dto.AdId,
            RequesterId = userId,
            Message = dto.Message,
            LendUntil = dto.LendUntil,
            Status = LendRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _db.LendRequests.Add(request);
        await _db.SaveChangesAsync();

        await _db.Entry(request).Reference(r => r.Ad).LoadAsync();
        await _db.Entry(request).Reference(r => r.Requester).LoadAsync();

        return CreatedAtAction(nameof(GetMyRequests), MapToDto(request));
    }

    [HttpPut("{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id) => await UpdateStatus(id, LendRequestStatus.Accepted);

    [HttpPut("{id:guid}/decline")]
    public async Task<IActionResult> Decline(Guid id) => await UpdateStatus(id, LendRequestStatus.Declined);

    private async Task<IActionResult> UpdateStatus(Guid id, LendRequestStatus status)
    {
        var request = await _db.LendRequests
            .Include(r => r.Ad)
            .Include(r => r.Requester)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request is null) return NotFound();
        if (request.Ad.OwnerId != GetCurrentUserId()) return Forbid();
        if (request.Status != LendRequestStatus.Pending) return BadRequest("Verzoek is al verwerkt.");

        request.Status = status;
        await _db.SaveChangesAsync();

        return Ok(MapToDto(request));
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static LendRequestDto MapToDto(LendRequest r) => new(
        r.Id,
        r.AdId,
        r.Ad?.Title ?? string.Empty,
        r.RequesterId,
        r.Requester?.DisplayName ?? string.Empty,
        r.Status,
        r.Message,
        r.CreatedAt,
        r.LendUntil
    );
}
