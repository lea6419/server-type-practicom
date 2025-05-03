using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class FileService : IFileService
{
    private readonly IFileRepository _fileRepository;
    private readonly Is3Service _s3Service;
    private readonly ILogger<FileService> _logger;
    private readonly IUserRepository _userRepository;

    public enum FileStatus
    {
        UploadedByClient = 1,
        WaitingForTyping = 2,
        TypingInProgress = 3,
        TypedAndUploaded = 4,
        DownloadedByClient = 5,
        UpdatedVersion = 6,
        SoftDeleted = 99
    }

    public FileService(IFileRepository fileRepository, Is3Service s3Service, ILogger<FileService> logger, IUserRepository userRepository)
    {
        _fileRepository = fileRepository;
        _s3Service = s3Service;
        _logger = logger;
        _userRepository = userRepository;
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
        var user = await _userRepository.GetByIdAsync(file.UserId);
        if (user == null)
        {
            throw new ArgumentException("User not found.");
        }

        if (user.Role == "typist")
        {
            _fileRepository.ChangeStatus((int)FileStatus.TypingInProgress, file.Id);
        }
        else
        {
            _fileRepository.ChangeStatus((int)FileStatus.UploadedByClient, file.Id);
        }

        return await _fileRepository.UpdateAsync(file);
    }

    public async Task<UserFile?> SoftDeleteFileAsync(int fileId)
    {
        var file = await _fileRepository.GetByIdAsync(fileId);
        if (file == null)
        {
            _logger.LogWarning("File with ID {FileId} not found for soft delete.", fileId);
            return null;
        }

        file.Status = (int)FileStatus.SoftDeleted;
        file.IsDeleted = true;
        file.UpdatedAt = DateTime.UtcNow;

        await _fileRepository.UpdateAsync(file);
        _logger.LogInformation("File {FileId} soft-deleted successfully.", fileId);

        return file;
    }
    public async Task<SystemStatsDto> GetSystemStatsAsync()
    {
        var users = await _userRepository.GetAllAsync();
        var files = await _fileRepository.GetAllAsync();

        return new SystemStatsDto
        {
            TotalUsers = users.Count(),
            TypistsCount = users.Count(u => u.Role == "typist"),
            ClientsCount = users.Count(u => u.Role == "client"),
            TotalFiles = files.Count(),
            FilesWaiting = files.Count(f => f.Status == (int)FileStatus.WaitingForTyping),
            FilesInProgress = files.Count(f => f.Status == (int)FileStatus.TypingInProgress),
            FilesCompleted = files.Count(f => f.Status == (int)FileStatus.TypedAndUploaded)
        };
    }


    public async Task<UserFile> UploadFileAsync(IFormFile file, DateTime deadline, int userId)
    {
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = await _s3Service.UploadFileAsync(file, fileName);

        var userFile = new UserFile
        {
            UserId = userId,
            Status = (int)FileStatus.WaitingForTyping,
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

        var fileKey = file.FilePath;

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

        var user = await _userRepository.GetByIdAsync(userFile.UserId);
        if (user == null)
        {
            throw new ArgumentException("User not found.");
        }

        // שינוי סטטוס בהתאם למי שמבצע את ההורדה
        if (user.Role == "typist")
        {
            _fileRepository.ChangeStatus((int)FileStatus.WaitingForTyping, fileId);
        }
        else if (user.Role == "client")
        {
            _fileRepository.ChangeStatus((int)FileStatus.DownloadedByClient, fileId);
        }

        return await _s3Service.GetDownloadUrlAsync(userFile.FilePath);
    }

    public async Task<Stream> GetFileStreamAsync(int fileId)
    {
        var userFile = await _fileRepository.GetByIdAsync(fileId);
        if (userFile == null)
        {
            throw new ArgumentException("File not found.");
        }

        return await _s3Service.GetFileStreamAsync(userFile.FilePath);
    }
    public async Task<IEnumerable<UserFile>> GetAllFileAsync()
    {
        return await _fileRepository.GetAllAsync();
    }

    public async Task<List<UserFile>> GetTypedFiles()
    {
        var files =await  _fileRepository.GetTypedFiles();
           
        return files;
    }
    public async Task<List<UserFile>> GetFilesWaitingForTyping()
    {
        var files = await _fileRepository.GetFilesWaitingForTyping();

        return files;
    }

}
