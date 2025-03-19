using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

public class S3Service : Is3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3Service(IConfiguration configuration)
    {
        var awsOptions = configuration.GetSection("AWS");
        var accessKey = awsOptions["AccessKey"];
        var secretKey = awsOptions["SecretKey"];
        var region = awsOptions["Region"];
        _bucketName = awsOptions["BucketName"];

        _s3Client = new AmazonS3Client(accessKey, secretKey, Amazon.RegionEndpoint.GetBySystemName(region));
    }

    
    public async Task<string> GetDownloadUrlAsync(string fileName)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = fileName,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddMinutes(30) // תוקף של 30 דקות
        };
        return _s3Client.GetPreSignedURL(request);
    }

    public async Task<string> UploadFileAsync(IFormFile file, string fileName)
    {
        var transferUtility = new TransferUtility(_s3Client);

        // העלאת הקובץ לסרוויס S3
        using (var stream = file.OpenReadStream())
        {
            var uploadRequest = new TransferUtilityUploadRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                InputStream = stream,
                ContentType = file.ContentType
            };

            await transferUtility.UploadAsync(uploadRequest);
        }

        // החזרת ה-URL של הקובץ המועלה ב-S3
        return await GetDownloadUrlAsync(fileName);
    }
    public async Task DeleteFileAsync(string fileKey)
    {
        var deleteObjectRequest = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = fileKey
        };

        await _s3Client.DeleteObjectAsync(deleteObjectRequest);
    }
    public async Task<string> GeneratePresignedUrlAsync(string fileName, string contentType)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = fileName,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(10),
            ContentType = contentType
        };

        return _s3Client.GetPreSignedURL(request);
    }


}
