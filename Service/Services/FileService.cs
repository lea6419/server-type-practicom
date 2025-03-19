

using Microsoft.AspNetCore.Http;

public class FileService : IFileService
{
    private readonly IFileRepository _fileRepository;

    public readonly Is3Service _s3Service;

    public FileService(IFileRepository fileRepository, Is3Service is3Service)
    {
        _fileRepository = fileRepository;
        _s3Service = is3Service;
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
            Status = "Pending", // סטטוס בהמתנה
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

        // מחיקת הקובץ מ-S3
        await _s3Service.DeleteFileAsync(file.FilePath);

        // מחיקת הקובץ מהמסד נתונים
        await _fileRepository.DeleteAsync(fileId);

        return true; // מחיקה בוצעה בהצלחה
    }

   
}