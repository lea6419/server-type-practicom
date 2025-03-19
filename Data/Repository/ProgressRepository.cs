using Data;
using Microsoft.EntityFrameworkCore;

public class ProgressRepository : Repository<Progress>, IProgressRepository
{
    public ProgressRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Progress>> GetProgressByFileIdAsync(int fileId)
    {
        return await _context.Progresses.Where(p => p.FileId == fileId).ToListAsync();
    }
}
