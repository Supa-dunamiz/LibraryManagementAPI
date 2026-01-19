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
            // result.Data contains the created user if registration succeeded.
            if (result.Data == null) return BadRequest(new { message = result.StatusMessage });

            // Return 201 Created along with the created user's minimal info.
            return CreatedAtAction(null, new { result.Data.Id, result.Data.Username });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(UserAuthDto dto)
        {
            var result = await _auth.LoginAsync(dto.Username, dto.Password);

            // If the token (Data) is empty or whitespace, authentication failed.
            if (string.IsNullOrWhiteSpace(result.Data)) return Unauthorized(new { message = result.StatusMessage });

            // Return the JWT token to the caller. The token should be used as a Bearer token for protected endpoints.
            return Ok(new { result.Data });
        }
    }
}
