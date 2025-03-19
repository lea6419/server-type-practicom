
using Microsoft.AspNetCore.Mvc;

namespace PropertyTax.Controllers
{



    [ApiController]
    [Route("api/[controller]")]
    public class UploadFileController : Controller
    {

        //private readonly IAmazonS3 _s3Client;

        //public UploadFileController(IAmazonS3 s3Client) {
        //    _s3Client = s3Client;
        //}

        //[HttpGet("presigned-url")]
        //public async Task<IActionResult> GetPresignedUrl([FromQuery] string fileName) {
        //    var request = new GetPreSignedUrlRequest {
        //        BucketName = "propertytax-documents",
        //        Key = fileName,
        //        Verb = HttpVerb.PUT,
        //        Expires = DateTime.UtcNow.AddMinutes(5),
        //        ContentType = "image/jpeg/pdf" // או סוג הקובץ המתאים
        //    };

        //    string url = _s3Client.GetPreSignedURL(request);
        //    return Ok(new { url });
        //}
        private readonly Is3Service _s3Service;


        public UploadFileController(Is3Service s3Server)
        {
            _s3Service = s3Server;
        }

        //   שלב 1: קבלת URL להעלאת קובץ ל-S3
        [HttpGet("/upload-url")]
        public async Task<IActionResult> GetUploadUrl([FromQuery] string fileName, [FromQuery] string contentType)
        {
            Console.WriteLine("enter....");
            if (string.IsNullOrEmpty(fileName))
                return BadRequest("Missing file name");
            var url = await _s3Service.GeneratePresignedUrlAsync(fileName, contentType);
            return Ok(new { url });
        }

        //   שלב 2: קבלת URL להורדת קובץ מה-S3
        [HttpGet("download-url/{fileName}")]
        public async Task<IActionResult> GetDownloadUrl(string fileName)
        {
            var url = await _s3Service.GetDownloadUrlAsync(fileName);
            return Ok(new { downloadUrl = url });
        }


    }
}
