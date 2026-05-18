using System.ComponentModel.DataAnnotations;
using Server.Models;

namespace Server.DTOs;

public record CreateLendRequestDto(
    [Required] Guid AdId,
    [MaxLength(500)] string? Message,
    DateTime? LendUntil
);

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
