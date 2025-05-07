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
        public async Task<ActionResult<IEnumerable<Bien>>> Get_BIENES()
        {
            return await _context.BIENES.ToListAsync();
        }

        // GET: api/Bien/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Bien>> GetBien(long id)
        {
            var bien = await _context.BIENES.
                Select(b => new Bien  // ⬅️ Solo selecciona las columnas existentes en la BD
            {
                IdBien = b.IdBien,
                IdColor = b.IdColor,
                FechaAlta = b.FechaAlta,
                Aviso = b.Aviso,
                Serie = b.Serie,
                Modelo = b.Modelo,
                IdEstadoFisico = b.IdEstadoFisico,
                IdMarca = b.IdMarca,
                Costo = b.Costo,
                Etiquetado = b.Etiquetado,
                FechaEtiquetado = b.FechaEtiquetado,
                Disponibilidad = b.Disponibilidad,
                FechaBaja = b.FechaBaja,
                IdCausalBaja = b.IdCausalBaja,
                IdDisposicionFinal = b.IdDisposicionFinal,
                IdFactura = b.IdFactura,
                NoInventario = b.NoInventario,
                IdCatalogoBien = b.IdCatalogoBien,
                Observaciones = b.Observaciones
            })
        .Where(b => b.IdBien == id)
        .SingleOrDefaultAsync();

            if (bien == null)
            {
                return NotFound();
            }

            return bien;
        }

        // PUT: api/Bien/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        public async Task<ActionResult> PutBien([FromBody] Bien request)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_UPD_BIENES";

            command.Parameters.Add(new SqlParameter("@IdPantalla", 1));
            command.Parameters.Add(new SqlParameter("@IdGeneral", 1));
            command.Parameters.Add(new SqlParameter("@idBien", request.IdBien));
            command.Parameters.Add(new SqlParameter("@idColor", (object?)request.IdColor ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@FechaRegistro", (object?)request.FechaRegistro ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@FechaAlta", (object?)request.FechaAlta ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Aviso", request.Aviso ?? (object)DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Serie", request.Serie ?? (object)DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Modelo", request.Modelo ?? (object)DBNull.Value));
            command.Parameters.Add(new SqlParameter("@idEstadoFisico", (object?)request.IdEstadoFisico ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@idMarca", (object?)request.IdMarca ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Costo", (object?)request.Costo ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Etiquetado", (object?)request.Etiquetado ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@FechaEtiquetado", (object?)request.FechaEtiquetado ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Activo", (object?)request.Activo ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Disponibilidad", (object?)request.Disponibilidad ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@FechaBaja", (object?)request.FechaBaja ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@idCausalBaja", (object?)request.IdCausalBaja ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@idDisposicionFinal", (object?)request.IdDisposicionFinal ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@idFactura", request.IdFactura));
            command.Parameters.Add(new SqlParameter("@NoInventario", request.NoInventario ?? (object)DBNull.Value));
            command.Parameters.Add(new SqlParameter("@idCatalogoBien", request.IdCatalogoBien));
            command.Parameters.Add(new SqlParameter("@Observaciones", request.Observaciones ?? (object)DBNull.Value));
            command.Parameters.Add(new SqlParameter("@AplicaUMAS", (object?)request.AplicaUMAS ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Salida", request.Salida ?? (object)DBNull.Value));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Bien actualizado correctamente." });
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



        // POST: api/Bien
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("Insertar")]
        public async Task<IActionResult> InsertarBien([FromBody] Bien bien)
        {
            if (bien == null)
            {
                return BadRequest("Los datos del bien son requeridos.");
            }

            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC PA_INS_BIENES @IdPantalla, @IdGeneral, @idColor, @FechaAlta, @Aviso, @Serie, @Modelo, " +
                    "@idEstadoFisico, @idMarca, @Costo, @Etiquetado, @FechaEtiquetado, @Activo, @Disponibilidad, " +
                    "@FechaBaja, @idCausalBaja, @idDisposicionFinal, @idFactura, @NoInventario, @idCatalogoBien, " +
                    "@Observaciones, @Salida",
                    new SqlParameter("@IdPantalla", 1),
                    new SqlParameter("@IdGeneral", 1),
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
                    new SqlParameter("@Activo", bien.Activo ?? (object)DBNull.Value),
                    new SqlParameter("@Disponibilidad", bien.Disponibilidad ?? (object)DBNull.Value),
                    new SqlParameter("@FechaBaja", bien.FechaBaja ?? (object)DBNull.Value),
                    new SqlParameter("@idCausalBaja", bien.IdCausalBaja ?? (object)DBNull.Value),
                    new SqlParameter("@idDisposicionFinal", bien.IdDisposicionFinal ?? (object)DBNull.Value),
                    new SqlParameter("@idFactura", bien.IdFactura ?? (object)DBNull.Value),
                    new SqlParameter("@NoInventario", bien.NoInventario ?? (object)DBNull.Value),
                    new SqlParameter("@idCatalogoBien", bien.IdCatalogoBien ?? (object)DBNull.Value),
                    new SqlParameter("@Observaciones", bien.Observaciones ?? (object)DBNull.Value),
                    new SqlParameter("@Salida", bien.Salida ?? (object)DBNull.Value)
                );

                return Ok(new { Message = "Bien insertado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }


        // DELETE: api/Bien/5
        [HttpDelete("eliminar{idBien}")]
        public async Task<IActionResult> DeleteBien(long idBien)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_DEL_BIENES";

            command.Parameters.Add(new SqlParameter("@IdPantalla", 1));
            command.Parameters.Add(new SqlParameter("@IdGeneral", 1));
            command.Parameters.Add(new SqlParameter("@idBien", idBien));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Bien eliminado (borrado lógico) correctamente." });
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


        private bool BienExists(long id)
        {
            return _context.BIENES.Any(e => e.IdBien == id);
        }
    }
}
