using Core.Models;
using Microsoft.AspNetCore.Http;

public interface IFileService
{
    Task<IEnumerable<UserFile>> GetFilesByUserIdAsync(int userId);
    Task<UserFile?> GetFileByIdAsync(int fileId);
    Task<UserFile> UpdateFileAsync(UserFile file);
    Task<UserFile> UploadFileAsync(IFormFile file, DateTime deadline, int userId); // תיקון שם המתודה
    Task<UserFile?> SoftDeleteFileAsync(int fileId);
    Task<bool> DeleteFileAsync(int fileId);
      Task<string> GetDownloadUrlAsync(int fileId, bool isTranscribed=false);
     Task<string> GetDownloadUrlAsyncType(int fileId, bool isTranscribed = false);

    Task<SystemStatsDto> GetSystemStatsAsync();
    Task<List<UserFile>> GetTypedFiles();
    Task<List<UserFile>> GetFilesWaitingForTyping();
      Task<IEnumerable<UserFile>> GetAllFileAsync();
    Task<UserFile> UploadTranscribedFileAsync(int fileId, IFormFile file, int userId);
    Task SetFileStatusAsync(int fileId, FileStatus newStatus);
}
