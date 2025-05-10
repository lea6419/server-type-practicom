using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;

public class FileService : IFileService
{
    private readonly IFileRepository _fileRepository;
    private readonly Is3Service _s3Service;
    private readonly ILogger<FileService> _logger;
    private readonly IUserRepository _userRepository;


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
            _fileRepository.ChangeStatus(FileStatus.InProgress, file.Id);
        }
        else
        {
            _fileRepository.ChangeStatus(FileStatus.UploadedByUser, file.Id);
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

        file.Status = FileStatus.SoftDeleted;
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
            ClientsCount = users.Count(u => u.Role == "user"),
            TotalFiles = files.Count(),
            FilesWaiting = files.Count(f => f.Status == FileStatus.UploadedByUser),
            FilesInProgress = files.Count(f => f.Status ==FileStatus.InProgress),
            FilesCompleted = files.Count(f => f.Status == (FileStatus.ReturnedToUser))
        };
    }


    public async Task<UserFile> UploadFileAsync(IFormFile file, DateTime deadline, int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException("User not found.");
        }

        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("No file uploaded.");
        }

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = await _s3Service.UploadFileAsync(file, fileName);

        UserFile userFile;

        if (user.Role == "user")
        {
            // לקוח מעלה קובץ סרוק חדש
            userFile = new UserFile
            {
                UserId = userId,
                Status = FileStatus.UploadedByUser,
                FileName = file.FileName,
                OriginalFileUrl = filePath,
                UploadedBy = "user",
                FileType = file.ContentType,
                Size = (int)file.Length,
                Deadline = deadline,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Name = file.FileName

            };

            return await _fileRepository.AddAsync(userFile);
        }
        else if (user.Role == "typist")
        {
            // הקלדנית מעלה את הקובץ המוקלד עבור קובץ קיים
            throw new InvalidOperationException("Typists must use a separate method to upload transcribed files.");
        }

        throw new InvalidOperationException("Unsupported user role.");
    }

    public async Task<UserFile> UploadTranscribedFileAsync(int fileId, IFormFile file, int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null || user.Role != "typist")
        {
            throw new ArgumentException("Only typists can upload transcribed files.");
        }

        var originalFile = await _fileRepository.GetByIdAsync(fileId);
        if (originalFile == null)
        {
            throw new ArgumentException("Original file not found.");
        }

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = await _s3Service.UploadFileAsync(file, fileName);

        originalFile.TranscribedFileUrl = filePath;
        originalFile.Status = FileStatus.Completed;
        originalFile.UpdatedAt = DateTime.UtcNow;

        return await _fileRepository.UpdateAsync(originalFile);
    }



    public async Task<bool> DeleteFileAsync(int fileId)
    {
        var file = await _fileRepository.GetByIdAsync(fileId);
        if (file == null)
        {
            _logger.LogWarning("File with ID {FileId} not found.", fileId);
            return false;
        }

        try
        {
            if (!string.IsNullOrEmpty(file.OriginalFileUrl))
            {
                await _s3Service.DeleteFileAsync(file.OriginalFileUrl);
                _logger.LogInformation("Original file deleted: {FileUrl}", file.OriginalFileUrl);
            }

            if (!string.IsNullOrEmpty(file.TranscribedFileUrl))
            {
                await _s3Service.DeleteFileAsync(file.TranscribedFileUrl);
                _logger.LogInformation("Transcribed file deleted: {FileUrl}", file.TranscribedFileUrl);
            }

            await _fileRepository.DeleteAsync(fileId);
            _logger.LogInformation("File metadata (ID: {FileId}) deleted from database.", fileId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting files for file ID {FileId}", fileId);
            return false;
        }
    }

    public async Task<string> GetDownloadUrlAsync(int fileId, bool isTranscribed = false)
    {
        _logger.LogInformation("Start GetDownloadUrlAsync for fileId: {FileId}, isTranscribed: {IsTranscribed}", fileId, isTranscribed);

        var userFile = await _fileRepository.GetByIdAsync(fileId);
        if (userFile == null)
        {
            _logger.LogWarning("File not found for fileId: {FileId}", fileId);
            throw new ArgumentException("File not found.");
        }

        _logger.LogInformation("File found: {FileName}, UserId: {UserId}", userFile.FileName, userFile.UserId);

        var user = await _userRepository.GetByIdAsync(userFile.UserId);
        if (user == null)
        {
            _logger.LogWarning("User not found for UserId: {UserId}", userFile.UserId);
            throw new ArgumentException("User not found.");
        }

        _logger.LogInformation("User found: {UserName}, Role: {Role}", user.Id, user.Role);

        if (user.Role == "typist")
        {
            _logger.LogInformation("Changing status to InProgress for fileId: {FileId}", fileId);
            _fileRepository.ChangeStatus(FileStatus.InProgress, fileId);
        }
        else if (user.Role == "user")
        {
            _logger.LogInformation("Changing status to ReturnedToUser for fileId: {FileId}", fileId);
            _fileRepository.ChangeStatus(FileStatus.ReturnedToUser, fileId);
        }

        if (isTranscribed && !string.IsNullOrEmpty(userFile.TranscribedFileUrl))
        {
            _logger.LogInformation("Returning transcribed file URL");
            return await _s3Service.GetDownloadUrlAsync(userFile.FileName+ "-typed");
        }

        if (!string.IsNullOrEmpty(userFile.OriginalFileUrl))
        {
            _logger.LogInformation("Returning original file URL");
            return await _s3Service.GetDownloadUrlAsync(userFile.FileName);
        }

        _logger.LogWarning("No file URL found for fileId: {FileId}", fileId);
        throw new ArgumentException("Neither original nor transcribed file URL found.");
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
