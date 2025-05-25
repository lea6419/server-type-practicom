using Amazon.Auth.AccessControlPolicy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Amazon.S3.Model;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using static FileService;
using Service.Services;

namespace WebApplication1.Controllers
{
    public class FileController : Controller
    {
        private readonly IFileService _fileService;
        private readonly AuthService authService;
        private readonly EmailService emailService;
        private readonly ILogger<FileController> _logger;
        private readonly IUserService _userService;

        public FileController(IFileService fileService, ILogger<FileController> logger, AuthService authService, EmailService emailService,IUserService userService)
        {
            _fileService = fileService;
            _logger = logger;
            this.authService = authService;
            this.emailService = emailService;
            this._userService = userService;
        }
        private string GetEmailBody(string fileUrl)
        {
            return $@"
    <html>
    <head>
        <style>
            body {{
                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                background-color: #f5f5f5;
                padding: 20px;
                color: #333;
            }}
            .container {{
                background-color: #fff;
                border-radius: 10px;
                padding: 30px;
                max-width: 600px;
                margin: auto;
                box-shadow: 0 2px 5px rgba(0,0,0,0.1);
            }}
            .button {{
                background-color: #4CAF50;
                color: white;
                padding: 12px 20px;
                text-align: center;
                text-decoration: none;
                display: inline-block;
                font-size: 16px;
                margin-top: 20px;
                border-radius: 5px;
            }}
            .footer {{
                margin-top: 30px;
                font-size: 12px;
                color: #888;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <h2>🔔 קובץ חדש הועלה בהצלחה</h2>
            <p>שלום,</p>
            <p>קובץ חדש הועלה למערכת וניתן להורדה בלחיצה על הכפתור מטה:</p>
            <a class='button' href='{fileUrl}' target='_blank'>הורד קובץ</a>
            <div class='footer'>
                <p>מייל זה נשלח באופן אוטומטי ממערכת הקבצים שלך.</p>
            </div>
        </div>
    </body>
    </html>";
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetSystemStats()
        {
            var stats = await _fileService.GetSystemStatsAsync();
            return Ok(stats);
        }
        [HttpGet("files/waiting-for-typing")]
        public IActionResult GetFilesWaitingForTyping()
        {
            var files = _fileService.GetFilesWaitingForTyping();

            return Ok(files);
        }
        [HttpGet("files/typed")]
        public IActionResult GetTypedFiles()
        {
            var files = _fileService.GetTypedFiles();

            return Ok(files);
        }

        //[HttpGet("download/stream/{fileId}")]
        //public async Task<IActionResult> DownloadFileFromS3(int fileId)
        //{
        //    try
        //    {
        //        // קריאה לשירות הקבצים לקבלת ה-Stream של הקובץ
        //        var fileStream = await _fileService.GetFileStreamAsync(fileId);

        //        if (fileStream == null)
        //        {
        //            _logger.LogWarning("File not found for fileId: {FileId}", fileId);
        //            return NotFound("File not found in S3");
        //        }


        //        // החזרת הקובץ כתגובה
        //        return File(fileStream, "", "");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error downloading file with ID: {FileId}", fileId);
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}

        [HttpGet("download/{fileId}")]

        public async Task<IActionResult> DownloadFile(int fileId)
        {
            try
            {
                // קריאה לשירות הקבצים להורדת ה-URL
                var downloadUrl = await _fileService.GetDownloadUrlAsync(fileId);
                if (string.IsNullOrEmpty(downloadUrl))
                {
                    _logger.LogWarning("Download URL not found for fileId: {FileId}", fileId);
                    return NotFound("File not found");
                }

                _logger.LogInformation("Redirecting to download URL: {DownloadUrl}", downloadUrl);
                return Ok(new { url = downloadUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file with ID: {FileId}", fileId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("download-type/{fileId}")]

        public async Task<IActionResult> DownloadFiletype(int fileId)
        {
            try
            {
                // קריאה לשירות הקבצים להורדת ה-URL
                var downloadUrl = await _fileService.GetDownloadUrlAsyncType(fileId);
                if (string.IsNullOrEmpty(downloadUrl))
                {
                    _logger.LogWarning("Download URL not found for fileId: {FileId}", fileId);
                    return NotFound("File not found");
                }

                _logger.LogInformation("Redirecting to download URL: {DownloadUrl}", downloadUrl);
                return Ok(new { url = downloadUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file with ID: {FileId}", fileId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //[HttpPost("start-typing/{fileId}")]
        //public async Task<IActionResult> StartTyping(int fileId)
        //{
        //    _logger.LogInformation("Starting typing for file ID: {FileId}", fileId);
        //    var file = await _fileService.GetFileByIdAsync(fileId);
        //    if (file == null)
        //    {
        //        _logger.LogWarning("File not found for ID: {FileId}", fileId);
        //        return NotFound();
        //    }

        //    file.Status = 0;
        //    await _fileService.UpdateFileAsync(file);
        //    _logger.LogInformation("File ID: {FileId} status updated to InProgress", fileId);

        //    return Ok(new { message = "File is now in progress" });
        //}
        //[HttpPost("complete-typing/{fileId}")]
        //public async Task<IActionResult> CompleteTyping(int fileId, [FromBody] string newFilePath)
        //{
        //    _logger.LogInformation("Completing typing for file ID: {FileId}", fileId);
        //    var file = await _fileService.GetFileByIdAsync(fileId);
        //    if (file == null)
        //    {
        //        _logger.LogWarning("File not found for ID: {FileId}", fileId);
        //        return NotFound();
        //    }

        //    // עדכון נתיב הקובץ
        //    file.FilePath = newFilePath;
        //    file.Status = 3; // סטטוס הושלמה
        //    file.UpdatedAt = DateTime.UtcNow;

        //    await _fileService.UpdateFileAsync(file);
        //    _logger.LogInformation("File ID: {FileId} updated and marked as completed", fileId);

        //    return Ok(new { message = "File has been updated and marked as completed" });
        //}


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

        [HttpGet("allFiles")]
        public async Task<IActionResult> GetUAllFiles()
        {
            _logger.LogInformation("Retrieving all files ");
            var files = await _fileService.GetAllFileAsync();
            if (files == null || !files.Any())
            {
                _logger.LogWarning("No files found");
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
        [HttpPost("start-typing/{fileId}")]
        public async Task<IActionResult> StartTyping(int fileId)
        {
            await _fileService.SetFileStatusAsync(fileId, FileStatus.InProgress);
            return Ok();
        }

        [HttpPost("upload-typist")]
        [Authorize]
        public async Task<IActionResult> UploadFileFromTypist([FromForm] TypistUploadDto uploadDto)
        {
            _logger.LogInformation("Uploading file. File name: {FileName}, originalFileId: {OriginalFileId}, fileId: {FileId}",
                uploadDto.File?.FileName, uploadDto.OriginalFileId, uploadDto.FileId);

            if (string.IsNullOrEmpty(uploadDto.OriginalFileId) || uploadDto.FileId == 0)
            {
                return BadRequest(new { message = "Missing required file information" });
            }

            if (uploadDto.File == null || uploadDto.File.Length == 0)
            {
                return BadRequest(new { message = "Invalid file upload" });
            }

            var userIdClaim = User.FindFirst("id");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            var userFile = await _fileService.UploadTranscribedFileAsync(uploadDto.FileId, uploadDto.File, userId);
            var fileUrl = await _fileService.GetDownloadUrlAsync(userFile.Id);

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            string email = user.Email;
            string subject = "📎 קובץ חדש הועלה למערכת";
            string body = GetEmailBody(fileUrl);
            await emailService.SendEmailAsync(email, subject, body);


            var userFileDto = new UserFileDto
            {
                Id = userFile.Id,
                FileName = userFile.FileName,
                TranscribedFileUrl = userFile.TranscribedFileUrl,
                OriginalFileUrl = userFile.OriginalFileUrl,
                FileType = userFile.FileType,
                Deadline = userFile.Deadline,
                Status = userFile.Status,
                Size = userFile.Size,
                CreatedAt = userFile.CreatedAt,
                DownloadUrl = fileUrl
            };

            return Ok(new
            {
                message = "File uploaded successfully",
                file = userFileDto
            });
        }


        [HttpPost("upload-client")]
        [Authorize]
        public async Task<IActionResult> UploadFileFromClient([FromForm] IFormFile file, [FromForm] DateTime deadline)
        {
            _logger.LogInformation("Uploading file. File name: {FileName}", file?.FileName);
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Invalid file upload attempt.");
                return BadRequest(new { message = "Invalid file upload" });
            }

            var userIdClaim = User.FindFirst("id");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogWarning("Unauthorized user attempt to upload file.");
                return Unauthorized(new { message = "User not authorized" });
            }

            // העלאה בפועל של הקובץ

            var userFile = await _fileService.UploadFileAsync(file, deadline, userId);
            string fileUrl = await _fileService.GetDownloadUrlAsync(userFile.Id,true);
            string email = "le6736419@gmail.com"; // לדוגמה

            string subject = "קובץ חדש הועלה";
            string body = $"<p>קובץ הועלה בהצלחה. ניתן לגשת אליו <a href={fileUrl}>כאן</a>.</p>";


            await emailService.SendEmailAsync(email, subject, body);

            var userFileDto = new UserFileDto
            {
                Id = userFile.Id,
                FileName = userFile.FileName,
               OriginalFileUrl=userFile.OriginalFileUrl,
               TranscribedFileUrl=userFile.TranscribedFileUrl,
                FileType = userFile.FileType,
                Deadline = userFile.Deadline,
                Status = userFile.Status,
                Size = userFile.Size,
                CreatedAt = userFile.CreatedAt,
                DownloadUrl = fileUrl
            };

            return Ok(new
            {
                message = "File uploaded successfully",
                file = userFileDto
            });
        }

    }
}