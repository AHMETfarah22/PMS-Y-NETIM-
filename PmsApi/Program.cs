using PmsApi.Database;
using PmsApi.Helpers;
using System.IO;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS AYARI: Dış dünyadan (web sitesinden) erişim için şart.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebsite",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "BuCokGizliVeGuvenliBirPmsAnahtaridir12345!";
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "PmsApi",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "PmsClients",
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey))
        };
    });

var app = builder.Build();

// Initialize DatabaseHelper and EmailHelper
DatabaseHelper.Initialize(app.Configuration);
EmailHelper.Initialize(app.Configuration);

// Swagger her durumda açık (Deployment sonrası test için)
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PMS Online Reservation API V1");
    c.RoutePrefix = "swagger";
});

if (Directory.Exists(Path.Combine(app.Environment.ContentRootPath, "wwwroot")))
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

app.UseCors("AllowWebsite");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
if (Directory.Exists(Path.Combine(app.Environment.ContentRootPath, "wwwroot")))
{
    app.MapFallbackToFile("index.html");
}

app.Run();
