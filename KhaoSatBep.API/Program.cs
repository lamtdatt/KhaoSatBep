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
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    if (!connectionString.Contains("Minimum Pool Size"))
    {
        connectionString += ";Minimum Pool Size=2;Keepalive=30";
    }
    else
    {
        connectionString = connectionString.Replace("Minimum Pool Size=0", "Minimum Pool Size=2");
    }
}

builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseNpgsql(
        connectionString,
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

// Health check endpoint nhẹ cho warm-up từ frontend (có làm nóng DB & EF Core)
app.MapGet("/health", async (AppDbContext db) => {
    try
    {
        _ = await db.Users.AnyAsync();
        return Results.Ok(new { status = "ok", database = true, time = DateTime.UtcNow });
    }
    catch (Exception ex)
    {
        return Results.Json(new { status = "error", message = ex.Message }, statusCode: 500);
    }
});

app.MapControllers();

// ============================
// 8. AUTO MIGRATE & WARM UP DATABASE
// ============================
var applyMigrationsOnStartup = app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (applyMigrationsOnStartup)
    {
        db.Database.Migrate();
    }
}

// Chạy background warm up để làm nóng EF Core và Connection Pool ngay khi app vừa start
_ = Task.Run(async () =>
{
    try
    {
        using var warmupScope = app.Services.CreateScope();
        var db = warmupScope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.CanConnectAsync();
        _ = await db.Users.AnyAsync();
        Console.WriteLine("[Warmup] EF Core and database connection warmed up successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Warmup] Failed to warm up database: {ex.Message}");
    }
});

app.Run();