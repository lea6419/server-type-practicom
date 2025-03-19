using Data;
using Microsoft.EntityFrameworkCore;

public class BackupRepository : Repository<Backup>, IBackupRepository
{
    private readonly BackupRepository _repository;
    public BackupRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Backup>> GetBackupsByFileIdAsync(int fileId)
    {
        return await _context.Backups.Where(b => b.FileId == fileId).ToListAsync();
    }
}
