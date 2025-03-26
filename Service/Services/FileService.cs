using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

public class FileService : IFileService
{
    private readonly IFileRepository _fileRepository;
    private readonly Is3Service _s3Service;

    public FileService(IFileRepository fileRepository, Is3Service s3Service)
    {
        _fileRepository = fileRepository;
        _s3Service = s3Service;
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
        // יצירת שם ייחודי לקובץ
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

        // העלאת הקובץ ל-S3 ושמירת הנתיב
        var filePath = await _s3Service.UploadFileAsync(file, fileName); // קריאה לפונקציה החדשה להעלאת הקובץ

        // יצירת אובייקט הקובץ
        var userFile = new UserFile
        {
            UserId = userId,
            Status = 2, // סטטוס בהמתנה
            FileName = file.FileName,
            FilePath = filePath,
            FileType = file.ContentType,
            Size = (int)file.Length,
            Deadline = deadline,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // שמירת הקובץ במסד הנתונים
        return await _fileRepository.AddAsync(userFile);
    }

    public async Task<bool> DeleteFileAsync(int fileId)
    {
        var file = await _fileRepository.GetByIdAsync(fileId);
        if (file == null)
        {
            return false; // הקובץ לא נמצא
        }

        // חילוץ ה-Key מתוך ה-URL של הקובץ
        var fileKey = Path.GetFileName(new Uri(file.FilePath).LocalPath);

        try
        {
            // מחיקת הקובץ מ-S3
            await _s3Service.DeleteFileAsync(fileKey);
        }
        catch (Exception ex)
        {
            // ניתן להוסיף לוגים כאן כדי לדעת למה מחיקה נכשלה
            Console.WriteLine($"שגיאה במחיקת קובץ מ-S3: {ex.Message}");
            return false;
        }

        // מחיקת הקובץ מהמסד נתונים
        await _fileRepository.DeleteAsync(fileId);

        return true; // מחיקה בוצעה בהצלחה
    }
    public async Task<string> GetDownloadUrlAsync(int fileId)
    {
        var userFile = await _fileRepository.GetByIdAsync(fileId);
        if (userFile == null)
        {
            throw new ArgumentException("File not found.");
        }

        // כעת תוכל להשתמש ב-userFile.FilePath או ב-userFile.FileName
        var fileName = userFile.FileName;

        // קבל URL חתום מ-S3
        return await _s3Service.GetDownloadUrlAsync(fileName);
    }
  public async  Task<Stream> GetFileStreamAsync(int fileId)
    {
        var userFile = await _fileRepository.GetByIdAsync(fileId);
        if (userFile == null)
        {
            throw new ArgumentException("File not found.");
        }

        // כעת תוכל להשתמש ב-userFile.FilePath או ב-userFile.FileName
        var fileName = userFile.FileName;

        // קבל URL חתום מ-S3
        return await _s3Service.GetFileStreamAsync(fileName);
    }
}
