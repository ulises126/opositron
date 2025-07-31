using AuthService.Data;
using AuthService.Dtos;
using AuthService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using System;
using System.Security.Claims;
using System.Text;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AuthDbContext _context;

        public UserController(AuthDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        [HttpGet("id/{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            return user;
        }

        [HttpGet("{email}")]
        public async Task<ActionResult<User>> GetUser(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound();

            return user;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || !PasswordHasher.VerifyPassword(request.Password, user.Password))
                return Unauthorized();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("clave-secreta-muy-segura"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = "AuthService",
                SigningCredentials = creds
            };

            var handler = new JsonWebTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);

            return Ok(new { token });
        }
    }
}
