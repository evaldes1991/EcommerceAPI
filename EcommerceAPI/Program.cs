using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using EcommerceAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// DbContext configuration: use PostgreSQL (Railway) if DATABASE_URL is set, otherwise SQLite (local dev)
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    // Railway provides DATABASE_URL as: postgresql://user:pass@host:port/dbname
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    var npgsqlConn = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(npgsqlConn));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Controllers
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Build CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not found");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Ensure wwwroot/uploads exists (important for Docker containers)
var webRootPath = app.Environment.WebRootPath
                  ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
var uploadsPath = Path.Combine(webRootPath, "uploads");
Directory.CreateDirectory(uploadsPath);

// If WebRootPath was null, set it so static files middleware works
if (app.Environment.WebRootPath == null)
{
    app.Environment.WebRootPath = webRootPath;
}

app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// If running in a hosting environment like Railway, the platform will provide a PORT env var.
// Bind to that port and avoid forcing HTTPS redirection (platform typically terminates TLS).
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    app.Urls.Add($"http://*:{port}");
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Ensure database is migrated on startup
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
        Console.WriteLine($"Database provider: {db.Database.ProviderName}");

        // Fix old image URLs that point to local IP addresses
        var newBase = Environment.GetEnvironmentVariable("API_BASE_URL")
                      ?? Environment.GetEnvironmentVariable("BASE_URL")
                      ?? Environment.GetEnvironmentVariable("BaseUrl")
                      ?? builder.Configuration["API_BASE_URL"]
                      ?? builder.Configuration["BASE_URL"]
                      ?? builder.Configuration["BaseUrl"]
                      ?? "";
        if (!string.IsNullOrEmpty(newBase))
        {
            newBase = newBase.TrimEnd('/');

            // Update Products.ImageUrl
            var products = db.Products.Where(p => p.ImageUrl != null && p.ImageUrl != "" && !p.ImageUrl.StartsWith(newBase)).ToList();
            foreach (var p in products)
            {
                // Extract just the /uploads/filename.ext part
                var idx = p.ImageUrl!.IndexOf("/uploads/");
                if (idx >= 0)
                {
                    p.ImageUrl = newBase + p.ImageUrl.Substring(idx);
                }
            }

            // Update ProductImages.Url
            var images = db.ProductImages.Where(i => i.Url != null && i.Url != "" && !i.Url.StartsWith(newBase)).ToList();
            foreach (var img in images)
            {
                var idx = img.Url!.IndexOf("/uploads/");
                if (idx >= 0)
                {
                    img.Url = newBase + img.Url.Substring(idx);
                }
            }

            if (products.Any() || images.Any())
            {
                db.SaveChanges();
                Console.WriteLine($"Updated {products.Count} product URLs and {images.Count} image URLs to use {newBase}");
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Startup error: {ex.Message}");
}

app.Run();
