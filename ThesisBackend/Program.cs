using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Scalar.AspNetCore;
using ThesisBackend.Data;
using ThesisBackend.Helpers;
using ThesisBackend.Services;

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

// Redis configuration
var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING") 
    ?? "localhost:6379";

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "ThesisBackend_";
});

// Register JwtGenerator as a singleton
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
    ?? throw new InvalidOperationException("JWT_SECRET is not set in environment variables");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
    ?? throw new InvalidOperationException("JWT_ISSUER is not set in environment variables");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
    ?? throw new InvalidOperationException("JWT_AUDIENCE is not set in environment variables");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
            };
        });

builder.Services.AddSingleton(new JwtHelper(jwtSecret, jwtIssuer, jwtAudience));

builder.Services.AddAuthorization();

// Register services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EmailService>(serviceProvider =>
{
    var smtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER") 
        ?? throw new InvalidOperationException("SMTP_SERVER is not set");
    var smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
    var senderEmail = Environment.GetEnvironmentVariable("SENDER_EMAIL") 
        ?? throw new InvalidOperationException("SENDER_EMAIL is not set");
    var senderName = Environment.GetEnvironmentVariable("SENDER_NAME") ?? "Thesis Backend";
    var senderPassword = Environment.GetEnvironmentVariable("SENDER_PASSWORD") 
        ?? throw new InvalidOperationException("SENDER_PASSWORD is not set");
    
    return new EmailService(smtpServer, smtpPort, senderEmail, senderName, senderPassword);
});
builder.Services.AddScoped<OTPService>();
builder.Services.AddSingleton<OTPGenerator>();

// Register DbContext
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

logger.LogInformation($"Using connection string: {connectionString}");

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference("/docs" ,options =>
    {
        options.WithTitle("API Documentation For Thesis Backend");
    });
    app.UseCors("DevCors");
}
else
{
    app.UseCors("ProdCors");
}

// Adds the /openapi/{documentName}.json endpoint
app.MapOpenApi();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();