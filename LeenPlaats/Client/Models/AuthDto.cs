namespace Client.Models;

public record LoginDto
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public record RegisterDto
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string DisplayName { get; set; } = "";
}

public record AuthResponseDto(string Token, string UserId, string DisplayName, string Email);
