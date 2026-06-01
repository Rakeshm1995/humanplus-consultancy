namespace Humanplus_Manpower_Consulting.Helpers
{
    public static class FileValidationHelper
    {
        private static readonly Dictionary<string, string[]> AllowedMimeTypes = new()
        {
            [".pdf"] = ["application/pdf"],
            [".jpg"] = ["image/jpeg"],
            [".jpeg"] = ["image/jpeg"],
            [".png"] = ["image/png"],
            [".doc"] = ["application/msword"],
            [".docx"] = ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
        };

        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        public static (bool IsValid, string Error) Validate(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return (false, "No file selected.");

            if (file.Length > MaxFileSize)
                return (false, "File size exceeds 5 MB limit.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedMimeTypes.ContainsKey(ext))
                return (false, "File type not allowed. Allowed: PDF, JPG, PNG, DOC, DOCX.");

            if (!AllowedMimeTypes[ext].Contains(file.ContentType.ToLowerInvariant()))
                return (false, "File content type mismatch.");

            return (true, string.Empty);
        }
    }
}
