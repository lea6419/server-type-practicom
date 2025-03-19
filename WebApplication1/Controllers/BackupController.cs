//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//[ApiController]
//[Route("api/[controller]")]
//public class BackupController : ControllerBase
//{
//    private readonly IBackupService _backupService;

//    public BackupController(IBackupService backupService)
//    {
//        _backupService = backupService;
//    }

//    // יצירת גיבוי
//    [HttpPost]
//    public async Task<IActionResult> CreateBackup([FromBody] BackupDto backupDto)
//    {
//        var backup = new Backup
//        {
//            FileId = backupDto.FileId,
//            CreatedAt = DateTime.UtcNow,
//            BackupPath = backupDto.BackupFilePath
//        };
//        var result = await _backupService.CreateBackupAsync(backup);
//        return Ok(result);
//    }

//    // קבלת גיבויים לפי קובץ
//    [HttpGet("file/{fileId}")]
//    public async Task<IActionResult> GetBackupsByFileId(int fileId)
//    {
//        var backups = await _backupService.GetBackupsByFileIdAsync(fileId);
//        var result = backups.Select(b => new BackupDto
//        {
//            Id = b.BackupId,
//            FileId = b.FileId,
//            BackupDate = b.CreatedAt,
//            BackupFilePath = b.BackupPath
//        });

//        return Ok(result);
//    }
//}