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
using AuthService.Helpers;

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

        #region GET

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

        #endregion

        #region POST

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

            // En un futuro tenemos que considerar el uso de un secreto más seguro y almacenarlo de forma segura
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this_is_a_very_secure_key_with_more_than_32_chars"));
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

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            var userDb = await _context.Users.SingleOrDefaultAsync(u => u.Email == request.Email);
            if (userDb != null)
                return Conflict("El usuario ya existe.");

            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("El nombre es obligatorio.");

            if (request.Password.Length < 8)
                return BadRequest("La contraseña debe tener al menos 8 caracteres.");

            if (IsAtLeast16YearsOld(request.Birthdate) == false)
                return BadRequest("Debes ser mayor de 16 años.");

            User user = new User
            {
                Email = request.Email,
                Password = PasswordHasher.HashPassword(request.Password),
                Name = request.Name,
                Birthdate = request.Birthdate,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al crear el usuario: " + ex.Message);
            }
        }

        #endregion

        #region Métodos privados

        private bool IsAtLeast16YearsOld(DateOnly birthDate)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var minimumBirthDate = today.AddYears(-16);

            return birthDate <= minimumBirthDate;
        }

        #endregion

    }
}
