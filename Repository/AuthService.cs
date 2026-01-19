using LibraryManagement.Data;
using LibraryManagement.Entities;
using LibraryManagement.Helper;
using LibraryManagement.Objs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;

namespace LibraryManagement.Repository
{
    public interface IAuthService
    {
        Task<OperationResult<User?>> RegisterAsync(string username, string password);
        Task<OperationResult<string>> LoginAsync(string username, string password);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _ctx;
        private readonly IConfiguration _config;
        private readonly Microsoft.AspNetCore.Identity.PasswordHasher<User> _hasher;
        private readonly ILogger<AuthService> _logger;


        public AuthService(AppDbContext ctx, IConfiguration config, ILogger<AuthService> logger)
        {
            _ctx = ctx;
            _config = config;
            _hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
            _logger = logger;
        }

        public async Task<OperationResult<User?>> RegisterAsync(string username, string password)
        {
            try
            {
                if (_ctx.Users.Any(u => u.Username == username))
                {
                    return new OperationResult<User?>
                    {
                        StatusMessage = "User with the specified username already exists.",
                        Data = null
                    };
                }

                var user = new User { Username = username };
                user.PasswordHash = _hasher.HashPassword(user, password);

                _ctx.Users.Add(user);
                await _ctx.SaveChangesAsync();

                return new OperationResult<User?>
                {
                    StatusMessage = "Registration successful.",
                    Data = user
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred");
                return new OperationResult<User?>
                {
                    StatusMessage = "Exception occurred",
                    Data = null
                };
            }
        }

        public async Task<OperationResult<string>> LoginAsync(string username, string password)
        {
            if(string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return new OperationResult<string>
                {
                    StatusMessage = "Invalid parameters supplied",
                    Data = string.Empty
                };
            }
            try
            {
                var user = await _ctx.Users.SingleOrDefaultAsync(u => u.Username == username);

                if (user == null)
                {
                    return new OperationResult<string>
                    {
                        StatusMessage = "Authentication failed. Invalid username or password.",
                        Data = string.Empty
                    };
                }

                var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);

                if (result == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
                {
                    return new OperationResult<string>
                    {
                        StatusMessage = "Authentication failed. Invalid username or password.",
                        Data = string.Empty
                    };
                }

                // Create JWT
                var jwtSettings = _config.GetSection("Jwt");

                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.GetValue<string>("Key"))
                );

                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                    new Claim("id", user.Id.ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: jwtSettings.GetValue<string>("Issuer"),
                    audience: jwtSettings.GetValue<string>("Audience"),
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(jwtSettings.GetValue<int>("DurationMinutes")),
                    signingCredentials: creds
                );

                var jwt = new JwtSecurityTokenHandler().WriteToken(token);

                return new OperationResult<string>
                {
                    StatusMessage = "Authentication successful.",
                    Data = jwt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred");
                return new OperationResult<string>
                {
                    StatusMessage = "Exception occurred",
                    Data = string.Empty
                };
            }

        }
    }
}
