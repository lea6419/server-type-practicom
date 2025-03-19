

public interface IBackupService
{
    Task<IEnumerable<Backup>> GetBackupsByFileIdAsync(int fileId);

    Task<Result<Backup>> CreateBackupAsync(Backup backup);
}
