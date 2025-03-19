public interface IProgressService
{
    Task<IEnumerable<Progress>> GetProgressByFileIdAsync(int fileId);
    Task<Progress?> GetProgressByIdAsync(int progressId);
    Task<Progress> UpdateProgressAsync(Progress progress);
}
