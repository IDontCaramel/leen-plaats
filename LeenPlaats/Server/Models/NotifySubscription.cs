namespace Server.Models;

public class NotifySubscription
{
    public Guid Id { get; set; }
    public Guid AdId { get; set; }
    public Ad Ad { get; set; } = null!;
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
}
