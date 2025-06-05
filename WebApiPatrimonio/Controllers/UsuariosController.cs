using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebApiPatrimonio.Context;
using WebApiPatrimonio.Models;

namespace WebApiPatrimonio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Usuarios
        [HttpGet("Obtener todo")]
        public async Task<ActionResult<IEnumerable<UsuarioModel>>> GetUsuarios()
        {
            var usuarios = await _context.USUARIOS
                .Select(u => new UsuarioModel
                {
                    idUsuario = u.idUsuario,
                    Nombre = u.Nombre,
                    Apellidos = u.Apellidos,
                    idGeneral = u.idGeneral,
                    idRol = u.idRol,
                    Activo = u.Activo,
                    Bloqueado = u.Bloqueado
                })
                .ToListAsync();
            return Ok(usuarios);
        }

        // GET: api/Usuarios/5
        [HttpGet("{idUsuario}")]
        public async Task<ActionResult<UsuarioModel>> GetUsuarioRequest(int idUsuario)
        {
            var usuario = await _context.USUARIOS
                .Where(u => u.idUsuario == idUsuario)
                .Select(u => new UsuarioModel
                {
                    idUsuario = u.idUsuario,
                    Nombre = u.Nombre,
                    Apellidos = u.Apellidos,
                    idGeneral = u.idGeneral,
                    idRol = u.idRol,
                    Activo = u.Activo,
                    Bloqueado = u.Bloqueado
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return NotFound();
            }

            return Ok(usuario);
        }


        [HttpGet("filtrar")]
        public async Task<ActionResult<IEnumerable<UsuarioModel>>> FiltrarUsuarios(
            [FromQuery] string? nombre,
            [FromQuery] int? idGeneral,
            [FromQuery] int? idRol,
            [FromQuery] bool? activo,
            [FromQuery] bool? bloqueado)
        {
            var query = _context.USUARIOS.AsQueryable();

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(u => u.Nombre.Contains(nombre));

            if (idGeneral.HasValue)
                query = query.Where(u => u.idGeneral == idGeneral);

            if (idRol.HasValue)
                query = query.Where(u => u.idRol == idRol);

            if (activo.HasValue)
                query = query.Where(u => u.Activo == activo);

            if (bloqueado.HasValue)
                query = query.Where(u => u.Bloqueado == bloqueado);

            var usuarios = await query
                .Select(u => new UsuarioModel
                {
                    idUsuario = u.idUsuario,
                    Nombre = u.Nombre,
                    Apellidos = u.Apellidos,
                    idGeneral = u.idGeneral,
                    idRol = u.idRol,
                    Activo = u.Activo,
                    Bloqueado = u.Bloqueado
                })
                .ToListAsync();

            return Ok(usuarios);
        }


        // PUT: api/Usuarios/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("modificar")]
        public async Task<IActionResult> ModificarUsuario([FromBody] UsuarioModel request)
        {
            // Obtener el ID del usuario autenticado
            /*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { error = "Usuario no autenticado o ID de usuario no válido." });
            }*/

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "PA_UPD_USUARIOS";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@idUsuario", request.idUsuario));
            command.Parameters.Add(new SqlParameter("@IdPantalla", 1));
            command.Parameters.Add(new SqlParameter("@IdGeneral", request.IdGeneralUsuario));
            command.Parameters.Add(new SqlParameter("@Nombre", request.Nombre));
            command.Parameters.Add(new SqlParameter("@Apellidos", request.Apellidos));
            command.Parameters.Add(new SqlParameter("@idRol", request.idRol));
            command.Parameters.Add(new SqlParameter("@Activo", request.Activo));
            command.Parameters.Add(new SqlParameter("@Bloqueado", request.Bloqueado));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Usuario modificado correctamente." });
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


        // POST: api/Usuarios
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

        [HttpPost("Agregar Usuario")]
        public async Task<IActionResult> InsertarUsuario([FromBody] UsuarioModel request)
        {
            // Obtener el ID del usuario autenticado
            /*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { error = "Usuario no autenticado o ID de usuario no válido." });
            }*/

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "PA_INS_USUARIOS";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@IdPantalla", 1));
            command.Parameters.Add(new SqlParameter("@IdGeneral",request.IdGeneralUsuario));
            command.Parameters.Add(new SqlParameter("@Nombre", request.Nombre));
            command.Parameters.Add(new SqlParameter("@Apellidos", request.Apellidos));
            command.Parameters.Add(new SqlParameter("@Password", request.Password));
            command.Parameters.Add(new SqlParameter("@idGeneralUsuario", request.idGeneral));
            command.Parameters.Add(new SqlParameter("@idRol", request.idRol));
            command.Parameters.Add(new SqlParameter("@Activo", request.Activo));
            command.Parameters.Add(new SqlParameter("@Bloqueado", request.Bloqueado));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Usuario insertado correctamente." });
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


        // DELETE: api/Usuarios/5
        [HttpDelete]
        public async Task<IActionResult> EliminarUsuario([FromBody] UsuarioModel request)
        {
        
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "PA_DEL_USUARIO";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@idUsuario", request.idUsuario));
            command.Parameters.Add(new SqlParameter("@IdPantalla", 1));
            command.Parameters.Add(new SqlParameter("@IdGeneral", request.idUsuario));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();

                return Ok(new { mensaje = "Usuario desactivado correctamente." });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { error = "Error al desactivar el usuario.", detalle = ex.Message });
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        [HttpPut("ResetPasswordSP/{idUsuario}")]
        public async Task<IActionResult> ResetPasswordSP(int idUsuario)
        {
          
            var nuevaPassword = GenerarContraseña();

            var parametros = new[]
            {
                new SqlParameter("@idUsuario", idUsuario),
                new SqlParameter("@NuevaPassword", nuevaPassword),
                new SqlParameter("@IdGeneral", 1), //loggedInUserId));
                new SqlParameter("@IdPantalla", 1)
            };

            try
            {
                await _context.Database.ExecuteSqlRawAsync("EXEC PA_RESET_PASSWORD_USUARIO @idUsuario, @NuevaPassword, @IdGeneral, @IdPantalla", parametros);
            }
            catch (SqlException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }

            return Ok(new
            {
                Mensaje = "Contraseña restablecida exitosamente.",
                UsuarioId = idUsuario,
                NuevaContraseña = nuevaPassword
            });
        }

        private string GenerarContraseña(int longitud = 10)
        {
            const string caracteres = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%&";
            var random = new Random();
            return new string(Enumerable.Repeat(caracteres, longitud)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [HttpPut("CambiarPassword")]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPassword request)
        {
            // Obtener el ID del usuario autenticado
            /*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { error = "Usuario no autenticado o ID de usuario no válido." });
            }*/
            var parametros = new[]
            {
                new SqlParameter("@idUsuario", request.Usuario),
                new SqlParameter("@PasswordActual", request.PasswordActual),
                new SqlParameter("@NuevaPassword", request.NuevaPassword),
                new SqlParameter("@IdGeneral", 1), //loggedInUserId));
                new SqlParameter("@IdPantalla", 1)
            };

            try
            {
                await _context.Database.ExecuteSqlRawAsync("EXEC PA_CAMBIAR_PASSWORD_USUARIO @idUsuario, @PasswordActual, @NuevaPassword, @IdGeneral, @IdPantalla", parametros);
            }
            catch (SqlException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }

            return Ok(new { mensaje = "Contraseña actualizada correctamente." });
        }


        private bool USUARIOSExists(int id)
        {
            return _context.USUARIOS.Any(e => e.idUsuario == id);
        }
    }
}
