namespace Server.DTOs;

public record RegisterPushDto(string Endpoint, string P256DH, string Auth);
public record UnregisterPushDto(string Endpoint);
