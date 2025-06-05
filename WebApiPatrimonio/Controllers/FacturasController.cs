using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WebApiPatrimonio.Context;
using WebApiPatrimonio.Models;
using System.IO;

namespace WebApiPatrimonio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacturasController : ControllerBase
    {
        private readonly ApplicationDbContext _ctx;
        private const string RUTA_NAS = @"\\MSI\facturas"; // Ajusta al path real

        public FacturasController(ApplicationDbContext ctx) => _ctx = ctx;

        /* ===== Helpers ================================================== */
        private async Task<bool> UsarNasAsync()
            => (await _ctx.ConfiguracionGeneral.FirstAsync()).GuardarEnNas;

        /* ======================= LISTA ================================== */
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Factura>>> GetAll()
            => await _ctx.FACTURAS.AsNoTracking().ToListAsync();

        /* ======================= FILTRAR ================================ */
        [HttpGet("filtrar")]
        public async Task<ActionResult<IEnumerable<Factura>>> Filtrar(
            [FromQuery] string? numeroFactura,
            [FromQuery] string? folioFiscal,
            [FromQuery] DateTime? fechaFactura,
            [FromQuery] string? nota,
            [FromQuery] bool? publicado,
            [FromQuery] bool? activo,
            [FromQuery] DateTime? fechaRegistro)
        {
            var q = _ctx.FACTURAS.AsQueryable();

            if (!string.IsNullOrWhiteSpace(numeroFactura))
                q = q.Where(f => f.NumeroFactura!.Contains(numeroFactura));
            if (!string.IsNullOrWhiteSpace(folioFiscal))
                q = q.Where(f => f.FolioFiscal!.Contains(folioFiscal));
            if (fechaFactura.HasValue) q = q.Where(f => f.FechaFactura == fechaFactura);
            if (!string.IsNullOrWhiteSpace(nota)) q = q.Where(f => f.Nota!.Contains(nota));
            if (publicado.HasValue) q = q.Where(f => f.Publicar == publicado);
            if (activo.HasValue) q = q.Where(f => f.Activo == activo);
            if (fechaRegistro.HasValue) q = q.Where(f => f.FechaRegistro == fechaRegistro);

            return Ok(await q.AsNoTracking().ToListAsync());
        }

        /* ======================= CREAR (multipart/form‑data) ============ */
        [HttpPost]
        [RequestSizeLimit(50_000_000)] // 50 MB
        public async Task<IActionResult> Post([FromForm] FacturaFormDto dto)
        {
            byte[]? fileBytes = null;
            if (dto.Archivo is { Length: > 0 })
            {
                using var ms = new MemoryStream();
                await dto.Archivo.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            bool usarNas = await UsarNasAsync();
            string? rutaNas = null;

            if (usarNas && fileBytes is not null)
            {
                Directory.CreateDirectory(RUTA_NAS); // por si no existe
                var fileName = $"{Guid.NewGuid()}.pdf";
                rutaNas = Path.Combine(RUTA_NAS, fileName);
                System.IO.File.WriteAllBytes(rutaNas, fileBytes);
                fileBytes = null; // no se guardará en la BD
            }

            await using var cmd = _ctx.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PA_INS_FACTURAS";

            cmd.Parameters.AddRange(new[]
            {
                new SqlParameter("@IdPantalla",           1),
                new SqlParameter("@IdGeneral",            1),
                new SqlParameter("@NumeroFactura",        dto.NumeroFactura        ?? (object)DBNull.Value),
                new SqlParameter("@FolioFiscal",          dto.FolioFiscal          ?? (object)DBNull.Value),
                new SqlParameter("@FechaFactura",         dto.FechaFactura         ?? (object)DBNull.Value),
                new SqlParameter("@idFinanciamiento",     dto.idFinanciamiento     ?? (object)DBNull.Value),
                new SqlParameter("@idUnidadResponsable",  dto.idUnidadResponsable  ?? (object)DBNull.Value),
                new SqlParameter("@idEstado",             dto.idEstado             ?? (object)DBNull.Value),
                new SqlParameter("@Nota",                 dto.Nota                 ?? (object)DBNull.Value),
                new SqlParameter("@Publicar",             dto.Publicar             ?? (object)DBNull.Value),
                new SqlParameter("@Activo",               dto.Activo               ?? (object)DBNull.Value),
                new SqlParameter("@FechaRegistro",        dto.FechaRegistro        ?? DateTime.Now),
                new SqlParameter("@Archivo",              SqlDbType.VarBinary, -1) { Value = (object?)fileBytes ?? DBNull.Value },
                new SqlParameter("@RutaArchivo",          SqlDbType.NVarChar, 260) { Value = (object?)rutaNas   ?? DBNull.Value },
                new SqlParameter("@CantidadBienes",       dto.CantidadBienes       ?? (object)DBNull.Value)
            });

            try
            {
                await _ctx.Database.OpenConnectionAsync();
                await cmd.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Factura agregada correctamente." });
            }
            catch (SqlException ex) { return BadRequest(new { ex.Message }); }
            finally { await _ctx.Database.CloseConnectionAsync(); }
        }

        /* ======================= MODIFICAR ============================== */
        [HttpPut("modificar")]
        public async Task<IActionResult> Put([FromBody] Factura f)
        {
            bool usarNas = await UsarNasAsync();
            byte[]? archivoBytes = f.Archivo;
            string? rutaNas = f.RutaArchivo;

            if (usarNas && archivoBytes is not null)
            {
                /* Guardar archivo nuevo en NAS y limpiar varbinary */
                Directory.CreateDirectory(RUTA_NAS);
                var fileName = $"{Guid.NewGuid()}.pdf";
                rutaNas = Path.Combine(RUTA_NAS, fileName);
                System.IO.File.WriteAllBytes(rutaNas, archivoBytes);
                archivoBytes = null;
            }
            else if (!usarNas)
            {
                /* Si vuelves a BD, vacía ruta y usa varbinary */
                rutaNas = null;
            }

            await using var cmd = _ctx.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PA_UPD_FACTURAS";

            cmd.Parameters.AddRange(new[]
            {
                new SqlParameter("@idFactura",      f.idFactura),
                new SqlParameter("@IdPantalla",     1),
                new SqlParameter("@IdGeneral",      1),
                new SqlParameter("@NumeroFactura",  f.NumeroFactura  ?? (object)DBNull.Value),
                new SqlParameter("@FolioFiscal",    f.FolioFiscal    ?? (object)DBNull.Value),
                new SqlParameter("@FechaFactura",   f.FechaFactura   ?? (object)DBNull.Value),
                new SqlParameter("@idFinanciamiento",   f.idFinanciamiento   ?? (object)DBNull.Value),
                new SqlParameter("@idUnidadResponsable",f.idUnidadResponsable?? (object)DBNull.Value),
                new SqlParameter("@idEstado",           f.idEstado          ?? (object)DBNull.Value),
                new SqlParameter("@Nota",          f.Nota          ?? (object)DBNull.Value),
                new SqlParameter("@Publicar",      f.Publicar      ?? (object)DBNull.Value),
                new SqlParameter("@Activo",        f.Activo        ?? (object)DBNull.Value),
                new SqlParameter("@Archivo",      SqlDbType.VarBinary,-1){Value=(object?)archivoBytes ?? DBNull.Value},
                new SqlParameter("@RutaArchivo",  SqlDbType.NVarChar,260){Value=(object?)rutaNas ?? DBNull.Value},
                new SqlParameter("@CantidadBienes", f.CantidadBienes ?? (object)DBNull.Value)
            });

            try
            {
                await _ctx.Database.OpenConnectionAsync();
                await cmd.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Factura modificada correctamente." });
            }
            catch (SqlException ex) { return BadRequest(new { ex.Message }); }
            finally { await _ctx.Database.CloseConnectionAsync(); }
        }

        /* ======================= ELIMINAR LÓGICO ======================== */
        [HttpDelete("{idFactura}")]
        public async Task<IActionResult> Delete(long idFactura)
        {
            var sql = "EXEC PA_DEL_FACTURAS @idFactura, @IdPantalla, @IdGeneral";
            await _ctx.Database.ExecuteSqlRawAsync(sql,
                new SqlParameter("@idFactura", idFactura),
                new SqlParameter("@IdPantalla", 1),
                new SqlParameter("@IdGeneral", 1));
            return Ok(new { mensaje = "Factura eliminada." });
        }

        /* ======================= PUBLICAR / DESPUBLICAR ================= */
        [HttpPut("publicar")]
        public async Task<IActionResult> Publicar(long idFactura)
        {
            var nuevoEstadoPublicarParam = new SqlParameter("@NuevoEstadoPublicar", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            var sql = "EXEC PA_PUBLICAR_FACTURAS @idFactura, @IdPantalla, @IdGeneral, @NuevoEstadoPublicar OUTPUT";

            await _ctx.Database.ExecuteSqlRawAsync(sql,
                new SqlParameter("@idFactura", idFactura),
                new SqlParameter("@IdPantalla", 1),
                new SqlParameter("@IdGeneral", 1),
                nuevoEstadoPublicarParam);

            bool nuevoEstado = (bool)nuevoEstadoPublicarParam.Value;
            string mensaje = nuevoEstado ? "Factura publicada correctamente." : "Factura despublicada correctamente.";
            return Ok(new { mensaje });
        }

        /* ======================= DESCARGAR ============================== */
        [HttpGet("archivo/{id}")]
        public async Task<IActionResult> Descargar(long id)
        {
            var f = await _ctx.FACTURAS.FindAsync(id);
            if (f is null) return NotFound();

            /* Preferencia: NAS */
            if (!string.IsNullOrWhiteSpace(f.RutaArchivo) && System.IO.File.Exists(f.RutaArchivo))
            {
                var bytes = await System.IO.File.ReadAllBytesAsync(f.RutaArchivo);
                var fileName = Path.GetFileName(f.RutaArchivo);
                return File(bytes, "application/pdf", fileName);
            }

            /* Alternativa: VARBINARY */
            if (f.Archivo is not null)
                return File(f.Archivo, "application/pdf", $"Factura_{id}.pdf");

            return NotFound();
        }
    }
}
