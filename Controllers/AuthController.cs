using LibraryManagement.Objs;
using LibraryManagement.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth) 
        { 
            _auth = auth; 
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(UserAuthDto dto)
        {
            var result = await _auth.RegisterAsync(dto.Username, dto.Password);
            if (result.Data == null) return BadRequest(new { message = result.StatusMessage });

            return CreatedAtAction(null, new { result.Data.Id, result.Data.Username });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(UserAuthDto dto)
        {
            var result = await _auth.LoginAsync(dto.Username, dto.Password);
            if (string.IsNullOrWhiteSpace(result.Data)) return Unauthorized(new { message = result.StatusMessage });

            return Ok(new { result.Data });
        }
    }
}
