using EcommerceAdmin.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// API base URL: prefer environment variable, otherwise default to the deployed Railway public domain
var adminApiBase = Environment.GetEnvironmentVariable("API_BASE_URL")
                   ?? Environment.GetEnvironmentVariable("BASE_URL")
                   ?? Environment.GetEnvironmentVariable("BaseUrl")
                   ?? "https://ecommerceapi-production-68f0.up.railway.app";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(adminApiBase) });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
