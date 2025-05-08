public interface IFileRepository : IRepository<UserFile>
{
    Task<IEnumerable<UserFile>> GetFilesByUserIdAsync(int userId);
    Task<UserFile?> SoftDeleteFileAsync(int fileId);
    Task ChangeStatus(FileStatus status, int failId);
    Task<List<UserFile>> GetTypedFiles();
    Task<List<UserFile>> GetFilesWaitingForTyping();
      Task<IEnumerable<UserFile>> GetAllFileAsync();
      Task UploadOriginalFileAsync(UserFile newFile);
      Task UploadTranscribedFileAsync(int fileId, string filePath);
}
