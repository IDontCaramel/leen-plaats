namespace Client.Models;

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
    string Title,
    string Description,
    string Category,
    double? Latitude,
    double? Longitude
);
