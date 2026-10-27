using System.IO;

namespace Spotsom.Api.Services;

public class AudioStorageService(IWebHostEnvironment environment)
{
    public async Task<string> SaveAsync(
        IFormFile audio,
        CancellationToken cancellationToken)
    {
        var allowedExtensions = new[] { ".mp3" };

        var extension = Path.GetExtension(audio.FileName)
            .ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                "This Audio format is not allowed. Only .mp3 files are accepted.");
        }

        if (audio.Length == 0 || audio.Length > 20 * 1024 * 1024)
        {
            throw new InvalidOperationException(
                "The file must be up to 20 MB.");
        }

        var fileName = $"{Guid.NewGuid():N}{extension}";

        var folder = Path.Combine(
            environment.ContentRootPath,
            "wwwroot",
            "audio"
        );

        Directory.CreateDirectory(folder);

        var filePath = Path.Combine(folder, fileName);

        await using var stream = new FileStream(
            filePath,
            FileMode.CreateNew,
            FileAccess.Write
        );

        await audio.CopyToAsync(stream, cancellationToken);

        return $"/audio/{fileName}";
    }
}