using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebApiPatrimonio.Context;
using WebApiPatrimonio.Models;

namespace WebApiPatrimonio.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration; // Para leer claves del appsettings.json

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || request.Usuario <= 0 || string.IsNullOrEmpty(request.Password))
                return BadRequest("Usuario y contraseña son requeridos");

            // Obtener el usuario de la BD
            var usuario = await _context.USUARIOS.FirstOrDefaultAsync(u => u.idGeneral == request.Usuario);
            if (usuario == null)
                return Unauthorized("Usuario o contraseña incorrectos");

            // Verificar la contraseña cifrada con HASHBYTES
            if (!VerificarPassword(request.Password, usuario.Password))
                return Unauthorized("Usuario o contraseña incorrectos");

            // Generar token JWT
            var token = GenerarToken(usuario);

            // Devolver los datos esenciales del usuario
            return Ok(new
            {
                token,
                usuario.idUsuarios,
                usuario.Nombre,
                usuario.Apellidos,
                usuario.idGeneral,
                usuario.Rol,
                usuario.Activo
            });
        }

        // Método para verificar la contraseña cifrada con HASHBYTES
        private bool VerificarPassword(string passwordIngresado, byte[] passwordAlmacenado)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] passwordIngresadoHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(passwordIngresado));
                return passwordAlmacenado.SequenceEqual(passwordIngresadoHash);
            }
        }

        // Método para generar el token JWT
        private string GenerarToken(UsuarioModel usuario)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.idGeneral.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
