using Microsoft.AspNetCore.Mvc;
// S3 support removed per request; keep only local storage handling

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

        // Store file in wwwroot/uploads
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
