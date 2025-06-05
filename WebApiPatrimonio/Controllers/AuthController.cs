using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiPatrimonio.Context;
using WebApiPatrimonio.Models;

namespace WebApiPatrimonio.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || request.Usuario <= 0 || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { mensaje = "Usuario y contraseña son requeridos" });

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "PA_LOGIN_USUARIO";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@Usuario", request.Usuario));
            command.Parameters.Add(new SqlParameter("@Password", request.Password));

            try
            {
                await _context.Database.OpenConnectionAsync();

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    int resultado = Convert.ToInt32(reader["Resultado"]);
                    string mensaje = reader["Mensaje"].ToString();

                    if (resultado == 0)
                        return Unauthorized(new { mensaje });

                    // Extraer datos del usuario desde el SP
                    var idUsuario = Convert.ToInt32(reader["idUsuario"]);
                    var nombreUsuario = reader["NombreUsuario"].ToString();
                    var nombreApellidos = reader["NombreApellidos"].ToString();
                    var idGeneral = Convert.ToInt32(reader["idGeneral"]);
                    var idRol = Convert.ToInt32(reader["idRol"]);
                    var rolNombre = reader["RolNombre"].ToString();
                    var activo = Convert.ToBoolean(reader["Activo"]);

                    // Generar token JWT
                    var token = GenerarToken(idGeneral, nombreUsuario, rolNombre);

                    return Ok(new
                    {
                        token,
                        idUsuario,
                        nombreUsuario,
                        nombreApellidos,
                        idGeneral,
                        idRol,
                        rolNombre,
                        activo
                    });
                }

                return Unauthorized(new { mensaje = "No se pudo procesar el login" });
            }
            catch (SqlException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }


        private string GenerarToken(int idGeneral, string nombreUsuario, string rol)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, idGeneral.ToString()),
                new Claim(ClaimTypes.Name, nombreUsuario),
                new Claim(ClaimTypes.Role, rol)
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
