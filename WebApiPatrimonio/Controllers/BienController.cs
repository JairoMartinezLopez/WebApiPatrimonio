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
    public class BienController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BienController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Bien
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bien>>> GetPAT_BIENES()
        {
            return await _context.PAT_BIENES.ToListAsync();
        }

        // GET: api/Bien/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Bien>> GetBien(long id)
        {
            var bien = await _context.PAT_BIENES.FindAsync(id);

            if (bien == null)
            {
                return NotFound();
            }

            return bien;
        }

        // PUT: api/Bien/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBien(long id, Bien bien)
        {
            if (id != bien.IdBien)
            {
                return BadRequest();
            }

            _context.Entry(bien).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BienExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Bien
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("Inserta un bien")]
        public async Task<IActionResult> InsertarBien([FromBody] Bien bien)
        {
            if (bien == null)
            {
                return BadRequest("Los datos del bien son requeridos.");
            }

            try
            {
                var idBienParam = new SqlParameter("@IdBien", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC PA_INS_PAT_BIENES @IdGeneral, @IdAreaSistemaUsuario, @IdPantalla, @IdBien OUT, " +
                    "@idColor, @FechaAlta, @Aviso, @Serie, @Modelo, @idEstadoFisico, @idMarca, @Costo, " +
                    "@Etiquetado, @FechaEtiquetado, @Estatus, @FechaBaja, @Causal, @idDisposicion, @IdFactura, " +
                    "@NoInventario, @idTipoBien, @IdCatBien, @Observaciones, @IdCategoria, @IdFinanciamiento, " +
                    "@IdAdscripcion, @Salida, @Cantidad",
                    new SqlParameter("@IdGeneral", bien.IdAreaSistemaUsuario),
                    new SqlParameter("@IdAreaSistemaUsuario", bien.IdAreaSistemaUsuario),
                    new SqlParameter("@IdPantalla", bien.IdPantalla),
                    idBienParam,
                    new SqlParameter("@idColor", bien.IdColor ?? (object)DBNull.Value),
                    new SqlParameter("@FechaAlta", bien.FechaAlta ?? (object)DBNull.Value),
                    new SqlParameter("@Aviso", bien.Aviso ?? (object)DBNull.Value),
                    new SqlParameter("@Serie", bien.Serie ?? (object)DBNull.Value),
                    new SqlParameter("@Modelo", bien.Modelo ?? (object)DBNull.Value),
                    new SqlParameter("@idEstadoFisico", bien.IdEstadoFisico ?? (object)DBNull.Value),
                    new SqlParameter("@idMarca", bien.IdMarca ?? (object)DBNull.Value),
                    new SqlParameter("@Costo", bien.Costo ?? (object)DBNull.Value),
                    new SqlParameter("@Etiquetado", bien.Etiquetado ?? (object)DBNull.Value),
                    new SqlParameter("@FechaEtiquetado", bien.FechaEtiquetado ?? (object)DBNull.Value),
                    new SqlParameter("@Estatus", bien.Estatus ?? (object)DBNull.Value),
                    new SqlParameter("@FechaBaja", bien.FechaBaja ?? (object)DBNull.Value),
                    new SqlParameter("@Causal", bien.IdCausal ?? (object)DBNull.Value),
                    new SqlParameter("@idDisposicion", bien.IdDisposicion ?? (object)DBNull.Value),
                    new SqlParameter("@IdFactura", bien.IdFactura ?? (object)DBNull.Value),
                    new SqlParameter("@NoInventario", bien.NoInventario ?? (object)DBNull.Value),
                    new SqlParameter("@idTipoBien", bien.IdTipoBien ?? (object)DBNull.Value),
                    new SqlParameter("@IdCatBien", bien.IdCatBien ?? (object)DBNull.Value),
                    new SqlParameter("@Observaciones", bien.Observaciones ?? (object)DBNull.Value),
                    new SqlParameter("@IdCategoria", bien.IdCategoria ?? (object)DBNull.Value),
                    new SqlParameter("@IdFinanciamiento", bien.IdFinanciamiento ?? (object)DBNull.Value),
                    new SqlParameter("@IdAdscripcion", bien.IdAdscripcion ?? (object)DBNull.Value),
                    new SqlParameter("@Salida", bien.Salida ?? (object)DBNull.Value),
                    new SqlParameter("@Cantidad", 1)
                );

                return Ok(new { Message = "Bien insertado correctamente", IdBien = idBienParam.Value });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        // DELETE: api/Bien/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBien(long id)
        {
            var bien = await _context.PAT_BIENES.FindAsync(id);
            if (bien == null)
            {
                return NotFound();
            }

            _context.PAT_BIENES.Remove(bien);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BienExists(long id)
        {
            return _context.PAT_BIENES.Any(e => e.IdBien == id);
        }
    }
}
