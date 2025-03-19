
    public class BackupService : IBackupService
    {
        private readonly IBackupRepository _backupRepository;
        private readonly IFileRepository _fileRepository;

        public BackupService(IBackupRepository backupRepository, IFileRepository fileRepository)
        {
            _backupRepository = backupRepository;
            _fileRepository = fileRepository;
        }

        public async Task<Result<Backup>> CreateBackupAsync(Backup backup)
        {
            // בדיקה אם הקובץ קיים
            var fileExists = await _fileRepository.GetByIdAsync(backup.FileId);
            if (fileExists==null)
            {
                return Result<Backup>.Fail($"File with ID {backup.FileId} does not exist.");
            }

            // הוספת הגיבוי
            await _backupRepository.AddAsync(backup);
            return Result<Backup>.Success(backup);
        }
        public async Task<IEnumerable<Backup>> GetBackupsByFileIdAsync(int fileId)
    {
        return await _backupRepository.GetBackupsByFileIdAsync(fileId);
    }


}
