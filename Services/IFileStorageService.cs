namespace TechMove.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string folder);
        Task<byte[]> GetFileAsync(string filePath);
        Task<bool> DeleteFileAsync(string filePath);
        bool FileExists(string filePath);
        string GetFileExtension(string fileName);
        bool IsValidPdfFile(IFormFile file);
    }
}
