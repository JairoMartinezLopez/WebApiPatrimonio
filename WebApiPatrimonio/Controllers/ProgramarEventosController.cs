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
    public class ProgramarEventosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProgramarEventosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ProgramaLevatamiento
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventosInventario>>> GetEVENTOSINVENTARIO()
        {
            return await _context.EVENTOSINVENTARIO.ToListAsync();
        }

        // GET: api/ProgramaLevatamiento/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EventosInventario>> GetProgramaLevatamiento(int id)
        {
            var programaLevatamiento = await _context.EVENTOSINVENTARIO.FindAsync(id);

            if (programaLevatamiento == null)
            {
                return NotFound();
            }

            return programaLevatamiento;
        }

        [HttpGet("filtar")]
        public async Task<ActionResult<IEnumerable<EventosInventario>>> filtrarEventos(
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaTermino,
            [FromQuery] int? idArea,
            [FromQuery] int? idGeneral,
            [FromQuery] int? idEventoestado,
            [FromQuery] bool? activo,
            [FromQuery] string? folio)
        {
            var query = _context.EVENTOSINVENTARIO.AsQueryable();

            if (fechaInicio.HasValue)
                query = query.Where(u => u.FechaInicio == fechaInicio);

            if (fechaTermino.HasValue)
                query = query.Where(u => u.FechaTermino == fechaTermino);

            if (idArea.HasValue)
                query = query.Where(u => u.idArea == idArea);

            if (idGeneral.HasValue)
                query = query.Where(u => u.idGeneral == idGeneral);

            if (idEventoestado.HasValue)
                query = query.Where(u => u.idEventoEstado == idEventoestado);

            if (activo.HasValue)
                query = query.Where(u => u.Activo == activo);

            if (!string.IsNullOrEmpty(folio))
                query = query.Where(u => u.Folio.Contains(folio));

            var eventos = await query
                .Select(u => new EventosInventario
                {
                    IdEventoInventario = u.IdEventoInventario,
                    FechaInicio = u.FechaInicio,
                    FechaTermino = u.FechaTermino,
                    idArea = u.idArea,
                    idGeneral = u.idGeneral,
                    idEventoEstado = u.idEventoEstado,
                    Activo = u.Activo,
                    Folio = u.Folio
                }).ToListAsync();

            return Ok(eventos);
        }

        // PUT: api/ProgramaLevatamiento/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        public async Task<IActionResult> PutProgramaLevatamiento([FromBody] EventosInventario request)
        {
            // Obtener el ID del usuario autenticado
            /*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { error = "Usuario no autenticado o ID de usuario no válido." });
            }*/

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_UPD_EVENTOSINVENTARIO";

            command.Parameters.Add(new SqlParameter("@IdPantalla", 1)); // Reemplaza con el ID de pantalla adecuado
            command.Parameters.Add(new SqlParameter("@IdGeneral ", 1115)); //loggedInUserId)); // Usar el ID del usuario autenticado
            command.Parameters.Add(new SqlParameter("@IdGeneralAsignado", request.idGeneral));// Este es el usuario asignado al evento
            command.Parameters.Add(new SqlParameter("@idEventoInventario", request.IdEventoInventario));
            command.Parameters.Add(new SqlParameter("@idArea", request.idArea));
            command.Parameters.Add(new SqlParameter("@FechaInicio", request.FechaInicio));
            command.Parameters.Add(new SqlParameter("@FechaTermino", request.FechaTermino ?? (object)DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Folio", request.Folio ?? (object)DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Activo", request.Activo));
            command.Parameters.Add(new SqlParameter("@idEventoEstado", request.idEventoEstado));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "El Evento del inventario a sido modificado correctamente." });
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

        [HttpPut("CambiarEstado")]
        public async Task<IActionResult> PutEstdoLevatamiento([FromBody] EventosInventario request)
        {
            // Obtener el ID del usuario autenticado
            /*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { error = "Usuario no autenticado o ID de usuario no válido." });
            }*/

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_UPD_EVENTOSINVENTARIO_ESTADO";

            command.Parameters.Add(new SqlParameter("@IdPantalla", 1)); // Reemplaza con el ID de pantalla adecuado
            command.Parameters.Add(new SqlParameter("@IdGeneral ", 1115)); //loggedInUserId)); // Usar el ID del usuario autenticado
            command.Parameters.Add(new SqlParameter("@idEventoInventario", request.IdEventoInventario));
            command.Parameters.Add(new SqlParameter("@idEventoEstado", request.idEventoEstado));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "El estado del evento a sido modificado correctamente." });
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

        // POST: api/ProgramaLevatamiento
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> InsertarEventoInventario(EventosInventario request)
        {
            // Obtener el ID del usuario autenticado
            /*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { error = "Usuario no autenticado o ID de usuario no válido." });
            }*/

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = "PA_INS_EVENTOSINVENTARIO";

            command.Parameters.Add(new SqlParameter("@IdPantalla", 1)); // Reemplaza con el ID de pantalla adecuado
            command.Parameters.Add(new SqlParameter("@IdGeneral ", 1115)); //loggedInUserId)); // Usar el ID del usuario autenticado
            command.Parameters.Add(new SqlParameter("@IdGeneralAsignado", request.idGeneral));// Este es el usuario asignado al evento
            command.Parameters.Add(new SqlParameter("@idArea", request.idArea));
            command.Parameters.Add(new SqlParameter("@FechaInicio", request.FechaInicio));
            command.Parameters.Add(new SqlParameter("@FechaTermino", request.FechaTermino));
            command.Parameters.Add(new SqlParameter("@Folio", request.Folio));
            command.Parameters.Add(new SqlParameter("@Activo", 1));
            command.Parameters.Add(new SqlParameter("@idEventoEstado", request.idEventoEstado));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Evento Programado correctamente." });
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

        // DELETE: api/ProgramaLevatamiento/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProgramaLevatamiento(int id)
        {
            var sql = "EXEC PA_DEL_EVENTOSINVENTARIO @IdPantalla, @IdGeneral, @idEventoInventario";
            var result = await _context.Database.ExecuteSqlRawAsync(sql,
                new SqlParameter("@IdPantalla", 1),
                new SqlParameter("@IdGeneral", 1), //loggedInUserId));
                new SqlParameter("@idEventoInventario", id)
            );

            return Ok(new { mensaje = "Evento eliminado lógicamente." });
        }

        private bool ProgramaLevatamientoExists(int id)
        {
            return _context.EVENTOSINVENTARIO.Any(e => e.IdEventoInventario == id);
        }
    }
}
