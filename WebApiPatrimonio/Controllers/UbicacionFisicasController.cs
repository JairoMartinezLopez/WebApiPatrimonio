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
    public class UbicacionFisicasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UbicacionFisicasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/UbicacionFisicas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UbicacionFisica>>> GetUBICACIONESFISICAS()
        {
            return await _context.UBICACIONESFISICAS.ToListAsync();
        }

        // GET: api/UbicacionFisicas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UbicacionFisica>> GetUbicacionFisica(int id)
        {
            var ubicacionFisica = await _context.UBICACIONESFISICAS.FindAsync(id);

            if (ubicacionFisica == null)
            {
                return NotFound();
            }

            return ubicacionFisica;
        }

        [HttpGet("filtrar")]
        public async Task<ActionResult<IEnumerable<Regiones>>> FiltrarUbicaciones(
            [FromQuery] long? idBien,
            [FromQuery] DateTime? fechaTranferencia,
            [FromQuery] DateTime? fechaCaptura,
            [FromQuery] bool? activo,
            [FromQuery] int? idTransferencia
        )
        {
            var query = _context.UBICACIONESFISICAS.AsQueryable();

            if (idBien.HasValue)
                query = query.Where(f => f.IdBien == idBien);

            if (fechaTranferencia.HasValue)
                query = query.Where(f => f.FechaTransferencia == fechaTranferencia);

            if (fechaCaptura.HasValue)
                query = query.Where(f => f.FechaCaptura == fechaCaptura);

            if (activo.HasValue)
                query = query.Where(f => f.Activo == activo);

            if (idTransferencia.HasValue)
                query = query.Where(f => f.IdTransferencia == idTransferencia);

            var tranfers = await query
                .Select(f => new UbicacionFisica
                {
                    IdUbicacionFisica = f.IdUbicacionFisica,
                    IdBien = f.IdBien,
                    FechaTransferencia = f.FechaTransferencia,
                    FechaCaptura = f.FechaCaptura,
                    Activo = f.Activo,
                    IdTransferencia = f.IdTransferencia
                })
                .ToListAsync();

            return Ok(tranfers);
        }

        // PUT: api/UbicacionFisicas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        public async Task<IActionResult> PutUbicacion([FromBody] UbicacionFisica request)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_UPD_UBICACIONFISICA";

            command.Parameters.Add(new SqlParameter("@idUbicacionFisica", request.IdUbicacionFisica));
            command.Parameters.Add(new SqlParameter("@idBien", request.IdBien));
            command.Parameters.Add(new SqlParameter("@FechaTransferencia", request.FechaTransferencia));
            command.Parameters.Add(new SqlParameter("@idTransferencia", (object?)request.IdTransferencia ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@IdGeneral", 1115));
            command.Parameters.Add(new SqlParameter("@IdPantalla", 1));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Ubicación actualizada correctamente." });
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

        // DELETE: api/UbicacionFisicas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUbicacionFisica(int id)
        {
            var ubicacionFisica = await _context.UBICACIONESFISICAS.FindAsync(id);
            if (ubicacionFisica == null)
            {
                return NotFound();
            }

            _context.UBICACIONESFISICAS.Remove(ubicacionFisica);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        // POST: api/UbicacionFisicas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Transferencia>> PostUbicacion(UbicacionFisica request)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_INS_UBICACIONESFISICAS";

            command.Parameters.Add(new SqlParameter("@idBien", request.IdBien));
            command.Parameters.Add(new SqlParameter("@FechaTransferencia", request.FechaTransferencia));
            command.Parameters.Add(new SqlParameter("@idTransferencia", (object?)request.IdTransferencia ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@IdPantalla", 1));
            command.Parameters.Add(new SqlParameter("@IdGeneral", 1115));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Ubicación fisica agregada correctamente." });
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

        private bool UbicacionFisicaExists(int id)
        {
            return _context.UBICACIONESFISICAS.Any(e => e.IdUbicacionFisica == id);
        }
    }
}
