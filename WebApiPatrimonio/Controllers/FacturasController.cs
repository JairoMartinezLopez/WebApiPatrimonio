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
    public class FacturasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FacturasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Facturas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Factura>>> GetFACTURAS()
        {
            return await _context.FACTURAS.ToListAsync();
        }

        // GET: api/Facturas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Factura>> GetFactura(long? id)
        {
            var factura = await _context.FACTURAS.FindAsync(id);

            if (factura == null)
            {
                return NotFound();
            }

            return factura;
        }

        // PUT: api/Facturas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpGet("filtrar")]
        public async Task<ActionResult<IEnumerable<Factura>>> FiltrarFacturas(
        [FromQuery] string? numeroFactura,
        [FromQuery] string? folioFiscal,
        [FromQuery] DateTime? fechaFactura,
        [FromQuery] string? nota,
        [FromQuery] bool? publicado,
        [FromQuery] DateTime? fechaRegistro,
        [FromQuery] bool? activo
    )
        {
            var query = _context.FACTURAS.AsQueryable();

            if (!string.IsNullOrEmpty(numeroFactura))
                query = query.Where(f => f.NumeroFactura.Contains(numeroFactura));

            if (!string.IsNullOrEmpty(folioFiscal))
                query = query.Where(f => f.FolioFiscal.Contains(folioFiscal));

            if (fechaFactura.HasValue)
                query = query.Where(f => f.FechaFactura == fechaFactura);

            if (!string.IsNullOrEmpty(nota))
                query = query.Where(f => f.Nota.Contains(nota));

            if (publicado.HasValue)
                query = query.Where(f => f.Publicar == publicado);

            if (activo.HasValue)
                query = query.Where(f => f.Activo == activo);

            if (fechaRegistro.HasValue)
                query = query.Where(f => f.FechaRegistro == fechaRegistro);

            var facturas = await query
                .Select(f => new Factura
                {
                    idFactura = f.idFactura,
                    NumeroFactura = f.NumeroFactura,
                    FolioFiscal = f.FolioFiscal,
                    FechaFactura = f.FechaFactura,
                    idFinanciamiento = f.idFinanciamiento,
                    idUnidadResponsable = f.idUnidadResponsable,
                    idEstado = f.idEstado,
                    Nota = f.Nota,
                    Publicar = f.Publicar,
                    Activo = f.Activo,
                    FechaRegistro = f.FechaRegistro
                })
                .ToListAsync();

            return Ok(facturas);
        }

        // PUT: api/Facturas/modificar
        [HttpPut("modificar")]
        public async Task<IActionResult> PutFactura([FromBody] Factura request)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_UPD_FACTURAS";

            command.Parameters.Add(new SqlParameter("@idFactura", request.idFactura));
            command.Parameters.Add(new SqlParameter("@IdPantalla", 1));
            command.Parameters.Add(new SqlParameter("@IdGeneral", 1));
            command.Parameters.Add(new SqlParameter("@NumeroFactura", request.NumeroFactura ?? (object)DBNull.Value));
            command.Parameters.Add(new SqlParameter("@FolioFiscal", request.FolioFiscal ?? (object)DBNull.Value));
            command.Parameters.Add(new SqlParameter("@FechaFactura", request.FechaFactura));
            command.Parameters.Add(new SqlParameter("@idFinanciamiento", request.idFinanciamiento));
            command.Parameters.Add(new SqlParameter("@idUnidadResponsable", request.idUnidadResponsable));
            command.Parameters.Add(new SqlParameter("@idEstado", request.idEstado));
            command.Parameters.Add(new SqlParameter("@Nota", request.Nota ?? (object)DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Publicar", request.Publicar));
            command.Parameters.Add(new SqlParameter("@Activo", request.Activo));
            command.Parameters.Add(new SqlParameter("@Archivo", (object)request.Archivo ?? DBNull.Value));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Factura modificada correctamente." });
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

        // POST: api/Facturas
        [HttpPost]
        public async Task<ActionResult> PostFactura([FromBody] Factura request)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_INS_FACTURAS";

            command.Parameters.Add(new SqlParameter("@IdPantalla", 1));
            command.Parameters.Add(new SqlParameter("@IdGeneral", 1));
            command.Parameters.Add(new SqlParameter("@NumeroFactura", request.NumeroFactura ?? (object)DBNull.Value));
            command.Parameters.Add(new SqlParameter("@FolioFiscal", request.FolioFiscal ?? (object)DBNull.Value));
            command.Parameters.Add(new SqlParameter("@FechaFactura", request.FechaFactura));
            command.Parameters.Add(new SqlParameter("@idFinanciamiento", request.idFinanciamiento));
            command.Parameters.Add(new SqlParameter("@idUnidadResponsable", request.idUnidadResponsable));
            command.Parameters.Add(new SqlParameter("@idEstado", request.idEstado));
            command.Parameters.Add(new SqlParameter("@Nota", request.Nota ?? (object)DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Publicar", request.Publicar));
            command.Parameters.Add(new SqlParameter("@Activo", request.Activo));
            command.Parameters.Add(new SqlParameter("@FechaRegistro", request.FechaRegistro));
            command.Parameters.Add(new SqlParameter("@Archivo", (object)request.Archivo ?? DBNull.Value));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Factura agregada correctamente." });
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

        // DELETE: api/Facturas/{idFactura}
        [HttpDelete("{idFactura}")]
        public async Task<IActionResult> DeleteFactura(long idFactura)
        {
            var sql = "EXEC PA_DEL_FACTURAS @idFactura, @IdPantalla, @IdGeneral";
            var result = await _context.Database.ExecuteSqlRawAsync(sql,
                new SqlParameter("@idFactura", idFactura),
                new SqlParameter("@IdPantalla", 1),
                new SqlParameter("@IdGeneral", 1)
            );

            return Ok(new { mensaje = "Factura eliminada lógicamente." });
        }

        [HttpGet("archivo/{id}")]
        public async Task<IActionResult> ObtenerArchivoFactura(long id)
        {
            var factura = await _context.FACTURAS.FindAsync(id);
            if (factura == null || factura.Archivo == null)
                return NotFound();

            return File(factura.Archivo, "application/octet-stream", $"Factura_{id}.pdf");
        }

        private bool FacturaExists(int? id)
        {
            return _context.FACTURAS.Any(e => e.idFactura == id);
        }
    }
}
