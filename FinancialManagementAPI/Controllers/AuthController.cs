using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FinancialManagementAPI.Database;
using FinancialManagementAPI.DTOs;
using FinancialManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FinancialManagementAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [AllowAnonymous]
    public class AuthController(AppDbContext context, IConfiguration configuration) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly IConfiguration _configuration = configuration;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            if (dto.Password == "")
                return BadRequest("Senha não pode ser vazia");
            
            if (_context.Users.Any(u => u.Email == dto.Email))
                return BadRequest("Email já cadastrado");


            var newUser = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Created("", new { newUser.Id, newUser.Name, newUser.Email });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO dto) {
            var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return BadRequest("Credenciais inválidas.");
            
            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        private string GenerateJwtToken(User user) {
            
            var jwtKey = _configuration.GetSection("Jwt").ToString() ?? "";
            if(string.IsNullOrEmpty(jwtKey)) throw new NullReferenceException(nameof(jwtKey));
            
            var key = Encoding.UTF8.GetBytes(jwtKey);
            
            var claims = new List<Claim> {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Email)
            };

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
            
            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}