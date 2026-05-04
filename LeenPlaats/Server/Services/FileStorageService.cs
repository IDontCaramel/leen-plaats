namespace Server.Services;

public interface IFileStorageService
{
    Task<string> SaveAsync(IFormFile file);
    void Delete(string fileName);
    string GetUrl(HttpRequest request, string fileName);
}

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadPath;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        _uploadPath = Path.Combine(env.WebRootPath, "uploads");
        Directory.CreateDirectory(_uploadPath);
    }

    public async Task<string> SaveAsync(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(_uploadPath, fileName);

        await using var stream = File.Create(filePath);
        await file.CopyToAsync(stream);

        return fileName;
    }

    public void Delete(string fileName)
    {
        var filePath = Path.Combine(_uploadPath, fileName);
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    public string GetUrl(HttpRequest request, string fileName) =>
        $"{request.Scheme}://{request.Host}/uploads/{fileName}";
}
