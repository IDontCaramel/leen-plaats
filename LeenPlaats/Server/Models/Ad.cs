namespace Server.Models;

public class Ad
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public ApplicationUser Owner { get; set; } = null!;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Photo> Photos { get; set; } = [];
    public ICollection<LendRequest> LendRequests { get; set; } = [];
    public ICollection<NotifySubscription> NotifySubscriptions { get; set; } = [];
}
