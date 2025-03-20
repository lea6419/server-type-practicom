using Amazon.Auth.AccessControlPolicy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApplication1.Controllers
{
    public class FileController : Controller
    {
        private readonly IFileService _fileService;
        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }


        [HttpPost("start-typing/{fileId}")]
        public async Task<IActionResult> StartTyping(int fileId)
        {
            var file = await _fileService.GetFileByIdAsync(fileId);
            if (file == null) return NotFound();

            file.Status = "InProgress";
            await _fileService.UpdateFileAsync(file);

            return Ok(new { message = "File is now in progress" });
        }
        [HttpPost("complete-typing/{fileId}")]
        public async Task<IActionResult> CompleteTyping(int fileId, [FromBody] string newFilePath)
        {
            var file = await _fileService.GetFileByIdAsync(fileId);
            if (file == null) return NotFound();

            // עדכון הקובץ עם הגרסה החדשה
            file.FilePath = newFilePath;
            file.Status ="Completed";
            file.UpdatedAt = DateTime.UtcNow;

            await _fileService.UpdateFileAsync(file);

            return Ok(new { message = "File has been updated and marked as completed" });
        }

        [HttpDelete("{fileId}")]
        public async Task<IActionResult> SoftDeleteFile(int fileId)
        {
            var deletedFile = await _fileService.SoftDeleteFileAsync(fileId);
            if (deletedFile == null)
            {
                return NotFound(new { message = "File not found" });
            }
            return Ok(new { message = "File soft deleted successfully", file = deletedFile });
        }
        [Authorize(Policy = "ClientOnly")] // רק משתמשים עם role="client" יכולים להעלות קובץ
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] DateTime deadline)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Invalid file upload" });

            // שליפת ה-ID של המשתמש המחובר מה-Token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(new { message = "User not authorized" });

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized(new { message = "Invalid user ID" });

            // שמירת הקובץ ב-S3 והוספה למערכת
            var userFile = await _fileService.UploadFileAsync(file, deadline, userId);

            return Ok(new { message = "File uploaded successfully", file = userFile });
        }


    }

}
