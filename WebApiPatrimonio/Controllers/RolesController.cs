using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiPatrimonio.Context;
using WebApiPatrimonio.Models;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Claims;

namespace WebApiPatrimonio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RolesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Roles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Roles>>> GetROLES()
        {
            var roles = await _context.ROLES
               .Select(u => new Roles
               {
                   idRol = u.idRol,
                   Clave = u.Clave,
                   Nombre = u.Nombre,
                   Descripcion = u.Descripcion,
                   Activo = u.Activo,
                   Bloqueado = u.Bloqueado
               })
               .ToListAsync();
            return Ok(roles);
        }

        // GET: api/Roles/ByName/{nombre}
        [HttpGet("filtar")]
        public async Task<ActionResult<IEnumerable<Roles>>> filtrarRoles(
            [FromQuery] string? nombre,
            [FromQuery] string? descripcion,
            [FromQuery] string? clave,
            [FromQuery] bool? activo,
            [FromQuery] bool? bloqueado)
        {
            var query = _context.ROLES.AsQueryable();

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(u => u.Nombre.Contains(nombre));

            if (!string.IsNullOrEmpty(descripcion))
                query = query.Where(u => u.Descripcion.Contains(descripcion));

            if (!string.IsNullOrEmpty(clave))
                query = query.Where(u => u.Clave.Contains(clave));

            if (activo.HasValue)
                query = query.Where(u => u.Activo == activo);

            if (bloqueado.HasValue)
                query = query.Where(u => u.Bloqueado == bloqueado);
            
            if(activo.HasValue)
                query = query.Where(u => u.Activo == activo);

            var roles = await query
                .Select(u => new Roles
                {
                    idRol = u.idRol,
                    Clave = u.Clave,
                    Nombre = u.Nombre,
                    Descripcion = u.Descripcion,
                    Activo = u.Activo,
                    Bloqueado = u.Bloqueado
                })
                .ToListAsync();

            return Ok(roles);
        }

        // PUT: api/Roles/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("Modificar")]
        public async Task<IActionResult> ModificarRoles([FromBody] Roles request)
        {
            // Obtener el ID del usuario autenticado
            /*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { error = "Usuario no autenticado o ID de usuario no válido." });
            }*/

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_UPD_ROL";

            command.Parameters.Add(new SqlParameter("@idRol", request.idRol));
            command.Parameters.Add(new SqlParameter("@IdPantalla", 1)); // Reemplaza con el ID de pantalla adecuado
            command.Parameters.Add(new SqlParameter("@IdGeneral", 1));//loggedInUserId)); // Usar el ID del usuario autenticado
            command.Parameters.Add(new SqlParameter("@Clave", request.Clave ?? (object)DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Nombre", request.Nombre));
            command.Parameters.Add(new SqlParameter("@Descripcion", request.Descripcion ?? (object)DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Activo", request.Activo));
            command.Parameters.Add(new SqlParameter("@Bloqueado", request.Bloqueado));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Rol modificado correctamente." });
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


        // POST: api/Roles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> InsertarRoles([FromBody] Roles request)
        {
            // Obtener el ID del usuario autenticado
            /*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { error = "Usuario no autenticado o ID de usuario no válido." });
            }*/

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = "PA_INS_ROL";

            command.Parameters.Add(new SqlParameter("@IdPantalla", 1)); // Reemplaza con el ID de pantalla adecuado
            command.Parameters.Add(new SqlParameter("@IdGeneral", 1));//loggedInUserId));
            command.Parameters.Add(new SqlParameter("@Clave", request.Clave));
            command.Parameters.Add(new SqlParameter("@Nombre", request.Nombre));
            command.Parameters.Add(new SqlParameter("@Descripcion", request.Descripcion));
            command.Parameters.Add(new SqlParameter("@Activo", request.Activo));
            command.Parameters.Add(new SqlParameter("@Bloqueado", request.Bloqueado));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Rol insertado correctamente." });
            }
            catch (SqlException ex) {
                return BadRequest(new { error = ex.Message });
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        // DELETE: api/Roles/5
        [HttpDelete("idRol")]
        public async Task<IActionResult> DeleteRoles([FromBody] Roles request)
        {
            // Obtener el ID del usuario autenticado
            /*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { error = "Usuario no autenticado o ID de usuario no válido." });
            }*/

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "PA_DEL_ROL";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@idRol", request.idRol));
            command.Parameters.Add(new SqlParameter("@IdPantalla", 1));
            command.Parameters.Add(new SqlParameter("@idGeneral", 1)); //loggedInUserId));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();

                return Ok(new { mensaje = "Rol desactivado correctamente." });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { error = "Error al desactivar el Rol.", detalle = ex.Message });
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        private bool RolesExists(int id)
        {
            return _context.ROLES.Any(e => e.idRol == id);
        }
    }
}