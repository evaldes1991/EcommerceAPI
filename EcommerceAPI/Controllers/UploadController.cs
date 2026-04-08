using Microsoft.AspNetCore.Mvc;

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

        // Use WebRootPath if available, otherwise fall back to app directory
        var webRoot = _environment.WebRootPath
                      ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploads = Path.Combine(webRoot, "uploads");
        Directory.CreateDirectory(uploads);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploads, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // BaseUrl priority: env var > appsettings > request URL
        var baseUrl = Environment.GetEnvironmentVariable("BaseUrl")?.TrimEnd('/')
                      ?? _configuration["BaseUrl"]?.TrimEnd('/')
                      ?? $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        var fileUrl = $"{baseUrl}/uploads/{fileName}";

        return Ok(new { url = fileUrl });
    }

    [HttpGet("list")]
    public IActionResult ListUploads()
    {
        var webRoot = _environment.WebRootPath
                      ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploads = Path.Combine(webRoot, "uploads");
        if (!Directory.Exists(uploads)) return Ok(Array.Empty<string>());
        var files = Directory.GetFiles(uploads).Select(Path.GetFileName).ToArray();
        return Ok(files);
    }
}
