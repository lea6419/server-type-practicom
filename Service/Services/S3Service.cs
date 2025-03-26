using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

public class S3Service : Is3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly ILogger<S3Service> _logger;

    public S3Service(IConfiguration configuration, ILogger<S3Service> logger)
    {
        _logger = logger;
        var awsOptions = configuration.GetSection("AWS");
        var accessKey = awsOptions["AccessKey"];
        var secretKey = awsOptions["SecretKey"];
        var region = awsOptions["Region"];
        _bucketName = awsOptions["BucketName"];

        _s3Client = new AmazonS3Client(accessKey, secretKey, Amazon.RegionEndpoint.GetBySystemName(region));
        _logger.LogInformation("S3Service initialized with bucket: {BucketName}", _bucketName);
    }

    public async Task<string> GetDownloadUrlAsync(string fileName)
    {
        _logger.LogInformation("Generating download URL for file: {FileName}", fileName);
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(30)
            };

            var url = _s3Client.GetPreSignedURL(request);
            _logger.LogInformation("Generated URL: {Url}", url);
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating download URL for file: {FileName}", fileName);
            throw;
        }
    }

    public async Task<string> UploadFileAsync(IFormFile file, string fileName)
    {
        _logger.LogInformation("Uploading file: {FileName}", fileName);
        var transferUtility = new TransferUtility(_s3Client);

        try
        {
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
                _logger.LogInformation("File uploaded successfully: {FileName}", fileName);
            }

            return await GetDownloadUrlAsync(fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", fileName);
            throw;
        }
    }

    public async Task DeleteFileAsync(string fileKey)
    {
        _logger.LogInformation("Deleting file: {FileKey}", fileKey);
        try
        {
            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = fileKey
            };

            await _s3Client.DeleteObjectAsync(deleteObjectRequest);
            _logger.LogInformation("File deleted successfully: {FileKey}", fileKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileKey}", fileKey);
            throw;
        }

    }
    public async Task<string> GeneratePresignedUrlAsync(string fileName, string contentType)
    {
        _logger.LogInformation("Generating presigned URL for file: {FileName}", fileName);
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                Verb = HttpVerb.PUT, // PUT כדי לאפשר העלאת קובץ
                Expires = DateTime.UtcNow.AddMinutes(10), // תוקף ה-URL
                ContentType = contentType // סוג התוכן
            };

            var url = _s3Client.GetPreSignedURL(request);
            _logger.LogInformation("Generated presigned URL: {Url}", url);
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL for file: {FileName}", fileName);
            throw;
        }
    }

    public async Task<Stream> GetFileStreamAsync(string fileName)
    {
        try
        {
            // תחילת קוד לזיהוי שם הקובץ. במציאות תוכל לאחזר את שם הקובץ מתוך DB או פרמטר אחר
            var fileKey = $"your-file-key/{fileName}.pdf"; // שם הקובץ ב-S3 (או מקש כמו ID שמפנה לקובץ)

            // הורדת הקובץ מ-S3
            var getObjectResponse = await _s3Client.GetObjectAsync(new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = fileKey
            });

            return getObjectResponse.ResponseStream;
        }
        catch (Exception ex)
        {
            // טיפול בשגיאות במקרה של בעיה בהורדת הקובץ
            throw new Exception($"Error retrieving file with ID {fileName} from S3", ex);
        }
    }
}


