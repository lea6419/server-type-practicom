using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProgressController : ControllerBase
{
    private readonly IProgressService _progressService;

    public ProgressController(IProgressService progressService)
    {
        _progressService = progressService;
    }

    // עדכון התקדמות
    [HttpPost("update")]
    public async Task<IActionResult> UpdateProgress([FromBody] ProgressDto progressDto)
    {
        var progress = new Progress
        {
            FileId = progressDto.FileId,
            Status = progressDto.Status,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _progressService.UpdateProgressAsync(progress);
        return Ok(result);
    }

    // קבלת התקדמות לפי קובץ
    [HttpGet("file/{fileId}")]
    public async Task<IActionResult> GetProgressByFileId(int fileId)
    {
        var progress = await _progressService.GetProgressByFileIdAsync(fileId);
        var result = progress.Select(p => new ProgressDto
        {
            Id = p.Id,
            FileId = p.FileId,
            Status = p.Status,
            UpdatedAt = p.UpdatedAt
        });

        return Ok(result);
    }
}