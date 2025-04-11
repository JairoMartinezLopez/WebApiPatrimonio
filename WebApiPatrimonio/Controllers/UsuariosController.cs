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
        public async Task<ActionResult<IEnumerable<UsuarioRequest>>> GetUsuarios()
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
        public async Task<IActionResult> ModificarUsuario([FromBody] UsuarioRequest request)
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
            command.Parameters.Add(new SqlParameter("@idGeneral", 1)); //loggedInUserId));
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
        public async Task<IActionResult> InsertarUsuario([FromBody] UsuarioRequest request)
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
            command.Parameters.Add(new SqlParameter("@IdGeneralUsuario", 1));//loggedInUserId));
            command.Parameters.Add(new SqlParameter("@Nombre", request.Nombre));
            command.Parameters.Add(new SqlParameter("@Apellidos", request.Apellidos));
            command.Parameters.Add(new SqlParameter("@Password", request.Password));
            command.Parameters.Add(new SqlParameter("@idGeneral", request.idGeneral));
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
        public async Task<IActionResult> EliminarUsuario([FromBody] UsuarioRequest request)
        {
            // Obtener el ID del usuario autenticado
            /*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { error = "Usuario no autenticado o ID de usuario no válido." });
            }*/

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "PA_DEL_USUARIO";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@idUsuario", request.idUsuario));
            command.Parameters.Add(new SqlParameter("@IdPantalla", 1));
            command.Parameters.Add(new SqlParameter("@idGeneral", 1));//loggedInUserId));

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


        private bool UsuarioRequestExists(int id)
        {
            return _context.UsuariosRequest.Any(e => e.idUsuario == id);
        }
    }
}
