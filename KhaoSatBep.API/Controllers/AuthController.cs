using KhaoSatBep.API.DTOs;
using KhaoSatBep.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace KhaoSatBep.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>Đăng nhập và nhận JWT token</summary>
        [HttpPost("dang-nhap")]
        public async Task<IActionResult> DangNhap([FromBody] DangNhapDto dto)
        {
            var timer = Stopwatch.StartNew();
            _logger.LogInformation("Auth login request received for {Email}", dto.Email);

            var result = await _authService.DangNhap(dto);
            if (result == null)
            {
                _logger.LogWarning("Auth login request unauthorized in {ElapsedMs}ms for {Email}", timer.ElapsedMilliseconds, dto.Email);
                return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });
            }

            _logger.LogInformation("Auth login response returned in {ElapsedMs}ms for {Email}", timer.ElapsedMilliseconds, dto.Email);
            return Ok(result);
        }

        /// <summary>Đăng ký tài khoản mới</summary>
        [HttpPost("dang-ky")]
        public async Task<IActionResult> DangKy([FromBody] DangKyDto dto)
        {
            var result = await _authService.DangKy(dto);
            if (result == null)
                return BadRequest(new { message = "Email đã được sử dụng" });

            return Ok(result);
        }
    }
}
