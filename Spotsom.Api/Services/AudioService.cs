using System.IO;

namespace Spotsom.Api.Services;

/// <summary>
/// Stores uploaded MP3 files in the application's public audio directory.
/// It validates the format and size before creating a unique file name.
/// </summary>
public class AudioStorageService(IWebHostEnvironment environment)
{
    private IWebHostEnvironment _environment = environment;

    /// <summary>
    /// Saves an accepted MP3 upload and returns its public URL path.
    /// </summary>
    /// <param name="audio">The uploaded audio file.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The public URL path of the saved audio file.</returns>
    public async Task<string> SaveAsync(
        IFormFile audio,
        CancellationToken cancellationToken)
    {
        if (audio is null)
        {
            throw new ArgumentNullException(nameof(audio));
        }

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
            _environment.ContentRootPath,
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