namespace TechMove.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileStorageService> _logger;
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public FileStorageService(IWebHostEnvironment environment, ILogger<FileStorageService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            if (!IsValidPdfFile(file))
                throw new ArgumentException("Only PDF files are allowed");

            // Create uploads folder if it doesn't exist
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsFolder);

            // Generate unique filename with GUID only (cleaner)
            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            _logger.LogInformation("File saved: {FileName} to folder: {Folder}", uniqueFileName, folder);

            // FIXED: Return relative path WITHOUT "uploads" prefix (since Download adds it)
            // Return format: "contracts/guid.pdf"
            return Path.Combine(folder, uniqueFileName).Replace("\\", "/");
        }

        public async Task<byte[]> GetFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_environment.WebRootPath, filePath);
            
            if (!File.Exists(fullPath))
                throw new FileNotFoundException("File not found");

            return await File.ReadAllBytesAsync(fullPath);
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath);
                
                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
                return false;
            }
        }

        public bool FileExists(string filePath)
        {
            var fullPath = Path.Combine(_environment.WebRootPath, filePath);
            return File.Exists(fullPath);
        }

        public string GetFileExtension(string fileName)
        {
            return Path.GetExtension(fileName).ToLowerInvariant();
        }

        public bool IsValidPdfFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            if (file.Length > MaxFileSize)
                return false;

            var extension = GetFileExtension(file.FileName);
            if (extension != ".pdf")
                return false;

            // Check content type
            if (file.ContentType != "application/pdf")
                return false;

            return true;
        }
    }
}
