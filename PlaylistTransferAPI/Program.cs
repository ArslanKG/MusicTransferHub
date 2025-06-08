using DotNetEnv;
using PlaylistTransferAPI.Services;
using PlaylistTransferAPI.Services.Interfaces;
using Serilog;
using System.Threading.RateLimiting;

// .env dosyasını yükle (varsa)
if (File.Exists(".env"))
{
    Env.Load();
}

var builder = WebApplication.CreateBuilder(args);

// Environment değişkenlerini configuration'a ekle
builder.Configuration.AddEnvironmentVariables();

// Environment değişkenlerini configuration section'larına map et
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["Spotify:ClientId"] = Environment.GetEnvironmentVariable("Spotify__ClientId") ?? Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID") ?? "",
    ["Spotify:ClientSecret"] = Environment.GetEnvironmentVariable("Spotify__ClientSecret") ?? Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET") ?? "",
    ["YouTube:ClientId"] = Environment.GetEnvironmentVariable("YouTube__ClientId") ?? Environment.GetEnvironmentVariable("YOUTUBE_CLIENT_ID") ?? "",
    ["YouTube:ClientSecret"] = Environment.GetEnvironmentVariable("YouTube__ClientSecret") ?? Environment.GetEnvironmentVariable("YOUTUBE_CLIENT_SECRET") ?? "",
    ["YouTube:ApiKey"] = Environment.GetEnvironmentVariable("YouTube__ApiKey") ?? Environment.GetEnvironmentVariable("YOUTUBE_API_KEY") ?? "",
    ["Production:Domain"] = Environment.GetEnvironmentVariable("DOMAIN") ?? "yourdomain.com"
});

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/playlist-transfer-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "TuneSync - Playlist Transfer API",
        Version = "v1",
        Description = "Spotify to YouTube Music Playlist Transfer API - Stateless & Simple",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Arslan Kemal Gündüz",
            Url = new Uri("https://arkegu-portfolio.vercel.app/")
        }
    });
});

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

builder.Services.AddScoped<ISpotifyService, SpotifyService>();
builder.Services.AddScoped<IYouTubeService, YouTubeService>();
builder.Services.AddScoped<ITransferService, TransferService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".PlaylistTransfer.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Playlist Transfer API v1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    // Production security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        await next();
    });
    
    // Global exception handler for production
    app.UseExceptionHandler("/error");
}

Log.Information("Starting stateless playlist transfer service");

app.UseSerilogRequestLogging();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseSession();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/error", () => Results.Problem("An error occurred"));
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));
app.MapFallbackToFile("index.html");

try
{
    Log.Information("Starting Playlist Transfer API on {Environment}", app.Environment.EnvironmentName);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
