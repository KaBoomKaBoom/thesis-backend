using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scalar.AspNetCore;
using ThesisBackend.Helpers;
using ThesisBackend.Services;
using ThesisBackend.Data;
using DotNetEnv;
using Npgsql.EntityFrameworkCore.PostgreSQL; 

var builder = WebApplication.CreateBuilder(args);

// Load .env file
Env.Load();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.  
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Build a configuration object from environment variables
IConfiguration config = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

// Add this line to get the connection string from environment variables or configuration
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
    ?? throw new InvalidOperationException("DB_CONNECTION_STRING is not set in environment variables");

// Register JwtGenerator as a singleton
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
    ?? throw new InvalidOperationException("JWT_SECRET is not set in environment variables");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
    ?? throw new InvalidOperationException("JWT_ISSUER is not set in environment variables");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
    ?? throw new InvalidOperationException("JWT_AUDIENCE is not set in environment variables");

builder.Services.AddSingleton(new JwtGenerator(jwtSecret, jwtIssuer, jwtAudience));

// Register UserService
builder.Services.AddScoped<UserService>();

// Register DbContext (uncomment and configure when ready)
builder.Services.AddDbContext<UserContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddCors((options) =>
{
    options.AddPolicy("DevCors", (corsBuilder) =>
    {
        corsBuilder.WithOrigins("http://localhost:32768", "http://localhost:3000", "http://localhost:8000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
    options.AddPolicy("ProdCors", (corsBuilder) =>
    {
        corsBuilder.WithOrigins("https://myProductionSite.com")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.WebHost.UseUrls("http://0.0.0.0:8080");

var app = builder.Build();

// Get logger instance
var logger = app.Services.GetRequiredService<ILogger<Program>>();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference("/docs" ,options =>
    {
        options.WithTitle("API Documentation For Thesis Bacend");
    });
    app.UseCors("DevCors");
}
else
{
    app.UseCors("ProdCors");
}

// Adds the /openapi/{documentName}.json endpoint
app.MapOpenApi();

app.MapControllers();

app.Run();