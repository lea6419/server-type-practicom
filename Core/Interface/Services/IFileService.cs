using Microsoft.AspNetCore.Http;

public interface IFileService
{
    Task<IEnumerable<UserFile>> GetFilesByUserIdAsync(int userId);
    Task<UserFile?> GetFileByIdAsync(int fileId);
    Task<UserFile> UpdateFileAsync(UserFile file);
    Task<UserFile> UploadFileAsync(IFormFile file, DateTime deadline, int userId); // תיקון שם המתודה
    Task<UserFile?> SoftDeleteFileAsync(int fileId);
    Task<bool> DeleteFileAsync(int fileId);
      Task<string> GetDownloadUrlAsync(int fileId);
}
