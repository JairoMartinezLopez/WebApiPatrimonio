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
    public class LevantamientoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LevantamientoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Levantamiento
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Levantamiento>>> GetPAT_LEVANTAMIENTO_INVENTARIO()
        {
            return await _context.LEVANTAMIENTOSINVENTARIO.ToListAsync();
        }

        // GET: api/Levantamiento/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Levantamiento>> GetLevantamiento(long id)
        {
            var levantamiento = await _context.LEVANTAMIENTOSINVENTARIO.FindAsync(id);

            if (levantamiento == null)
            {
                return NotFound();
            }

            return levantamiento;
        }

        [HttpGet("filtar")]
        public async Task<ActionResult<IEnumerable<EventosInventario>>> filtrarLevantamiento(
            [FromQuery] int? idbien,
            [FromQuery] int? idEventoInv,
            [FromQuery] string? observaciones,
            [FromQuery] int? existeBien,
            [FromQuery] DateTime? fechaVerificacion,
            [FromQuery] bool? fueActualizado
            )
        {
            var query = _context.LEVANTAMIENTOSINVENTARIO.AsQueryable();

            if (idbien.HasValue)
                query = query.Where(u => u.idBien == idbien);

            if (idEventoInv.HasValue)
                query = query.Where(u => u.idEventoInventario == idEventoInv);

            if (!string.IsNullOrEmpty(observaciones))
                query = query.Where(u => u.Observaciones.Contains(observaciones));
            
            if (existeBien.HasValue)
                query = query.Where(u => u.ExisteElBien == existeBien);

            if (fechaVerificacion.HasValue)
                query = query.Where(u => u.FechaVerificacion == fechaVerificacion);

            if (fueActualizado.HasValue)
                query = query.Where(u => u.FueActualizado == fueActualizado);

            var levantamientos = await query
                .Select(u => new Levantamiento
                {
                    idLevantamientoInventario = u.idLevantamientoInventario,
                    idBien = u.idBien,
                    idEventoInventario = u.idEventoInventario,
                    Observaciones = u.Observaciones,
                    ExisteElBien = u.ExisteElBien,
                    FechaVerificacion = u.FechaVerificacion,
                    FueActualizado = u.FueActualizado
                }).ToListAsync();
            return Ok(levantamientos);
        }

        // PUT: api/Levantamiento/5
        [HttpPost("insertar")]
        public async Task<ActionResult> InsertarLevantamiento([FromBody] Levantamiento request)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_INS_LEVANTAMIENTOSINVENTARIO";

            command.Parameters.Add(new SqlParameter("@IdPantalla", 1));
            command.Parameters.Add(new SqlParameter("@IdGeneral", 1115));
            command.Parameters.Add(new SqlParameter("@idBien", request.idBien));
            command.Parameters.Add(new SqlParameter("@idEventoInventario", request.idEventoInventario));
            command.Parameters.Add(new SqlParameter("@Observaciones", (object)request.Observaciones ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@ExisteElBien", (object?)request.ExisteElBien ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@FechaVerificacion", (object?)request.FechaVerificacion ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@FueActualizado", (object?)request.FueActualizado ?? DBNull.Value));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Levantamiento de inventario insertado correctamente." });
            }
            catch (SqlException ex)
            {
                // Manejo de errores específicos de SQL (por ejemplo, los RAISERROR del SP)
                return BadRequest(new { error = ex.Message });
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        // PUT: api/LevantamientosInventario/actualizar
        [HttpPut("actualizar")]
        public async Task<ActionResult> ActualizarLevantamiento([FromBody] Levantamiento request)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_UPD_LEVANTAMIENTOSINVENTARIO";

            command.Parameters.Add(new SqlParameter("@IdPantalla", 1));
            command.Parameters.Add(new SqlParameter("@IdGeneral", 1115));
            command.Parameters.Add(new SqlParameter("@idLevantamientoInventario", request.idLevantamientoInventario));
            command.Parameters.Add(new SqlParameter("@Observaciones", (object)request.Observaciones ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@ExisteElBien", (object?)request.ExisteElBien ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@FechaVerificacion", (object?)request.FechaVerificacion ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@FueActualizado", (object?)request.FueActualizado ?? DBNull.Value));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Levantamiento de inventario actualizado correctamente." });
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

        // PUT: api/LevantamientosInventario/actualizar-masivo
        [HttpPut("actualizar-masivo")]
        public async Task<ActionResult> ActualizarLevantamientosMasivos([FromBody] LevantamientoMasivoUpdate request)
        {
            if (request == null || !request.ListaLevantamientos.Any())
            {
                return BadRequest(new { error = "La solicitud debe contener al menos un levantamiento para actualizar." });
            }

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_UPD_LEVANTAMIENTOSINVENTARIO_MASIVO";

            command.Parameters.Add(new SqlParameter("@IdPantalla", request.IdPantalla));
            command.Parameters.Add(new SqlParameter("@IdGeneral", request.IdGeneral));

            // Crear el DataTable para el parámetro con valores de tabla
            DataTable dtLevantamientos = new DataTable();
            dtLevantamientos.Columns.Add("idLevantamientoInventario", typeof(long));
            dtLevantamientos.Columns.Add("Observaciones", typeof(string));
            dtLevantamientos.Columns.Add("ExisteElBien", typeof(int)); // Coincide con int? en tu modelo y TVP INT
            dtLevantamientos.Columns.Add("FechaVerificacion", typeof(DateTime));
            dtLevantamientos.Columns.Add("FueActualizado", typeof(bool)); // Coincide con bool? en tu modelo y TVP BIT

            foreach (var item in request.ListaLevantamientos)
            {
                dtLevantamientos.Rows.Add(
                    item.IdLevantamientoInventario,
                    (object)item.Observaciones ?? DBNull.Value,
                    (object?)item.ExisteElBien ?? DBNull.Value,
                    (object?)item.FechaVerificacion ?? DBNull.Value,
                    (object?)item.FueActualizado ?? DBNull.Value
                );
            }

            SqlParameter tvpParam = new SqlParameter("@ListaLevantamientos", dtLevantamientos);
            tvpParam.SqlDbType = SqlDbType.Structured;
            tvpParam.TypeName = "dbo.TipoLevantamientoInventarioUpdate"; // Asegúrate de que este nombre sea exacto al TVP en SQL Server
            command.Parameters.Add(tvpParam);

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Levantamientos de inventario actualizados masivamente correctamente." });
            }
            catch (SqlException ex)
            {
                // Captura y devuelve el mensaje de error de SQL Server, incluyendo los RAISERROR del SP
                return BadRequest(new { error = ex.Message });
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        // POST: api/LevantamientosInventario/insertar-masivo
        [HttpPost("insertar-masivo")]
        public async Task<ActionResult> InsertarLevantamientosMasivos([FromBody] LevantamientoMasivo request)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_INS_LEVANTAMIENTOSINVENTARIO_MASIVO";

            command.Parameters.Add(new SqlParameter("@IdPantalla", request.IdPantalla));
            command.Parameters.Add(new SqlParameter("@IdGeneral", request.IdGeneral));
            command.Parameters.Add(new SqlParameter("@idEventoInventario", request.IdEventoInventario));

            // Crear el DataTable para el parámetro con valores de tabla (TipoLevantamientoInventario)
            DataTable dtLevantamientos = new DataTable();
            dtLevantamientos.Columns.Add("idBien", typeof(long));
            dtLevantamientos.Columns.Add("Observaciones", typeof(string));
            dtLevantamientos.Columns.Add("ExisteElBien", typeof(int)); // O typeof(bool) si tu SP lo espera como BIT directamente
            dtLevantamientos.Columns.Add("FechaVerificacion", typeof(DateTime));
            dtLevantamientos.Columns.Add("FueActualizado", typeof(bool));


            foreach (var item in request.ListaLevantamientos)
            {
                // Asegúrate de manejar DBNull.Value para los campos que pueden ser NULL
                dtLevantamientos.Rows.Add(
                    item.IdBien,
                    (object)item.Observaciones ?? DBNull.Value,
                    (object?)item.ExisteElBien ?? DBNull.Value,
                    (object?)item.FechaVerificacion ?? DBNull.Value,
                    (object?)item.FueActualizado ?? DBNull.Value
                );
            }

            SqlParameter tvpParam = new SqlParameter("@ListaLevantamientos", dtLevantamientos);
            tvpParam.SqlDbType = SqlDbType.Structured;
            tvpParam.TypeName = "dbo.TipoLevantamientoInventario"; // El nombre del tipo de tabla que creaste en SQL Server
            command.Parameters.Add(tvpParam);

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Levantamientos de inventario masivos insertados correctamente." });
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

        // GET: api/LevantamientosInventario/bienEventos
        [HttpGet("bienesPorArea/{idEventoInventario}")]
        public async Task<ActionResult<IEnumerable<dynamic>>> ObtenerBienesPorAreaEvento(int idEventoInventario)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_SEL_BIENES_POR_AREA_EVENTO";

            command.Parameters.Add(new SqlParameter("@idEventoInventario", idEventoInventario));

            try
            {
                await _context.Database.OpenConnectionAsync();
                using var reader = await command.ExecuteReaderAsync();
                var resultados = new List<dynamic>(); // O crea una clase DTO para BienesAverificarDto
                while (await reader.ReadAsync())
                {
                    resultados.Add(new
                    {
                        idBien = reader["idBien"],
                        NoInventario = reader["NoInventario"],
                        Serie = reader["Serie"],
                        Modelo = reader["Modelo"],
                        Observaciones = reader["Observaciones"], // Observaciones del bien
                        Activo = reader["Activo"],
                        Disponibilidad = reader["Disponibilidad"],
                        //idLevantamientoInventario = reader["idLevantamientoInventario"],
                        ExisteElBien = reader["ExisteElBien"],
                        //FechaVerificacion = reader["FechaVerificacion"],
                        //FueActualizado = reader["FueActualizado"],
                       // ObservacionesLevantamiento = reader["ObservacionesLevantamiento"], // Observaciones de la verificación
                        YaVerificado = reader["YaVerificado"]
                    });
                }
                return Ok(resultados);
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

        // GET: api/LevantamientosInventario/progreso/{idEventoInventario}
        [HttpGet("progreso/{idEventoInventario}")]
        public async Task<ActionResult<IEnumerable<dynamic>>> ObtenerProgresoInventario(int idEventoInventario)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_SEL_PROGRESO_INVENTARIO_AREA";

            command.Parameters.Add(new SqlParameter("@idEventoInventario", idEventoInventario));

            try
            {
                await _context.Database.OpenConnectionAsync();
                using var reader = await command.ExecuteReaderAsync();
                var resultados = new List<dynamic>();
                while (await reader.ReadAsync())
                {
                    // Puedes mapear esto a una clase DTO específica si lo prefieres para una mayor tipificación
                    resultados.Add(new
                    {
                        idEventoInventario = reader["idEventoInventario"],
                        EventoFolio = reader["EventoFolio"],
                        idArea = reader["idArea"],
                        NombreArea = reader["NombreArea"],
                        TotalBienesAsignadosArea = reader["TotalBienesAsignadosArea"],
                        TotalBienesVerificados = reader["TotalBienesVerificados"],
                        TotalBienesEncontrados = reader["TotalBienesEncontrados"],
                        TotalBienesNoEncontrados = reader["TotalBienesNoEncontrados"], // Faltantes
                        PorcentajeVerificado = reader["PorcentajeVerificado"],
                        CantidadFaltantes = reader["CantidadFaltantes"],
                        CantidadSobrantes = reader["CantidadSobrantes"]
                    });
                }
                return Ok(resultados);
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

        // GET: api/LevantamientosInventario/bienes-comprobados/{idEventoInventario}
        [HttpGet("bienes-comprobados/{idEventoInventario}")]
        public async Task<ActionResult<IEnumerable<dynamic>>> ObtenerBienesComprobadosEnInventario(int idEventoInventario)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_SEL_BIENES_COMPROBADOS_EN_INVENTARIO";

            command.Parameters.Add(new SqlParameter("@idEventoInventario", idEventoInventario));

            try
            {
                await _context.Database.OpenConnectionAsync();
                using var reader = await command.ExecuteReaderAsync();
                var resultados = new List<dynamic>();
                while (await reader.ReadAsync())
                {
                    resultados.Add(new
                    {
                        idLevantamientoInventario = reader["idLevantamientoInventario"],
                        idBien = reader["idBien"],
                        NoInventario = reader["NoInventario"],
                        Marca = reader["Marca"],
                        Modelo = reader["Modelo"],
                        Serie = reader["Serie"],
                        ObservacionesLevantamiento = reader["ObservacionesLevantamiento"],
                        FechaVerificacion = reader["FechaVerificacion"],
                        AreaDelEvento = reader["AreaDelEvento"]
                    });
                }
                return Ok(resultados);
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

        // GET: api/LevantamientosInventario/sobrantes/{idEventoInventario}
        [HttpGet("sobrantes/{idEventoInventario}")]
        public async Task<ActionResult<IEnumerable<dynamic>>> ObtenerBienesSobrantes(int idEventoInventario)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_SEL_BIENES_SOBRANTES";

            command.Parameters.Add(new SqlParameter("@idEventoInventario", idEventoInventario));

            try
            {
                await _context.Database.OpenConnectionAsync();
                using var reader = await command.ExecuteReaderAsync();
                var resultados = new List<dynamic>();
                while (await reader.ReadAsync())
                {
                    resultados.Add(new
                    {
                        idBien = reader["idBien"],
                        NoInventario = reader["NoInventario"],
                        Serie = reader["Serie"],
                        Modelo = reader["Modelo"],
                        Observaciones = reader["Observaciones"],
                        FechaVerificacion = reader["FechaVerificacion"],
                        AreaReportada = reader["AreaReportada"]
                    });
                }
                return Ok(resultados);
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

        // GET: api/LevantamientosInventario/faltantes/{idEventoInventario}
        [HttpGet("faltantes/{idEventoInventario}")]
        public async Task<ActionResult<IEnumerable<dynamic>>> ObtenerBienesFaltantes(int idEventoInventario)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_SEL_BIENES_FALTANTES";

            command.Parameters.Add(new SqlParameter("@idEventoInventario", idEventoInventario));

            try
            {
                await _context.Database.OpenConnectionAsync();
                using var reader = await command.ExecuteReaderAsync();
                var resultados = new List<dynamic>();
                while (await reader.ReadAsync())
                {
                    resultados.Add(new
                    {
                        idBien = reader["idBien"],
                        NoInventario = reader["NoInventario"],
                        Serie = reader["Serie"],
                        Modelo = reader["Modelo"],
                        Observaciones = reader["Observaciones"],
                        FechaVerificacion = reader["FechaVerificacion"]
                    });
                }
                return Ok(resultados);
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

        // DELETE: api/Levantamiento/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLevantamiento(long id)
        {
            var levantamiento = await _context.LEVANTAMIENTOSINVENTARIO.FindAsync(id);
            if (levantamiento == null)
            {
                return NotFound();
            }

            _context.LEVANTAMIENTOSINVENTARIO.Remove(levantamiento);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LevantamientoExists(long id)
        {
            return _context.LEVANTAMIENTOSINVENTARIO.Any(e => e.idLevantamientoInventario == id);
        }
    }
}
