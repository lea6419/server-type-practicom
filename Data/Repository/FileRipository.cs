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

    public async Task<UserFile?> GetByIdAsync(int fileId)
    {
        return await _context.Files.FirstOrDefaultAsync(f => f.Id == fileId);
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
}
