using KhaoSatBep.API.Data;
using KhaoSatBep.API.DTOs;
using KhaoSatBep.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace KhaoSatBep.API.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<AuthResponseDto?> DangNhap(DangNhapDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.MatKhau, user.MatKhauHash))
                return null;

            var token = TaoToken(user);
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

            var token = TaoToken(user);
            return new AuthResponseDto
            {
                Token = token,
                HoTen = user.HoTen,
                Email = user.Email,
                VaiTro = user.VaiTro
            };
        }

        private string TaoToken(User user)
        {
            var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key không được cấu hình");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.HoTen),
                new Claim(ClaimTypes.Role, user.VaiTro)
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
