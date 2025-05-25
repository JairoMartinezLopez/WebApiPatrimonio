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
    public class TransferenciasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TransferenciasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Transferencias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transferencia>>> GetTRANSFERENCIAS()
        {
            return await _context.TRANSFERENCIAS.ToListAsync();
        }

        // GET: api/Transferencias/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Transferencia>> GetTransferencia(int id)
        {
            var transferencia = await _context.TRANSFERENCIAS.FindAsync(id);

            if (transferencia == null)
            {
                return NotFound();
            }

            return transferencia;
        }

        [HttpGet("filtrar")]
        public async Task<ActionResult<IEnumerable<Regiones>>> FiltrarTransferencias(
            [FromQuery] string? folio,
            [FromQuery] DateTime? fechaRegistro,
            [FromQuery] string? responsable,
            [FromQuery] int? idgeneral,
            [FromQuery] bool? activo,
            [FromQuery] int? idAreaOri,
            [FromQuery] int? idAreades
        )
        {
            var query = _context.TRANSFERENCIAS.AsQueryable();

            if (!string.IsNullOrEmpty(folio))
                query = query.Where(f => f.Folio.Contains(folio));

            if (fechaRegistro.HasValue)
                query = query.Where(f => f.FechaRegistro == fechaRegistro);

            if (!string.IsNullOrEmpty(responsable))
                query = query.Where(f => f.Responsable.Contains(responsable));

            if (idgeneral.HasValue)
                query = query.Where(f => f.idGeneral == idgeneral);

            if (activo.HasValue)
                query = query.Where(f => f.Activo == activo);

            if (idAreaOri.HasValue)
                query = query.Where(f => f.idAreaOrigen == idAreaOri);

            if (idAreades.HasValue)
                query = query.Where(f => f.idAreaDestino == idAreades);

            var tranfers = await query
                .Select(f => new Transferencia
                {
                    idTransferencia = f.idTransferencia,
                    Folio = f.Folio,
                    FechaRegistro = f.FechaRegistro,
                    Responsable = f.Responsable,
                    idGeneral = f.idGeneral,
                    Activo = f.Activo,
                    idAreaOrigen = f.idAreaOrigen,
                    idAreaDestino = f.idAreaDestino
                })
                .ToListAsync();

            return Ok(tranfers);
        }

        // PUT: api/Transferencias/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        public async Task<IActionResult> PutTransferencia([FromBody] Transferencia request)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_UPD_TRANSFERENCIA";

            command.Parameters.Add(new SqlParameter("@IdGeneralLogueado", 1115)); // Este es el usuario logueado
            command.Parameters.Add(new SqlParameter("@idTransferencia", request.idTransferencia));
            command.Parameters.Add(new SqlParameter("@Folio", request.Folio));
            command.Parameters.Add(new SqlParameter("@FechaRegistro", request.FechaRegistro));
            command.Parameters.Add(new SqlParameter("@Observaciones", (object?)request.Observaciones ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Responsable", request.Responsable));
            command.Parameters.Add(new SqlParameter("@Activo", (object?)request.Activo ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@idAreaOrigen", request.idAreaOrigen));
            command.Parameters.Add(new SqlParameter("@idAreaDestino", request.idAreaDestino));
            command.Parameters.Add(new SqlParameter("@idGeneral", request.idGeneral)); // Este es el usuario asociado a la transferencia
            command.Parameters.Add(new SqlParameter("@IdPantalla", 1));
            
            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Transferencia actualizada correctamente." });
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

        // POST: api/Transferencias
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Transferencia>> PostTransferencia(Transferencia request)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_INS_TRANSFERENCIA";

            command.Parameters.Add(new SqlParameter("@IdGeneralLogueado", 1115));// Este es el usuario logueado
            command.Parameters.Add(new SqlParameter("@Folio", request.Folio));
            command.Parameters.Add(new SqlParameter("@FechaRegistro", request.FechaRegistro));
            command.Parameters.Add(new SqlParameter("@Observaciones", (object?)request.Observaciones ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Responsable", request.Responsable));
            command.Parameters.Add(new SqlParameter("@Activo", (object?)request.Activo ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@idAreaOrigen", request.idAreaOrigen));
            command.Parameters.Add(new SqlParameter("@idAreaDestino", request.idAreaDestino));
            command.Parameters.Add(new SqlParameter("@idGeneral", request.idGeneral)); // Este es el usuario asociado a la transferencia
            command.Parameters.Add(new SqlParameter("@IdPantalla", 1));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Transferencia agregada correctamente." });
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

        // DELETE: api/Transferencias/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransferencia(int id)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_DEL_TRANSFERENCIA";

            command.Parameters.Add(new SqlParameter("@idTransferencia", id));
            command.Parameters.Add(new SqlParameter("@IdGeneralLogueado", 1)); // Este es el usuario logueado
            command.Parameters.Add(new SqlParameter("@IdPantalla", 1));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Tranferencia eliminada (borrado lógico) correctamente." });
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

        private bool TransferenciaExists(int id)
        {
            return _context.TRANSFERENCIAS.Any(e => e.idTransferencia == id);
        }
    }
}
