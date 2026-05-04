namespace Server.Models;

public class Photo
{
    public Guid Id { get; set; }
    public Guid AdId { get; set; }
    public Ad Ad { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}
