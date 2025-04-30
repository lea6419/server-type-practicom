using Amazon.S3;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

[ApiController]
[Route("api/ocr")]
public class OcrController : ControllerBase
{
    private readonly IConfiguration _config;
    public OcrController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet("from-s3")]
    public async Task<IActionResult> OcrFromS3(string fileKey)
    {
        // 1. הורדה מ־S3
        var s3Client = new AmazonS3Client();
        var s3Bucket = _config["AWS:BucketName"];
        var s3Object = await s3Client.GetObjectAsync(s3Bucket, fileKey);
        using var stream = s3Object.ResponseStream;
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        var fileBytes = ms.ToArray();

        // 2. קריאה ל־HuggingFace
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "hf_xxx");

        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg"); // או application/pdf אם PDF
        content.Add(fileContent, "file", "scan.jpg");

        var hfModelUrl = "https://api-inference.huggingface.co/models/microsoft/trocr-base-handwritten";
        var response = await httpClient.PostAsync(hfModelUrl, content);
        var responseText = await response.Content.ReadAsStringAsync();

        // 3. שליפת טקסט
        var json = JsonDocument.Parse(responseText);
        var text = json.RootElement.GetProperty("generated_text").GetString();

        // 4. ספירת מילים
        var wordCount = text.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;

        return Ok(new { text, wordCount });
    }
}
