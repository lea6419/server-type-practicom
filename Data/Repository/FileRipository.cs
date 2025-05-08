using Data;
using Microsoft.EntityFrameworkCore;


public class FileRepository : Repository<UserFile>, IFileRepository
{

    public FileRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<UserFile>> GetFilesByUserIdAsync(int userId)
    {
        return await _context.Files
            .Where(f => f.UserId == userId && !f.IsDeleted) // מסנן קבצים שנמחקו רך
            .ToListAsync();
    }
    public async Task<IEnumerable<UserFile>> GetAllFileAsync()
    {
        return await _context.Files
            .Where(f => !f.IsDeleted) // מסנן קבצים שנמחקו רך
            .ToListAsync();
    }

    public async Task<UserFile?> GetByIdAsync(int fileId)
    {
        return await _context.Files
        .AsNoTracking()
        .FirstOrDefaultAsync(f => f.Id == fileId && !f.IsDeleted);
    }

    public async Task ChangeStatus(FileStatus status, int fileId)
    {
        var file = await _context.Files.FindAsync(fileId);
        if (file == null) return;

        file.Status = (global::FileStatus)(int)status;
        file.UpdatedAt = DateTime.UtcNow;
        _context.Files.Update(file);
        await _context.SaveChangesAsync();
    }

    public async Task<UserFile> SoftDeleteFileAsync(int fileId)
    {
        var file = await _context.Files.FindAsync(fileId);
        if (file == null) return null;

        file.IsDeleted = true;
        file.UpdatedAt = DateTime.UtcNow;
        _context.Files.Update(file);
        await _context.SaveChangesAsync();
        return file;
    }
    public async Task<List<UserFile>> GetTypedFiles()
    {
        var files = await _context.Files
      .Where(f => f.Status == FileStatus.TypingInProgress)

       .ToListAsync();
        return files;
    }
    public async Task UploadOriginalFileAsync(UserFile newFile)
    {
        newFile.Status = (global::FileStatus)(int)FileStatus.UploadedByClient;
        newFile.CreatedAt = DateTime.UtcNow;
        _context.Files.Add(newFile);
        await _context.SaveChangesAsync();
    }

    public async Task UploadTranscribedFileAsync(int fileId, string filePath)
    {
        var file = await _context.Files.FindAsync(fileId);
        if (file == null) return;

        file.TranscribedFileUrl = filePath;
        file.Status = (global::FileStatus)(int)FileStatus.TypedAndUploaded;
        file.UpdatedAt = DateTime.UtcNow;

        _context.Files.Update(file);
        await _context.SaveChangesAsync();
    }

    public async Task<List<UserFile>> GetFilesWaitingForTyping()
    {
        var files = await _context.Files
       .Where(f => f.Status == FileStatus.WaitingForTyping)
       .ToListAsync();
        return files;
    }


}
