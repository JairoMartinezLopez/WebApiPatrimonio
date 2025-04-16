using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
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
    public class UsuariosPermisosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsuariosPermisosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/UsuariosPermisos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuariosPermiso>>> GetUsuariosPermisos()
        {
            return await _context.UsuariosPermisos.ToListAsync();
        }

        // GET: api/UsuariosPermisos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UsuariosPermiso>> GetUsuariosPermiso(int id)
        {
            var usuariosPermiso = await _context.UsuariosPermisos.FindAsync(id);

            if (usuariosPermiso == null)
            {
                return NotFound();
            }

            return usuariosPermiso;
        }

        [HttpGet("filtar")]
        public async Task<ActionResult<IEnumerable<UsuariosPermiso>>> filtrarPermisos(
            [FromQuery] int? usuario,
            [FromQuery] int? permiso,
            [FromQuery] bool? otorgado)
        {
            var query = _context.UsuariosPermisos.AsQueryable();

            if (usuario.HasValue)
                query = query.Where(u => u.idUsuario == usuario);

            if (permiso.HasValue)
                query = query.Where(u => u.idPermiso == permiso);

            if (otorgado.HasValue)
                query = query.Where(u => u.Otorgado == otorgado);

            var Usuariospermisos = await query
                .Select(u => new UsuariosPermiso
                {
                    idUsuarioPermiso = u.idUsuarioPermiso,
                    idUsuario = u.idUsuario,
                    idPermiso = u.idPermiso,
                    Otorgado = u.Otorgado,
                    FechaOtorgamiento = u.FechaOtorgamiento
                })
                .ToListAsync();

            return Ok(Usuariospermisos);
        }

        // PUT: api/UsuariosPermisos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("Modificar")]
        public async Task<IActionResult> PutUsuariosPermiso(UsuariosPermiso request)
        {
            // Obtener el ID del usuario autenticado
            /*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { error = "Usuario no autenticado o ID de usuario no válido." });
            }*/

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_ModificarPermisoUsuario";

            command.Parameters.Add(new SqlParameter("@idUsuarioPermiso", request.idUsuarioPermiso));
            command.Parameters.Add(new SqlParameter("@IdPantalla", 1)); // Reemplaza con el ID de pantalla adecuado
            command.Parameters.Add(new SqlParameter("@IdGeneral", 11));//loggedInUserId)); // Usar el ID del usuario autenticado
            command.Parameters.Add(new SqlParameter("@idUsuario", request.idUsuario));
            command.Parameters.Add(new SqlParameter("@idPermiso", request.idPermiso));
            command.Parameters.Add(new SqlParameter("@Otorgado", request.Otorgado));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Permiso del usuario modificado correctamente." });
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

        // POST: api/UsuariosPermisos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UsuariosPermiso>> PostUsuariosPermiso(UsuariosPermiso request)
        {
            // Obtener el ID del usuario autenticado
            /*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { error = "Usuario no autenticado o ID de usuario no válido." });
            }*/

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = "PA_OtorgarPermisoUsuario";

            command.Parameters.Add(new SqlParameter("@IdPantalla", 1)); // Reemplaza con el ID de pantalla adecuado
            command.Parameters.Add(new SqlParameter("@IdGeneral", 11));//loggedInUserId)); // Usar el ID del usuario autenticado
            command.Parameters.Add(new SqlParameter("@idUsuario", request.idUsuario));
            command.Parameters.Add(new SqlParameter("@idPermiso", request.idPermiso));
            command.Parameters.Add(new SqlParameter("@Otorgado", request.Otorgado));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Permiso agregado al usuario correctamente." });
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

        // DELETE: api/UsuariosPermisos/5
        [HttpDelete("{idUsuarioPermiso}")]
        public async Task<IActionResult> DeleteUsuariosPermiso(int idUsuarioPermiso)
        {
            // Obtener el ID del usuario autenticado
            /*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { error = "Usuario no autenticado o ID de usuario no válido." });
            }*/

            var sql = "EXEC PA_RevocarPermisoUsuario @idUsuarioPermiso, @IdPantalla, @IdGeneral";
            var result = await _context.Database.ExecuteSqlRawAsync(sql,
                new SqlParameter("@idUsuarioPermiso", idUsuarioPermiso),
                new SqlParameter("@IdPantalla", 1),
                new SqlParameter("@IdGeneral", 1) //loggedInUserId));
            );

            return Ok(new { mensaje = "Permiso del usuario eliminado." });
        }

        private bool UsuariosPermisoExists(int id)
        {
            return _context.UsuariosPermisos.Any(e => e.idUsuarioPermiso == id);
        }
    }
}
