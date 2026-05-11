using System.ComponentModel.DataAnnotations;

namespace Server.DTOs;

public record PhotoDto(Guid Id, string Url);

public record AdDto(
    Guid Id,
    string Title,
    string Description,
    string Category,
    Guid OwnerId,
    string OwnerDisplayName,
    bool IsAvailable,
    double? Latitude,
    double? Longitude,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<PhotoDto> Photos
);

public record CreateAdDto(
    [Required][MaxLength(200)] string Title,
    [Required][MaxLength(2000)] string Description,
    [Required][MaxLength(100)] string Category,
    double? Latitude,
    double? Longitude
);

public record SetAvailabilityDto(bool IsAvailable);

public record UpdateAdDto(
    [Required][MaxLength(200)] string Title,
    [Required][MaxLength(2000)] string Description,
    [Required][MaxLength(100)] string Category,
    double? Latitude,
    double? Longitude
);
