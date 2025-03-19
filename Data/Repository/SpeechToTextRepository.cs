using Data;
using Microsoft.EntityFrameworkCore;

public class SpeechToTextRepository : Repository<SpeechToText>, ISpeechToTextRepository
{
    public SpeechToTextRepository(ApplicationDbContext context) : base(context) { }

    public async Task<SpeechToText> GetByFileIdAsync(int fileId)
    {
        return await _context.SpeechToTexts.FirstOrDefaultAsync(s => s.FileId == fileId);
    }
}
