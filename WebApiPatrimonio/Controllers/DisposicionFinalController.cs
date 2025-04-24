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
    public class DisposicionFinalController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DisposicionFinalController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/DisposicionFinal
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DisposicionFinal>>> GetDISPOSICIONESFINALES()
        {
            return await _context.DISPOSICIONESFINALES.ToListAsync();
        }

        // GET: api/DisposicionFinal/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DisposicionFinal>> GetDisposicionFinal(int id)
        {
            var disposicionFinal = await _context.DISPOSICIONESFINALES.FindAsync(id);

            if (disposicionFinal == null)
            {
                return NotFound();
            }

            return disposicionFinal;
        }

        // PUT: api/DisposicionFinal/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpGet("filtar")]
        public async Task<ActionResult<IEnumerable<DisposicionFinal>>> filtrarDisposicionFinal(
            [FromQuery] string? nombre,
            [FromQuery] string? descripcion,
            [FromQuery] string? clave,
            [FromQuery] bool? activo,
            [FromQuery] bool? bloqueado)
        {
            var query = _context.DISPOSICIONESFINALES.AsQueryable();

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

            var DisposicionFinales = await query
                .Select(u => new DisposicionFinal
                {
                    idDisposicionFinal = u.idDisposicionFinal,
                    Clave = u.Clave,
                    Nombre = u.Nombre,
                    Descripcion = u.Descripcion,
                    Activo = u.Activo,
                    Bloqueado = u.Bloqueado
                })
                .ToListAsync();

            return Ok(DisposicionFinales);
        }

        // PUT: api/DisposicionFinal/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("modificar")]
        public async Task<IActionResult> PutDisposicionFinal([FromBody] DisposicionFinal request)
        {
            // Obtener el ID del usuario autenticado
            /*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { error = "Usuario no autenticado o ID de usuario no válido." });
            }*/

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_UPD_CAT_DISPOSICIONESFINALES";

            command.Parameters.Add(new SqlParameter("@idDisposicionFinal", request.idDisposicionFinal));
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
                return Ok(new { mensaje = "Disposicion Final modificada correctamente." });
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

        // POST: api/DisposicionFinal
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<DisposicionFinal>> PostCausalBajas(DisposicionFinal request)
        {
            // Obtener el ID del usuario autenticado
            /*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { error = "Usuario no autenticado o ID de usuario no válido." });
            }*/

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = "PA_INS_CAT_DISPOSICIONESFINALES";

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
                return Ok(new { mensaje = "Disposicion Final agregado correctamente." });
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

        // DELETE: api/DisposicionFinal/5
        [HttpDelete("{idDisposicionFinal}")]
        public async Task<IActionResult> DeleteDisposicionFinal(int idDisposicionFinal)
        {
            // Obtener el ID del usuario autenticado
            /*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { error = "Usuario no autenticado o ID de usuario no válido." });
            }*/

            var sql = "EXEC PA_DEL_CAT_DISPOSICIONESFINALES @idDisposicionFinal, @IdPantalla, @IdGeneral";
            var result = await _context.Database.ExecuteSqlRawAsync(sql,
                new SqlParameter("@idDisposicionFinal", idDisposicionFinal),
                new SqlParameter("@IdPantalla", 1),
                new SqlParameter("@IdGeneral", 1) //loggedInUserId));
            );

            return Ok(new { mensaje = "Disposicion Final eliminada lógicamente." });
        }

        private bool DisposicionFinalExists(int id)
        {
            return _context.DISPOSICIONESFINALES.Any(e => e.idDisposicionFinal == id);
        }
    }
}
