using KhaoSatBep.API.DTOs;
using KhaoSatBep.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace KhaoSatBep.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>Đăng nhập và nhận JWT token</summary>
        [HttpPost("dang-nhap")]
        public async Task<IActionResult> DangNhap([FromBody] DangNhapDto dto)
        {
            var result = await _authService.DangNhap(dto);
            if (result == null)
                return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });

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
