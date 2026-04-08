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

        var uploads = Path.Combine(_environment.WebRootPath, "uploads");
        if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploads, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Use configured BaseUrl (LAN IP) so Android devices can reach the image
        var baseUrl = _configuration["BaseUrl"]?.TrimEnd('/')
                      ?? $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        var fileUrl = $"{baseUrl}/uploads/{fileName}";

        return Ok(new { url = fileUrl });
    }
}
