using System.ComponentModel.DataAnnotations;

namespace Server.DTOs;

public record RegisterDto(
    [Required][EmailAddress] string Email,
    [Required][MinLength(8)] string Password,
    [Required][MaxLength(100)] string DisplayName
);

public record LoginDto(
    [Required] string Email,
    [Required] string Password
);

public record AuthResponseDto(string Token, string UserId, string DisplayName, string Email);
