using Microsoft.EntityFrameworkCore;
using Cutly.Data;
using Cutly.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Cutly API", Version = "v1" });
});

// Add HttpClient
builder.Services.AddHttpClient("ServerAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BaseUrl"] ?? "https://localhost:5269/");
});
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ServerAPI"));

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add UrlShortenerService
builder.Services.AddScoped<IUrlShortenerService, UrlShortenerService>();

// Add UserService as a singleton
builder.Services.AddSingleton<UserService>();

// Configure JSON options
builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cutly API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapControllers();

// Add a custom route for short URLs
app.MapGet("/s/{shortCode}", async (HttpContext context, string shortCode, IUrlShortenerService urlShortenerService) =>
{
    var originalUrl = await urlShortenerService.GetOriginalUrl(shortCode);
    if (string.IsNullOrEmpty(originalUrl))
    {
        return Results.NotFound(new { error = "Short URL not found" });
    }

    // Get the visitor's IP address
    var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    if (urlShortenerService is Cutly.Services.UrlShortenerService concreteService)
    {
        await concreteService.IncrementClickCountAndTrackIp(shortCode, ipAddress);
    }
    else
    {
        await urlShortenerService.IncrementClickCount(shortCode);
    }
    return Results.Redirect(originalUrl);
});

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
