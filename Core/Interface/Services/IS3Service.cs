

using Microsoft.AspNetCore.Http;

public interface  Is3Service
    {
        public Task<string> GetDownloadUrlAsync(string fileName);

    public Task<string> UploadFileAsync(IFormFile file, string fileName);

    public  Task DeleteFileAsync(string fileKey);

    public Task<string> GeneratePresignedUrlAsync(string fileName, string contentType);

    }

