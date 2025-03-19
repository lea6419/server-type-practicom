public class ProgressService : IProgressService
{
    private readonly IProgressRepository _progressRepository;

    public ProgressService(IProgressRepository progressRepository)
    {
        _progressRepository = progressRepository;
    }

    public async Task<IEnumerable<Progress>> GetProgressByFileIdAsync(int fileId)
    {
        return await _progressRepository.GetProgressByFileIdAsync(fileId);
    }

    public async Task<Progress?> GetProgressByIdAsync(int progressId)
    {
        return await _progressRepository.GetByIdAsync(progressId);
    }

    public async Task<Progress> UpdateProgressAsync(Progress progress)
    {
         await _progressRepository.UpdateAsync(progress);
        return progress;
    }
}
