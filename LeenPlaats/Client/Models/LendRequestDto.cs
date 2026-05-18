namespace Client.Models;

public enum LendRequestStatus { Pending, Accepted, Declined }

public record LendRequestDto(
    Guid Id,
    Guid AdId,
    string AdTitle,
    Guid RequesterId,
    string RequesterDisplayName,
    LendRequestStatus Status,
    string? Message,
    DateTime CreatedAt,
    DateTime? LendUntil
);

public record CreateLendRequestDto(Guid AdId, string? Message, DateTime? LendUntil);
