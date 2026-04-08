using Microsoft.AspNetCore.Mvc;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon;

namespace EcommerceAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public UploadController(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

        // Prefer S3 if credentials and bucket are configured
        var s3Bucket = _configuration["S3:BucketName"];
        var s3Region = _configuration["S3:Region"]; // e.g. us-east-1

        if (!string.IsNullOrWhiteSpace(s3Bucket) && !string.IsNullOrWhiteSpace(s3Region))
        {
            var accessKey = _configuration["S3:AccessKeyId"];
            var secretKey = _configuration["S3:SecretAccessKey"];

            var config = new AmazonS3Config { RegionEndpoint = RegionEndpoint.GetBySystemName(s3Region) };
            using var client = new AmazonS3Client(accessKey, secretKey, config);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = file.OpenReadStream(),
                Key = $"uploads/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}",
                BucketName = s3Bucket,
                CannedACL = S3CannedACL.PublicRead
            };

            var fileKey = uploadRequest.Key;
            var transferUtility = new TransferUtility(client);
            await transferUtility.UploadAsync(uploadRequest);

            // Construct public URL for S3 (works for AWS S3; other providers may vary)
            var fileUrl = $"https://{s3Bucket}.s3.{s3Region}.amazonaws.com/{fileKey}";
            return Ok(new { url = fileUrl });
        }

        // Fallback to local storage in wwwroot/uploads
        var uploads = Path.Combine(_environment.WebRootPath, "uploads");
        if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploads, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Use configured BaseUrl (should be https public URL)
        var baseUrl = _configuration["BaseUrl"]?.TrimEnd('/')
                      ?? $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        var fileUrlLocal = $"{baseUrl}/uploads/{fileName}";

        return Ok(new { url = fileUrlLocal });
    }
}
