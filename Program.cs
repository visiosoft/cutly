using Microsoft.EntityFrameworkCore;
using Cutly.Data;
using Cutly.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Cutly API", Version = "v1" });
});

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add UrlShortenerService
builder.Services.AddScoped<IUrlShortenerService, UrlShortenerService>();

// Configure JSON options
builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cutly API v1");
        c.RoutePrefix = "swagger"; // Move Swagger UI to /swagger
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Add a custom route for short URLs at the root path
app.MapGet("/{shortCode}", async (string shortCode, IUrlShortenerService urlShortenerService) =>
{
    var originalUrl = await urlShortenerService.GetOriginalUrl(shortCode);
    if (string.IsNullOrEmpty(originalUrl))
    {
        return Results.NotFound(new { error = "Short URL not found" });
    }

    await urlShortenerService.IncrementClickCount(shortCode);
    return Results.Redirect(originalUrl);
});

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
