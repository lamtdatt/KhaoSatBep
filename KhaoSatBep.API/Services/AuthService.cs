using KhaoSatBep.API.Data;
using KhaoSatBep.API.DTOs;
using KhaoSatBep.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace KhaoSatBep.API.Services
{
    public class AuthService
    {
        private sealed class LoginUser
        {
            public int Id { get; init; }
            public string HoTen { get; init; } = string.Empty;
            public string Email { get; init; } = string.Empty;
            public string VaiTro { get; init; } = string.Empty;
            public string MatKhauHash { get; init; } = string.Empty;
        }

        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext context, IConfiguration config, ILogger<AuthService> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        public async Task<AuthResponseDto?> DangNhap(DangNhapDto dto)
        {
            var totalTimer = Stopwatch.StartNew();
            var stepTimer = Stopwatch.StartNew();
            var email = dto.Email.Trim();

            _logger.LogInformation("Auth login start for {Email}", email);

            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.Email == email)
                .Select(u => new LoginUser
                {
                    Id = u.Id,
                    HoTen = u.HoTen,
                    Email = u.Email,
                    VaiTro = u.VaiTro,
                    MatKhauHash = u.MatKhauHash
                })
                .FirstOrDefaultAsync();

            _logger.LogInformation("Auth login query user took {ElapsedMs}ms for {Email}", stepTimer.ElapsedMilliseconds, email);

            stepTimer.Restart();
            var passwordOk = user != null && BCrypt.Net.BCrypt.Verify(dto.MatKhau, user.MatKhauHash);
            _logger.LogInformation("Auth login password verify took {ElapsedMs}ms for {Email}", stepTimer.ElapsedMilliseconds, email);

            if (user == null || !passwordOk)
            {
                _logger.LogWarning("Auth login failed in {ElapsedMs}ms for {Email}", totalTimer.ElapsedMilliseconds, email);
                return null;
            }

            stepTimer.Restart();
            var token = TaoToken(user.Id, user.Email, user.HoTen, user.VaiTro);
            _logger.LogInformation("Auth login token creation took {ElapsedMs}ms for {Email}", stepTimer.ElapsedMilliseconds, email);
            _logger.LogInformation("Auth login success in {ElapsedMs}ms for {Email}", totalTimer.ElapsedMilliseconds, email);

            return new AuthResponseDto
            {
                Token = token,
                HoTen = user.HoTen,
                Email = user.Email,
                VaiTro = user.VaiTro
            };
        }

        public async Task<AuthResponseDto?> DangKy(DangKyDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return null;

            var user = new User
            {
                HoTen = dto.HoTen,
                Email = dto.Email,
                MatKhauHash = BCrypt.Net.BCrypt.HashPassword(dto.MatKhau),
                VaiTro = "NguoiDung"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = TaoToken(user.Id, user.Email, user.HoTen, user.VaiTro);
            return new AuthResponseDto
            {
                Token = token,
                HoTen = user.HoTen,
                Email = user.Email,
                VaiTro = user.VaiTro
            };
        }

        private string TaoToken(int userId, string email, string hoTen, string vaiTro)
        {
            var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key không được cấu hình");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, hoTen),
                new Claim(ClaimTypes.Role, vaiTro)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
