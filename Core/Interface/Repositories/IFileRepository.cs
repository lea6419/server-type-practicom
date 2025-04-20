public interface IFileRepository : IRepository<UserFile>
{
    Task<IEnumerable<UserFile>> GetFilesByUserIdAsync(int userId);
    Task<UserFile?> SoftDeleteFileAsync(int fileId);
    Task ChangeStatus(int status, int failId);
}
