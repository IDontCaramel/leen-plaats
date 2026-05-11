using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs;
using Server.Models;
using Server.Services;

namespace Server.Controllers;

[ApiController]
[Route("api/ads")]
public class AdsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IFileStorageService _storage;

    public AdsController(AppDbContext db, IFileStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    [HttpGet]
    public async Task<IActionResult> GetAds(
        [FromQuery] string? q,
        [FromQuery] string? category,
        [FromQuery] string? sort = "newest")
    {
        var query = _db.Ads
            .Include(a => a.Owner)
            .Include(a => a.Photos)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(a => a.Title.Contains(q) || a.Description.Contains(q));

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(a => a.Category == category);

        query = sort == "oldest"
            ? query.OrderBy(a => a.CreatedAt)
            : query.OrderByDescending(a => a.CreatedAt);

        var ads = await query.ToListAsync();
        return Ok(ads.Select(a => MapToDto(a, _storage, Request)));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAd(Guid id)
    {
        var ad = await _db.Ads
            .Include(a => a.Owner)
            .Include(a => a.Photos)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (ad is null) return NotFound();
        return Ok(MapToDto(ad, _storage, Request));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateAd(CreateAdDto dto)
    {
        var ad = new Ad
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            OwnerId = GetCurrentUserId(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Ads.Add(ad);
        await _db.SaveChangesAsync();

        await _db.Entry(ad).Reference(a => a.Owner).LoadAsync();

        return CreatedAtAction(nameof(GetAd), new { id = ad.Id }, MapToDto(ad, _storage, Request));
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateAd(Guid id, UpdateAdDto dto)
    {
        var ad = await _db.Ads.Include(a => a.Owner).Include(a => a.Photos).FirstOrDefaultAsync(a => a.Id == id);
        if (ad is null) return NotFound();
        if (ad.OwnerId != GetCurrentUserId()) return Forbid();

        ad.Title = dto.Title;
        ad.Description = dto.Description;
        ad.Category = dto.Category;
        ad.Latitude = dto.Latitude;
        ad.Longitude = dto.Longitude;
        ad.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(MapToDto(ad, _storage, Request));
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteAd(Guid id)
    {
        var ad = await _db.Ads.Include(a => a.Photos).FirstOrDefaultAsync(a => a.Id == id);
        if (ad is null) return NotFound();
        if (ad.OwnerId != GetCurrentUserId()) return Forbid();

        foreach (var photo in ad.Photos)
            _storage.Delete(photo.FileName);

        _db.Ads.Remove(ad);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/photos")]
    [Authorize]
    public async Task<IActionResult> UploadPhoto(Guid id, IFormFile file)
    {
        var ad = await _db.Ads.Include(a => a.Photos).FirstOrDefaultAsync(a => a.Id == id);
        if (ad is null) return NotFound();
        if (ad.OwnerId != GetCurrentUserId()) return Forbid();

        if (file.Length == 0) return BadRequest("Geen bestand geselecteerd.");
        if (!file.ContentType.StartsWith("image/")) return BadRequest("Alleen afbeeldingen zijn toegestaan.");

        var fileName = await _storage.SaveAsync(file);
        var photo = new Photo
        {
            Id = Guid.NewGuid(),
            AdId = id,
            FileName = fileName,
            ContentType = file.ContentType
        };

        _db.Photos.Add(photo);
        await _db.SaveChangesAsync();

        return Ok(new PhotoDto(photo.Id, _storage.GetUrl(Request, photo.FileName)));
    }

    [HttpGet("{id:guid}/requests")]
    [Authorize]
    public async Task<IActionResult> GetRequestsForAd(Guid id)
    {
        var ad = await _db.Ads.FirstOrDefaultAsync(a => a.Id == id);
        if (ad is null) return NotFound();
        if (ad.OwnerId != GetCurrentUserId()) return Forbid();

        var requests = await _db.LendRequests
            .Include(r => r.Requester)
            .Include(r => r.Ad)
            .Where(r => r.AdId == id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(requests.Select(r => new LendRequestDto(
            r.Id, r.AdId, r.Ad?.Title ?? string.Empty,
            r.RequesterId, r.Requester?.DisplayName ?? string.Empty,
            r.Status, r.Message, r.CreatedAt)));
    }

    [HttpDelete("{adId:guid}/photos/{photoId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeletePhoto(Guid adId, Guid photoId)
    {
        var ad = await _db.Ads.FirstOrDefaultAsync(a => a.Id == adId);
        if (ad is null) return NotFound();
        if (ad.OwnerId != GetCurrentUserId()) return Forbid();

        var photo = await _db.Photos.FirstOrDefaultAsync(p => p.Id == photoId && p.AdId == adId);
        if (photo is null) return NotFound();

        _storage.Delete(photo.FileName);
        _db.Photos.Remove(photo);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static AdDto MapToDto(Ad ad, IFileStorageService storage, HttpRequest request) => new(
        ad.Id,
        ad.Title,
        ad.Description,
        ad.Category,
        ad.OwnerId,
        ad.Owner?.DisplayName ?? string.Empty,
        ad.Latitude,
        ad.Longitude,
        ad.CreatedAt,
        ad.UpdatedAt,
        ad.Photos.Select(p => new PhotoDto(p.Id, storage.GetUrl(request, p.FileName))).ToList()
    );
}
