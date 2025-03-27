using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using Microsoft.Extensions.Logging;

public class FileService : IFileService
{
    private readonly IFileRepository _fileRepository;
    private readonly Is3Service _s3Service;
    private readonly ILogger<FileService> _logger;

    public FileService(IFileRepository fileRepository, Is3Service s3Service, ILogger<FileService> logger)
    {
        _fileRepository = fileRepository;
        _s3Service = s3Service;
        _logger = logger;
    }

    public async Task<IEnumerable<UserFile>> GetFilesByUserIdAsync(int userId)
    {
        return await _fileRepository.GetFilesByUserIdAsync(userId);
    }

    public async Task<UserFile?> GetFileByIdAsync(int fileId)
    {
        return await _fileRepository.GetByIdAsync(fileId);
    }

    public async Task<UserFile> UpdateFileAsync(UserFile file)
    {
        return await _fileRepository.UpdateAsync(file);
    }

    public async Task<UserFile?> SoftDeleteFileAsync(int fileId)
    {
        return await _fileRepository.SoftDeleteFileAsync(fileId);
    }

    public async Task<UserFile> UploadFileAsync(IFormFile file, DateTime deadline, int userId)
    {
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        // העלאת הקובץ ל-S3 וקבלת ה-URL להורדה
        var filePath = await _s3Service.UploadFileAsync(file, fileName);


        var userFile = new UserFile
        {
            UserId = userId,
            Status = 2,
            FileName = file.FileName,
            FilePath = fileName,
            FileType = file.ContentType,
            Size = (int)file.Length,
            Deadline = deadline,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _fileRepository.AddAsync(userFile);
    }

    public async Task<bool> DeleteFileAsync(int fileId)
    {
        var file = await _fileRepository.GetByIdAsync(fileId);
        if (file == null)
        {
            _logger.LogWarning("File with ID {FileId} not found.", fileId);
            return false;
        }

        // שימוש בשם הקובץ מהנתיב השמור
        var fileKey = file.FilePath;

        // יצירת URL חתום להורדה


        try
        {
            await _s3Service.DeleteFileAsync(fileKey);
            await _fileRepository.DeleteAsync(fileId);
            _logger.LogInformation("File {FileId} deleted successfully.", fileId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileId}", fileId);
            return false;
        }
    }

    public async Task<string> GetDownloadUrlAsync(int fileId)
    {
        var userFile = await _fileRepository.GetByIdAsync(fileId);
        if (userFile == null)
        {
            throw new ArgumentException("File not found.");
        }

        // שימוש בשם הקובץ מהנתיב השמור
        var filepath = userFile.FilePath;

        // יצירת URL חתום להורדה
        return await _s3Service.GetDownloadUrlAsync(filepath);
    }

    public async Task<Stream> GetFileStreamAsync(int fileId)
    {
        var userFile = await _fileRepository.GetByIdAsync(fileId);
        if (userFile == null)
        {
            throw new ArgumentException("File not found.");
        }

        // שימוש בשם הקובץ מהנתיב השמור
        var filepath = userFile.FilePath;

        // יצירת URL חתום להורדה
        return await _s3Service.GetFileStreamAsync(filepath);
    }
}
