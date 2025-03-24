using Amazon.Auth.AccessControlPolicy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace WebApplication1.Controllers
{
    public class FileController : Controller
    {
        private readonly IFileService _fileService;
        private readonly ILogger<FileController> _logger;

        public FileController(IFileService fileService, ILogger<FileController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        [HttpPost("start-typing/{fileId}")]
        public async Task<IActionResult> StartTyping(int fileId)
        {
            _logger.LogInformation("Starting typing for file ID: {FileId}", fileId);
            var file = await _fileService.GetFileByIdAsync(fileId);
            if (file == null)
            {
                _logger.LogWarning("File not found for ID: {FileId}", fileId);
                return NotFound();
            }

            file.Status = 0;
            await _fileService.UpdateFileAsync(file);
            _logger.LogInformation("File ID: {FileId} status updated to InProgress", fileId);

            return Ok(new { message = "File is now in progress" });
        }

        [HttpPost("complete-typing/{fileId}")]
        public async Task<IActionResult> CompleteTyping(int fileId, [FromBody] string newFilePath)
        {
            _logger.LogInformation("Completing typing for file ID: {FileId}", fileId);
            var file = await _fileService.GetFileByIdAsync(fileId);
            if (file == null)
            {
                _logger.LogWarning("File not found for ID: {FileId}", fileId);
                return NotFound();
            }

            file.FilePath = newFilePath;
            file.Status = 3;
            file.UpdatedAt = DateTime.UtcNow;

            await _fileService.UpdateFileAsync(file);
            _logger.LogInformation("File ID: {FileId} updated and marked as completed", fileId);

            return Ok(new { message = "File has been updated and marked as completed" });
        }

        [HttpGet("user-files/{userId}")]
        public async Task<IActionResult> GetUserFiles(int userId)
        {
            _logger.LogInformation("Retrieving files for user ID: {UserId}", userId);
            var files = await _fileService.GetFilesByUserIdAsync(userId);
            if (files == null || !files.Any())
            {
                _logger.LogWarning("No files found for user ID: {UserId}", userId);
                return NotFound(new { message = "No files found for this user." });
            }

            var sortedFiles = files.OrderBy(f => f.CreatedAt).ToList();
            return Ok(sortedFiles);
        }

        [HttpDelete("{fileId}")]
        public async Task<IActionResult> SoftDeleteFile(int fileId)
        {
            _logger.LogInformation("Soft deleting file ID: {FileId}", fileId);
            var deletedFile = await _fileService.SoftDeleteFileAsync(fileId);
            if (deletedFile == null)
            {
                _logger.LogWarning("File not found for ID: {FileId}", fileId);
                return NotFound(new { message = "File not found" });
            }

            _logger.LogInformation("File ID: {FileId} soft deleted successfully", fileId);
            return Ok(new { message = "File soft deleted successfully", file = deletedFile });
        }
        [HttpPost("upload")]
        [Authorize(Policy = "ClientOrTypeist")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] DateTime deadline)
        {
            _logger.LogInformation("Uploading file. File name: {FileName}", file?.FileName);
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Invalid file upload attempt.");
                return BadRequest(new { message = "Invalid file upload" });
            }

            // הסרת קטע הקוד שאחראי על אימות המשתמש
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogWarning("Unauthorized user attempt to upload file.");
                return Unauthorized(new { message = "User not authorized" });
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogWarning("Invalid user ID retrieved from claims.");
                return Unauthorized(new { message = "Invalid user ID" });
            }

            // כאן, ניתן לקבוע userId לערך ברירת מחדל או לשנות את הלוגיקה שלך בהתאם
            
            var userFile = await _fileService.UploadFileAsync(file, deadline, userId);
            _logger.LogInformation("File uploaded successfully for user ID: {UserId}", userId);

            return Ok(new { message = "File uploaded successfully", file = userFile });
        }

    }
}