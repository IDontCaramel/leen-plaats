using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using Server.Services;

namespace Server.Services;

public class LendExpiryService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LendExpiryService> _logger;

    public LendExpiryService(IServiceScopeFactory scopeFactory, ILogger<LendExpiryService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ExpireOverdueLendsAsync();
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task ExpireOverdueLendsAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var push = scope.ServiceProvider.GetRequiredService<PushNotificationService>();

        var expired = await db.LendRequests
            .Include(r => r.Ad)
                .ThenInclude(a => a.NotifySubscriptions)
            .Where(r =>
                r.Status == LendRequestStatus.Accepted &&
                r.LendUntil != null &&
                r.LendUntil < DateTime.UtcNow &&
                !r.Ad.IsAvailable)
            .ToListAsync();

        foreach (var request in expired)
        {
            request.Ad.IsAvailable = true;
            request.Ad.UpdatedAt = DateTime.UtcNow;

            foreach (var sub in request.Ad.NotifySubscriptions)
            {
                await push.SendToUserAsync(
                    sub.UserId,
                    $"{request.Ad.Title} is weer beschikbaar!",
                    "Stuur nu een leenverzoek.");
            }

            db.NotifySubscriptions.RemoveRange(request.Ad.NotifySubscriptions);
            _logger.LogInformation("Ad {AdId} restored after lend expiry (LendUntil: {LendUntil}).",
                request.AdId, request.LendUntil);
        }

        if (expired.Count > 0)
            await db.SaveChangesAsync();
    }
}
