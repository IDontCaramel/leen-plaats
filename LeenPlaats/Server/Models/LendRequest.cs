namespace Server.Models;

public enum LendRequestStatus { Pending, Accepted, Declined }

public class LendRequest
{
    public Guid Id { get; set; }
    public Guid AdId { get; set; }
    public Ad Ad { get; set; } = null!;
    public Guid RequesterId { get; set; }
    public ApplicationUser Requester { get; set; } = null!;
    public LendRequestStatus Status { get; set; } = LendRequestStatus.Pending;
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
}
