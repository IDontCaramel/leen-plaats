using Microsoft.AspNetCore.Identity;

namespace Server.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public ICollection<Ad> Ads { get; set; } = [];
    public ICollection<LendRequest> LendRequests { get; set; } = [];
    public ICollection<NotifySubscription> NotifySubscriptions { get; set; } = [];
    public ICollection<UserPushSubscription> UserPushSubscriptions { get; set; } = [];
}
