using KhaoSatBep.API.Data;
using KhaoSatBep.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// ============================
// 0. PORT CHO RENDER
// ============================
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ============================
// 1. DATABASE (Supabase PostgreSQL)
// ============================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()));

// ============================
// 2. JWT AUTHENTICATION
// ============================
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new Exception("JWT Key chưa cấu hình!");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ============================
// 3. CORS (cho phép Vue frontend)
// ============================
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("VueFrontend", policy =>
    {
        policy.WithOrigins(
                allowedOrigins ?? new[]
                {
                    "https://khaosatbep.io.vn",
                    "https://www.khaosatbep.io.vn",
                    "https://khao-sat-bep.vercel.app"
                }
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ============================
// 4. SERVICES
// ============================
builder.Services.AddScoped<AuthService>();
builder.Services.AddControllers();

// ============================
// 5. SWAGGER
// ============================
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "KhaoSatBep API",
        Version = "v1",
        Description = "API quản lý khảo sát bếp ăn"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập JWT token theo định dạng: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ============================
// BUILD APP
// ============================
var app = builder.Build();

// ============================
// 6. SWAGGER UI
// ============================
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "KhaoSatBep API v1");
    c.RoutePrefix = "swagger";
});

// ============================
// 7. MIDDLEWARE
// ============================

// Không dùng dòng này trên Render để tránh lỗi:
// Failed to determine the https port for redirect.
// app.UseHttpsRedirection();

app.UseCors("VueFrontend");

app.UseAuthentication();
app.UseAuthorization();

// Route test để biết API sống hay chưa
app.MapGet("/", () => "KhaoSatBep API is running");

app.MapControllers();

// ============================
// 8. AUTO MIGRATE DATABASE
// ============================
var applyMigrationsOnStartup = app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup");

if (applyMigrationsOnStartup)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();