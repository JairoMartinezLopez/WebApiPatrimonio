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
    public class PermisosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PermisosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Permisos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Permiso>>> GetPERMISOS()
        {
            return await _context.PERMISOS.ToListAsync();
        }

        // GET: api/Permisos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Permiso>> GetPermiso(int id)
        {
            var permiso = await _context.PERMISOS.FindAsync(id);

            if (permiso == null)
            {
                return NotFound();
            }

            return permiso;
        }

        [HttpGet("filtar")]
        public async Task<ActionResult<IEnumerable<Permiso>>> filtrarPermisos(
            [FromQuery] string? nombre,
            [FromQuery] string? descripcion,
            [FromQuery] string? clave,
            [FromQuery] int? modulo,
            [FromQuery] int? accion,
            [FromQuery] bool? activo,
            [FromQuery] bool? bloqueado)
        {
            var query = _context.PERMISOS.AsQueryable();

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(u => u.Nombre.Contains(nombre));

            if (!string.IsNullOrEmpty(descripcion))
                query = query.Where(u => u.Descripcion.Contains(descripcion));

            if (!string.IsNullOrEmpty(clave))
                query = query.Where(u => u.Clave.Contains(clave));

            if (modulo.HasValue)
                query = query.Where(u => u.idModulo == modulo);

            if (accion.HasValue)
                query = query.Where(u => u.idAccion == accion);

            if (activo.HasValue)
                query = query.Where(u => u.Activo == activo);

            if (bloqueado.HasValue)
                query = query.Where(u => u.Bloqueado == bloqueado);

            var permisos = await query
                .Select(u => new Permiso
                {
                    idPermiso = u.idPermiso,
                    Clave = u.Clave,
                    Nombre = u.Nombre,
                    Descripcion = u.Descripcion,
                    idModulo = u.idModulo,
                    idAccion = u.idAccion,
                    Activo = u.Activo,
                    Bloqueado = u.Bloqueado
                })
                .ToListAsync();

            return Ok(permisos);
        }

        // PUT: api/Permisos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("modificar")]
        public async Task<IActionResult> PutPermiso(Permiso request)
        {
            // Obtener el ID del usuario autenticado
            /*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { error = "Usuario no autenticado o ID de usuario no válido." });
            }*/

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_UPD_PERMISOS";

            command.Parameters.Add(new SqlParameter("@idPermiso", request.idPermiso));
            command.Parameters.Add(new SqlParameter("@IdPantalla", 1)); // Reemplaza con el ID de pantalla adecuado
            command.Parameters.Add(new SqlParameter("@IdGeneral", 11));//loggedInUserId)); // Usar el ID del usuario autenticado
            command.Parameters.Add(new SqlParameter("@Clave", request.Clave ?? (object)DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Nombre", request.Nombre));
            command.Parameters.Add(new SqlParameter("@Descripcion", request.Descripcion ?? (object)DBNull.Value));
            command.Parameters.Add(new SqlParameter("@idModulo", request.idModulo));
            command.Parameters.Add(new SqlParameter("@idAccion", request.idAccion));
            command.Parameters.Add(new SqlParameter("@Activo", request.Activo));
            command.Parameters.Add(new SqlParameter("@Bloqueado", request.Bloqueado));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Permiso modificado correctamente." });
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

        // POST: api/Permisos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Permiso>> PostPermiso(Permiso request)
        {
            // Obtener el ID del usuario autenticado
            /*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { error = "Usuario no autenticado o ID de usuario no válido." });
            }*/

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = "PA_INS_PERMISOS";

            command.Parameters.Add(new SqlParameter("@IdPantalla", 1)); // Reemplaza con el ID de pantalla adecuado
            command.Parameters.Add(new SqlParameter("@IdGeneral", 11));//loggedInUserId)); // Usar el ID del usuario autenticado
            command.Parameters.Add(new SqlParameter("@Clave", request.Clave ?? (object)DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Nombre", request.Nombre));
            command.Parameters.Add(new SqlParameter("@Descripcion", request.Descripcion ?? (object)DBNull.Value));
            command.Parameters.Add(new SqlParameter("@idModulo", request.idModulo));
            command.Parameters.Add(new SqlParameter("@idAccion", request.idAccion));
            command.Parameters.Add(new SqlParameter("@Activo", request.Activo));
            command.Parameters.Add(new SqlParameter("@Bloqueado", request.Bloqueado));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Permiso agregado correctamente." });
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

        // DELETE: api/Permisos/5
        [HttpDelete("{idPermiso}")]
        public async Task<IActionResult> DeletePermiso(int idPermiso)
        {
            // Obtener el ID del usuario autenticado
            /*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { error = "Usuario no autenticado o ID de usuario no válido." });
            }*/

            var sql = "EXEC PA_DEL_PERMISOS @idPermiso, @IdPantalla, @IdGeneral";
            var result = await _context.Database.ExecuteSqlRawAsync(sql,
                new SqlParameter("@idPermiso", idPermiso),
                new SqlParameter("@IdPantalla", 1),
                new SqlParameter("@IdGeneral", 1) //loggedInUserId));
            );

            return Ok(new { mensaje = "Permiso eliminado lógicamente." });
        }

        private bool PermisoExists(int id)
        {
            return _context.PERMISOS.Any(e => e.idPermiso == id);
        }
    }
}
