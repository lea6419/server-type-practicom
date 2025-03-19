public interface IBackupRepository : IRepository<Backup>
{
    Task<IEnumerable<Backup>> GetBackupsByFileIdAsync(int fileId);

}
