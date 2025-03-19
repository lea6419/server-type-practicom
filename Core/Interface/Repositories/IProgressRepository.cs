public interface IProgressRepository : IRepository<Progress>
{
    Task<IEnumerable<Progress>> GetProgressByFileIdAsync(int fileId);
}
