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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            //Comprobación de email no vacío
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { message = "El email no puede estar vacío." });
            }

            //Comprobación email no existente en BD
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == request.Email);
            if (user != null ) return BadRequest(new { message = "El email ya está registrado." });

            //Comprobación de contraseña no vacía
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "La contraseña no puede estar vacía." });
            }

            //Comprobación de nombre no vacío
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "El nombre no puede estar vacío." });
            }

            // Comprobación de edad mínima (16 años)
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
            var edad = hoy.Year - request.Birthdate.Year - (hoy.DayOfYear < request.Birthdate.DayOfYear ? 1 : 0); //Si aun no ha cumplido años este año se resta 1
            if (edad < 16)
            {
                return BadRequest(new { message = "Debes ser mayor de 16 años para registrarte." });
            }

            // Crear el nuevo usuario
            var newUser = new User
            {
                Email = request.Email,
                Password = PasswordHasher.HashPassword(request.Password),//Si la pass se mete hasheada el login no la detecta
                Name = request.Name,
                Birthdate = request.Birthdate,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario registrado correctamente." });
        }
    }
}
