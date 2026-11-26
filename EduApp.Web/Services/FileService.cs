namespace EduApp.Web.Services;

public class FileService
{
    private readonly IWebHostEnvironment _environment;

    private const long MaxImageSize = 5 * 1024 * 1024;
    private const long MaxMaterialSize = 10 * 1024 * 1024;

    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly string[] MaterialExtensions = { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".zip" };

    public FileService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public string? ValidateImage(IFormFile? file, bool isRequired = false, string label = "Görsel")
    {
        if (file is null)
        {
            return isRequired ? $"{label} zorunludur." : null;
        }

        if (file.Length == 0)
        {
            return $"{label} boş olamaz.";
        }
           
        if (file.Length > MaxImageSize)
        {
            return $"{label} 5 MB'den büyük olamaz.";
        }

        string ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(ext) || !ImageExtensions.Contains(ext))
        {
            return $"{label} yalnızca .jpg, .jpeg, .png veya .webp formatında olmalıdır.";
        }

        return null;
    }

    public string? ValidateMaterial(IFormFile? file)
    {
        if (file is null) return null;

        if (file.Length == 0)
        {
            return "Dosya boş olamaz.";
        }

        if (file.Length > MaxMaterialSize)
        {
            return "Dosya 10 MB'den büyük olamaz.";
        }

        string ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(ext) || !MaterialExtensions.Contains(ext))
        {
            return "Dosya yalnızca .pdf, .doc, .docx, .ppt, .pptx veya .zip formatında olmalıdır.";
        }

        return null;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string folder)
    {
        string ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        string directory = Path.Combine(_environment.WebRootPath, "uploads", folder);

        Directory.CreateDirectory(directory);

        string fileName = $"{Guid.NewGuid():N}{ext}";
        string physicalPath = Path.Combine(directory, fileName);

        await using FileStream stream = new(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(stream);

        return $"/uploads/{folder}/{fileName}";
    }

    public void DeleteFile(string? filePath, string folder)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return;

        string directory = Path.Combine(_environment.WebRootPath, "uploads", folder);
        string fileName = Path.GetFileName(filePath);

        if (string.IsNullOrWhiteSpace(fileName)) return;

        string physicalPath = Path.Combine(directory, fileName);
        if (!physicalPath.StartsWith(directory, StringComparison.OrdinalIgnoreCase)) return;

        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }
    }
}