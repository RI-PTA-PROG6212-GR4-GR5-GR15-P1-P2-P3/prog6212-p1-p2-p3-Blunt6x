namespace RaceDay.Web.Infrastructure;

/// <summary>Friendly up-front checks for image uploads. The API repeats these checks; the MVC app never stores the file itself.</summary>
public static class ImageUpload {
    public const long MaxBytes = 5 * 1024 * 1024;
    private static readonly string[] Extensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
    public const string ErrorMessage = "Upload a JPG, PNG, GIF or WebP image of 5 MB or less.";

    public static bool IsValid(IFormFile file) =>
        file.Length > 0 && file.Length <= MaxBytes && Extensions.Contains(Path.GetExtension(file.FileName), StringComparer.OrdinalIgnoreCase);
}
