using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace E1___Sosa_Morales.Services.Shared;

public static class CatalogPhotoStorage
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxPhotoBytes = 2 * 1024 * 1024;

    public static async Task<(bool Success, string Message, string? WebPath)> SaveAsync(
        IFormFile? photo,
        IWebHostEnvironment env,
        string folderName,
        string filePrefix)
    {
        if (photo is null || photo.Length == 0)
            return (true, string.Empty, null);

        if (photo.Length > MaxPhotoBytes)
            return (false, "La imagen no puede superar 2 MB.", null);

        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return (false, "Formato no permitido. Use JPG, PNG o WEBP.", null);

        var targetDir = Path.Combine(env.WebRootPath, "Public", "Images", folderName);
        Directory.CreateDirectory(targetDir);

        var safePrefix = string.Concat(filePrefix.Where(char.IsLetterOrDigit));
        if (string.IsNullOrWhiteSpace(safePrefix)) safePrefix = "photo";

        var fileName = $"{safePrefix}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(targetDir, fileName);

        await using (var stream = new FileStream(physicalPath, FileMode.Create))
            await photo.CopyToAsync(stream);

        return (true, string.Empty, $"/Public/Images/{folderName}/{fileName}");
    }

    public static void Delete(IWebHostEnvironment env, string? photoPath)
    {
        if (string.IsNullOrWhiteSpace(photoPath)) return;
        if (!photoPath.StartsWith("/Public/Images/", StringComparison.OrdinalIgnoreCase)) return;

        var relative = photoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(env.WebRootPath, relative));
        var webRoot = Path.GetFullPath(env.WebRootPath);

        if (!fullPath.StartsWith(webRoot, StringComparison.OrdinalIgnoreCase)) return;
        if (File.Exists(fullPath)) File.Delete(fullPath);
    }
}
