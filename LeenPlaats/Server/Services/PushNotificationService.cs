using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using WebPush;

namespace Server.Services;

public class PushNotificationService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(AppDbContext db, IConfiguration config, ILogger<PushNotificationService> logger)
    {
        _db = db;
        _config = config;
        _logger = logger;
    }

    public async Task SendToUserAsync(Guid userId, string title, string body)
    {
        var subscriptions = await _db.UserPushSubscriptions
            .Where(s => s.UserId == userId)
            .ToListAsync();

        if (subscriptions.Count == 0) return;

        var client = new WebPushClient();
        var vapidDetails = new VapidDetails(
            _config["Vapid:Subject"]!,
            _config["Vapid:PublicKey"]!,
            _config["Vapid:PrivateKey"]!);
        var payload = JsonSerializer.Serialize(new { title, body });
        var staleIds = new List<Guid>();

        foreach (var sub in subscriptions)
        {
            try
            {
                await client.SendNotificationAsync(
                    new PushSubscription(sub.Endpoint, sub.P256DH, sub.Auth),
                    payload,
                    vapidDetails);
            }
            catch (WebPushException ex) when (ex.StatusCode == HttpStatusCode.Gone)
            {
                staleIds.Add(sub.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Push send failed for endpoint {Endpoint}", sub.Endpoint);
            }
        }

        if (staleIds.Count > 0)
        {
            var stale = await _db.UserPushSubscriptions
                .Where(s => staleIds.Contains(s.Id))
                .ToListAsync();
            _db.UserPushSubscriptions.RemoveRange(stale);
            await _db.SaveChangesAsync();
        }
    }
}
