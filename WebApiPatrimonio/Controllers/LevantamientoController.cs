using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WebApiPatrimonio.Context;
using WebApiPatrimonio.Models;

namespace WebApiPatrimonio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LevantamientoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LevantamientoController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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

        [HttpGet("levantamientosPorEvento/{idEventoInventario}")]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetLevantamientosByEvento(int idEventoInventario)
        {
            await using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_SEL_LEVANTAMIENTOSINVENTARIO_BY_EVENTO";

            // Agrega el parámetro para el procedimiento almacenado
            command.Parameters.Add(new SqlParameter("@idEventoInventario", idEventoInventario));

            try
            {
                await _context.Database.OpenConnectionAsync(); // Abre la conexión de forma asíncrona
                await using var reader = await command.ExecuteReaderAsync(); // Ejecuta el comando y obtiene un reader
                var resultados = new List<dynamic>();

                while (await reader.ReadAsync()) // Lee cada fila devuelta por el procedimiento almacenado
                {
                    resultados.Add(new
                    {
                        idLevantamientoInventario = reader["idLevantamientoInventario"],
                        idBien = reader["idBien"],
                        idEventoInventario = reader["idEventoInventario"],
                        NoInventario = reader["NoInventario"],
                        Serie = reader["Serie"],
                        Modelo = reader["Modelo"],
                        Color = reader["Color"], // Ahora leerá el nombre del color
                        Marca = reader["Marca"], // Ahora leerá el nombre de la marca
                        Observaciones = reader["Observaciones"],
                        ExisteElBien = reader["ExisteElBien"],
                        FechaVerificacion = reader["FechaVerificacion"],
                        FueActualizado = reader["FueActualizado"]
                    });
                }

                return Ok(resultados); // Retorna 200 OK con los datos
            }
            catch (SqlException ex)
            {
                // Para excepciones SQL, puedes manejar el RAISERROR específico o un error general
                if (ex.Message.Contains("El ID de Evento de Inventario especificado no existe."))
                {
                    return NotFound(new { error = ex.Message }); // Retorna 404 si el ID no existe
                }
                return BadRequest(new { error = ex.Message }); // Retorna 400 Bad Request para otros errores SQL
            }
            finally
            {
                // Asegura que la conexión se cierre, incluso si ocurre un error
                await _context.Database.CloseConnectionAsync();
            }
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

        // PUT: api/Levantamiento/actualizar-masivo
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

        // POST: api/Levantamiento/insertar-masivo
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

        // GET: api/Levantamientos/bienEventos
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
                var resultados = new List<dynamic>(); 
                while (await reader.ReadAsync())
                {
                    resultados.Add(new
                    {
                        idBien = reader["idBien"],
                        NoInventario = reader["NoInventario"],
                        Serie = reader["Serie"],
                        Modelo = reader["Modelo"],
                        Color = reader["Color"],
                        Marca = reader["Marca"],
                        NoFactura = reader["NoFactura"],
                        //Observaciones = reader["Observaciones"], // Observaciones del bien
                        Activo = reader["Activo"],
                        Disponibilidad = reader["Disponibilidad"],
                        //idLevantamientoInventario = reader["idLevantamientoInventario"],
                        ExisteElBien = reader["ExisteElBien"],
                        //FechaVerificacion = reader["FechaVerificacion"],
                        //FueActualizado = reader["FueActualizado"],
                        ObservacionesLevantamiento = reader["ObservacionesLevantamiento"], // Observaciones de la verificación
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

        // GET: api/Levantamientos/progreso/{idEventoInventario}
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
                string nombreAreaTitulo = string.Empty;
                if (await reader.ReadAsync()) // Read the first row from the first result set
                {
                    int ordinal = reader.GetOrdinal("NombreAreaParaTitulo");
                    if (!reader.IsDBNull(ordinal))
                    {
                        nombreAreaTitulo = reader.GetString(ordinal);
                    }
                }
                await reader.NextResultAsync();
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

        // GET: api/Reportes/ReporteBienesVerificadosPDF
        [HttpGet("ReporteBienesVerificadosPDF")]
        public async Task<IActionResult> GenerarReporteBienesVerificadosPDF(
            [FromQuery] int idEventoInventario)
        {
            try
            {
                var bienesVerificadosData = new List<(/*long idLevantamiento, long idBien,*/ string NoInventario, string NombreMarca, string Modelo, string Serie, string ObservacionesLevantamiento, DateTime FechaVerificacion/*, string AreaReportada*/)>();
                string nombreAreaReporte = "";

                using (var connection = _context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "PA_SEL_BIENES_COMPROBADOS_EN_INVENTARIO";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@idEventoInventario", idEventoInventario));

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync() && reader.FieldCount > 0)
                            {
                                var ordinal = reader.GetOrdinal("NombreAreaParaTitulo");
                                if (!reader.IsDBNull(ordinal))
                                {
                                    nombreAreaReporte = reader.GetString(ordinal);
                                }
                            }

                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    bienesVerificadosData.Add((
                                        //reader.GetInt64(reader.GetOrdinal("idLevantamientoInventario")),
                                        //reader.GetInt64(reader.GetOrdinal("idBien")),
                                        reader["NoInventario"]?.ToString(),
                                        reader["NombreMarca"]?.ToString(),
                                        reader["Modelo"]?.ToString(),
                                        reader["Serie"]?.ToString(),
                                        reader["ObservacionesLevantamiento"]?.ToString(),
                                        reader.GetDateTime(reader.GetOrdinal("FechaVerificacion"))
                                    //reader["AreaDelEvento"]?.ToString()
                                    ));
                                }
                            }
                        }
                    }
                }

                using var ms = new MemoryStream();
                using var document = new PdfDocument();
                document.Info.Title = "Reporte de Bienes Verificados"; // New document title

                PdfPage page = document.AddPage();
                //page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                //page.Size = PdfSharpCore.PageSize.Legal;

                XGraphics gfx = XGraphics.FromPdfPage(page);

                double contentMarginTop = 150;
                double contentMarginBottom = 70;
                double contentMarginLeft = 0;
                double contentMarginRight = 0;

                XFont tableHeaderFont = new XFont("Arial", 8, XFontStyle.Bold);
                XFont tableContentFont = new XFont("Arial", 7, XFontStyle.Regular);
                double rowHeightBase = 18;
                double cellPadding = 3;
                double lineHeight = 10;

                XPen solidBorderPen = XPens.Black;
                XPen dottedBottomBorderPen = new XPen(XColor.FromArgb(150, 0, 0, 0), 0.5);
                dottedBottomBorderPen.DashStyle = XDashStyle.Dot;

                // Define columns and their widths for "Bienes Sobrantes" report
                var columnHeaders = new (string Name, double WidthFactor)[]
                {
                    ("NO. INVENTARIO", 0.35),
                    ("MARCA", 0.2),
                    ("MODELO", 0.2),
                    ("SERIE", 0.2),
                    ("OBSERVACIONES", 0.45),
                    ("FECHA VERIFICACIÓN", 0.30),
                    //("ÁREA DEL EVENTO", 0.15)
                };

                double totalFactor = columnHeaders.Sum(ch => ch.WidthFactor);
                double availableContentWidth = page.Width - 80;
                double[] colWidths = columnHeaders.Select(ch => ch.WidthFactor * availableContentWidth / totalFactor).ToArray();
                double actualTableWidth = colWidths.Sum();
                contentMarginLeft = (page.Width - actualTableWidth) / 2;
                contentMarginRight = contentMarginLeft;

                int pageNumber = 1;
                double currentY = contentMarginTop;
                double currentX;

                Action DrawTableHeaders = () =>
                {
                    currentX = contentMarginLeft;
                    XSolidBrush grayBrush = new XSolidBrush(XColors.LightGray);
                    for (int i = 0; i < columnHeaders.Length; i++)
                    {
                        gfx.DrawRectangle(grayBrush, currentX, currentY, colWidths[i], rowHeightBase);
                        gfx.DrawRectangle(solidBorderPen, currentX, currentY, colWidths[i], rowHeightBase);
                        gfx.DrawString(columnHeaders[i].Name, tableHeaderFont, XBrushes.Black,
                            new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[i] - cellPadding * 2, rowHeightBase - cellPadding * 2), XStringFormats.Center);
                        currentX += colWidths[i];
                    }
                    currentY += rowHeightBase;
                };

                DrawHeader(gfx, page, "REPORTE DE BIENES VERIFICADOS", "BIENES VERIFICADOS", 0, 0, nombreAreaReporte, pageNumber, 1);
                DrawFooter(gfx, page);
                DrawTableHeaders();

                foreach (var item in bienesVerificadosData)
                {
                    string[] textos = {
                    item.NoInventario,
                    item.NombreMarca,
                    item.Modelo,
                    item.Serie,
                    item.ObservacionesLevantamiento,
                    item.FechaVerificacion.ToString("dd/MM/yyyy"),
                    //item.AreaReportada
                    };

                    double maxLineCount = 1;
                    List<List<string>> wrappedTexts = new List<List<string>>();

                    for (int i = 0; i < textos.Length; i++)
                    {
                        string text = textos[i] ?? string.Empty;
                        double cellWidth = colWidths[i];
                        // Estimate characters per line (adjust 1.0/0.5 based on your font and content)
                        int aproxCharPerLine = (int)(cellWidth / tableContentFont.Size * (1.0 / 0.5));
                        if (aproxCharPerLine <= 0) aproxCharPerLine = 1;

                        var lines = WrapText(text, aproxCharPerLine);
                        wrappedTexts.Add(lines);
                        maxLineCount = Math.Max(maxLineCount, lines.Count);
                    }

                    double adjustedRowHeight = maxLineCount * lineHeight + (cellPadding * 2);

                    if (currentY + adjustedRowHeight > page.Height - contentMarginBottom)
                    {
                        pageNumber++;
                        page = document.AddPage();
                        //page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                        //page.Size = PdfSharpCore.PageSize.Legal;
                        gfx = XGraphics.FromPdfPage(page);
                        availableContentWidth = page.Width - 80;
                        colWidths = columnHeaders.Select(ch => ch.WidthFactor * availableContentWidth / totalFactor).ToArray();
                        actualTableWidth = colWidths.Sum();
                        contentMarginLeft = (page.Width - actualTableWidth) / 2;

                        DrawHeader(gfx, page, "REPORTE DE BIENES VERIFICADOS", "BIENES VERIFICADOS", 0, 0, nombreAreaReporte, pageNumber, 1);
                        DrawFooter(gfx, page);
                        currentY = contentMarginTop;
                        DrawTableHeaders();
                    }

                    currentX = contentMarginLeft;
                    for (int colIndex = 0; colIndex < wrappedTexts.Count; colIndex++)
                    {
                        var lines = wrappedTexts[colIndex];
                        double cellWidth = colWidths[colIndex];
                        double textHeight = lines.Count * lineHeight;
                        double yOffset = currentY + (adjustedRowHeight - textHeight) / 2 + 4; // Center vertically

                        gfx.DrawRectangle(solidBorderPen, currentX, currentY, cellWidth, adjustedRowHeight);

                        for (int i = 0; i < lines.Count; i++)
                        {
                            XStringFormat stringFormat = XStringFormats.TopLeft;
                            // Adjust alignment if needed for specific columns (e.g., numbers)
                            if (columnHeaders[colIndex].Name == "FECHA VERIFICACIÓN" || columnHeaders[colIndex].Name == "NO. INVENTARIO")
                            {
                                stringFormat = XStringFormats.TopCenter;
                            }

                            gfx.DrawString(lines[i], tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, yOffset + i * lineHeight, cellWidth - cellPadding * 2, lineHeight),
                                stringFormat);
                        }
                        currentX += cellWidth;
                    }
                    if (item != bienesVerificadosData.Last())
                    {
                        gfx.DrawLine(dottedBottomBorderPen, contentMarginLeft, currentY + adjustedRowHeight, contentMarginLeft + actualTableWidth, currentY + adjustedRowHeight);
                    }
                    currentY += adjustedRowHeight;

                }

                document.Save(ms);
                ms.Position = 0;
                return File(ms.ToArray(), "application/pdf", $"ReporteBienesVerificadoss_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar reporte de bienes verificados: {ex.Message}");
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
                string nombreAreaTitulo = string.Empty;
                if (await reader.ReadAsync()) // Read the first row from the first result set
                {
                    int ordinal = reader.GetOrdinal("NombreAreaParaTitulo");
                    if (!reader.IsDBNull(ordinal))
                    {
                        nombreAreaTitulo = reader.GetString(ordinal);
                    }
                }
                await reader.NextResultAsync();
                var resultados = new List<dynamic>();
                while (await reader.ReadAsync())
                {
                    resultados.Add(new
                    {
                        idBien = reader["idBien"],
                        NoInventario = reader["NoInventario"],
                        Marca = reader["NombreMarca"],
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

        // GET: api/Reportes/ReporteBienesSobrantesPDF
        [HttpGet("ReporteBienesSobrantesPDF")]
        public async Task<IActionResult> GenerarReporteBienesSobrantesPDF(
            [FromQuery] int idEventoInventario)
        {
            try
            {
                // Data structure to hold the results from PA_SEL_BIENES_SOBRANTES
                var bienesSobrantesData = new List<(long idBien, string NoInventario, string NombreMarca, string Serie, string Modelo, string Observaciones, DateTime FechaVerificacion/*, string AreaReportada*/)>();
                string nombreAreaReporte = "";

                using (var connection = _context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "PA_SEL_BIENES_SOBRANTES";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@idEventoInventario", idEventoInventario));

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync() && reader.FieldCount > 0)
                            {
                                var ordinal = reader.GetOrdinal("NombreAreaParaTitulo");
                                if (!reader.IsDBNull(ordinal))
                                {
                                    nombreAreaReporte = reader.GetString(ordinal);
                                }
                            }

                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    bienesSobrantesData.Add((
                                        reader.GetInt64(reader.GetOrdinal("idBien")),
                                        reader["NoInventario"]?.ToString(),
                                        reader["NombreMarca"]?.ToString(),
                                        reader["Serie"]?.ToString(),
                                        reader["Modelo"]?.ToString(),
                                        reader["Observaciones"]?.ToString(),
                                        reader.GetDateTime(reader.GetOrdinal("FechaVerificacion"))
                                        //reader["AreaReportada"]?.ToString()
                                    ));
                                }
                            }
                        }
                    }
                }

                using var ms = new MemoryStream();
                using var document = new PdfDocument();
                document.Info.Title = "Reporte de Bienes Sobrantes"; // New document title

                PdfPage page = document.AddPage();
                //page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                //page.Size = PdfSharpCore.PageSize.Legal;

                XGraphics gfx = XGraphics.FromPdfPage(page);

                double contentMarginTop = 150;
                double contentMarginBottom = 70;
                double contentMarginLeft = 0;
                double contentMarginRight = 0;

                XFont tableHeaderFont = new XFont("Arial", 8, XFontStyle.Bold);
                XFont tableContentFont = new XFont("Arial", 7, XFontStyle.Regular);
                double rowHeightBase = 18;
                double cellPadding = 3;
                double lineHeight = 10;

                XPen solidBorderPen = XPens.Black;
                XPen dottedBottomBorderPen = new XPen(XColor.FromArgb(150, 0, 0, 0), 0.5);
                dottedBottomBorderPen.DashStyle = XDashStyle.Dot;

                // Define columns and their widths for "Bienes Sobrantes" report
                var columnHeaders = new (string Name, double WidthFactor)[]
                {
                ("NO. INVENTARIO", 0.35),
                ("MARCA", 0.2),
                ("SERIE", 0.2),
                ("MODELO", 0.2),
                ("OBSERVACIONES", 0.45),
                ("FECHA VERIFICACIÓN", 0.30),
                //("ÁREA REPORTADA", 0.15)
                };

                double totalFactor = columnHeaders.Sum(ch => ch.WidthFactor);
                double availableContentWidth = page.Width - 80;
                double[] colWidths = columnHeaders.Select(ch => ch.WidthFactor * availableContentWidth / totalFactor).ToArray();
                double actualTableWidth = colWidths.Sum();
                contentMarginLeft = (page.Width - actualTableWidth) / 2;
                contentMarginRight = contentMarginLeft;

                int pageNumber = 1;
                double currentY = contentMarginTop;
                double currentX;

                Action DrawTableHeaders = () =>
                {
                    currentX = contentMarginLeft;
                    XSolidBrush grayBrush = new XSolidBrush(XColors.LightGray);
                    for (int i = 0; i < columnHeaders.Length; i++)
                    {
                        gfx.DrawRectangle(grayBrush, currentX, currentY, colWidths[i], rowHeightBase);
                        gfx.DrawRectangle(solidBorderPen, currentX, currentY, colWidths[i], rowHeightBase);
                        gfx.DrawString(columnHeaders[i].Name, tableHeaderFont, XBrushes.Black,
                            new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[i] - cellPadding * 2, rowHeightBase - cellPadding * 2), XStringFormats.Center);
                        currentX += colWidths[i];
                    }
                    currentY += rowHeightBase;
                };

                DrawHeader(gfx, page, "REPORTE DE BIENES SOBRANTES", "BIENES SOBRANTES", 0, 0, nombreAreaReporte, pageNumber, 1);
                DrawFooter(gfx, page);
                DrawTableHeaders();

                foreach (var item in bienesSobrantesData)
                {
                    string[] textos = {
                    item.NoInventario,
                    item.NombreMarca,
                    item.Serie,
                    item.Modelo,
                    item.Observaciones,
                    item.FechaVerificacion.ToString("dd/MM/yyyy"),
                    //item.AreaReportada
                };

                    double maxLineCount = 1;
                    List<List<string>> wrappedTexts = new List<List<string>>();

                    for (int i = 0; i < textos.Length; i++)
                    {
                        string text = textos[i] ?? string.Empty;
                        double cellWidth = colWidths[i];
                        // Estimate characters per line (adjust 1.0/0.5 based on your font and content)
                        int aproxCharPerLine = (int)(cellWidth / tableContentFont.Size * (1.0 / 0.5));
                        if (aproxCharPerLine <= 0) aproxCharPerLine = 1;

                        var lines = WrapText(text, aproxCharPerLine);
                        wrappedTexts.Add(lines);
                        maxLineCount = Math.Max(maxLineCount, lines.Count);
                    }

                    double adjustedRowHeight = maxLineCount * lineHeight + (cellPadding * 2);

                    if (currentY + adjustedRowHeight > page.Height - contentMarginBottom)
                    {
                        pageNumber++;
                        page = document.AddPage();
                        //page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                        //page.Size = PdfSharpCore.PageSize.Legal;
                        gfx = XGraphics.FromPdfPage(page);
                        availableContentWidth = page.Width - 80;
                        colWidths = columnHeaders.Select(ch => ch.WidthFactor * availableContentWidth / totalFactor).ToArray();
                        actualTableWidth = colWidths.Sum();
                        contentMarginLeft = (page.Width - actualTableWidth) / 2;

                        DrawHeader(gfx, page, "REPORTE DE BIENES SOBRANTES", "BIENES SOBRANTES", 0, 0, nombreAreaReporte, pageNumber, 1);
                        DrawFooter(gfx, page);
                        currentY = contentMarginTop;
                        DrawTableHeaders();
                    }

                    currentX = contentMarginLeft;
                    for (int colIndex = 0; colIndex < wrappedTexts.Count; colIndex++)
                    {
                        var lines = wrappedTexts[colIndex];
                        double cellWidth = colWidths[colIndex];
                        double textHeight = lines.Count * lineHeight;
                        double yOffset = currentY + (adjustedRowHeight - textHeight) / 2 + 4; // Center vertically

                        gfx.DrawRectangle(solidBorderPen, currentX, currentY, cellWidth, adjustedRowHeight);

                        for (int i = 0; i < lines.Count; i++)
                        {
                            XStringFormat stringFormat = XStringFormats.TopLeft;
                            // Adjust alignment if needed for specific columns (e.g., numbers)
                            if (columnHeaders[colIndex].Name == "FECHA VERIFICACIÓN" || columnHeaders[colIndex].Name == "NO. INVENTARIO")
                            {
                                stringFormat = XStringFormats.TopCenter;
                            }

                            gfx.DrawString(lines[i], tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, yOffset + i * lineHeight, cellWidth - cellPadding * 2, lineHeight),
                                stringFormat);
                        }
                        currentX += cellWidth;
                    }
                    if (item != bienesSobrantesData.Last())
                    {
                        gfx.DrawLine(dottedBottomBorderPen, contentMarginLeft, currentY + adjustedRowHeight, contentMarginLeft + actualTableWidth, currentY + adjustedRowHeight);
                    }
                    currentY += adjustedRowHeight;
                
                }

                document.Save(ms);
                ms.Position = 0;
                return File(ms.ToArray(), "application/pdf", $"ReporteBienesSobrantes_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar reporte de bienes sobrantes: {ex.Message}");
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
                string nombreAreaTitulo = string.Empty;
                if (await reader.ReadAsync()) // Read the first row from the first result set
                {
                    int ordinal = reader.GetOrdinal("NombreAreaParaTitulo");
                    if (!reader.IsDBNull(ordinal))
                    {
                        nombreAreaTitulo = reader.GetString(ordinal);
                    }
                }
                await reader.NextResultAsync();
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

        // GET: api/Reportes/ReporteBienesfaltantesPDF
        [HttpGet("ReporteBienesFaltantesPDF")]
        public async Task<IActionResult> GenerarReporteBienesFaltantesPDF(
            [FromQuery] int idEventoInventario)
        {
            try
            {
                // Data structure to hold the results from PA_SEL_BIENES_SOBRANTES
                var bienesFaltantesData = new List<(long idBien, string NoInventario, string NombreMarca, string Serie, string Modelo, string Observaciones, DateTime FechaVerificacion/*, string AreaReportada*/)>();
                string nombreAreaReporte = "";

                using (var connection = _context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "PA_SEL_BIENES_FALTANTES";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@idEventoInventario", idEventoInventario));

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync() && reader.FieldCount > 0)
                            {
                                var ordinal = reader.GetOrdinal("NombreAreaParaTitulo");
                                if (!reader.IsDBNull(ordinal))
                                {
                                    nombreAreaReporte = reader.GetString(ordinal);
                                }
                            }

                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    bienesFaltantesData.Add((
                                        reader.GetInt64(reader.GetOrdinal("idBien")),
                                        reader["NoInventario"]?.ToString(),
                                        reader["NombreMarca"]?.ToString(),
                                        reader["Serie"]?.ToString(),
                                        reader["Modelo"]?.ToString(),
                                        reader["Observaciones"]?.ToString(),
                                        reader.GetDateTime(reader.GetOrdinal("FechaVerificacion"))
                                    //reader["AreaReportada"]?.ToString()
                                    ));
                                }
                            }
                        }
                    }
                }

                using var ms = new MemoryStream();
                using var document = new PdfDocument();
                document.Info.Title = "Reporte de Bienes Faltantes"; // New document title

                PdfPage page = document.AddPage();
                //page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                //page.Size = PdfSharpCore.PageSize.Legal;

                XGraphics gfx = XGraphics.FromPdfPage(page);

                double contentMarginTop = 150;
                double contentMarginBottom = 70;
                double contentMarginLeft = 0;
                double contentMarginRight = 0;

                XFont tableHeaderFont = new XFont("Arial", 8, XFontStyle.Bold);
                XFont tableContentFont = new XFont("Arial", 7, XFontStyle.Regular);
                double rowHeightBase = 18;
                double cellPadding = 3;
                double lineHeight = 10;

                XPen solidBorderPen = XPens.Black;
                XPen dottedBottomBorderPen = new XPen(XColor.FromArgb(150, 0, 0, 0), 0.5);
                dottedBottomBorderPen.DashStyle = XDashStyle.Dot;

                // Define columns and their widths for "Bienes Sobrantes" report
                var columnHeaders = new (string Name, double WidthFactor)[]
                {
                ("NO. INVENTARIO", 0.35),
                ("MARCA", 0.15),
                ("SERIE", 0.30),
                ("MODELO", 0.3),
                ("OBSERVACIONES", 0.30),
                ("FECHA VERIFICACIÓN", 0.30),
                //("ÁREA REPORTADA", 0.15)
                };

                double totalFactor = columnHeaders.Sum(ch => ch.WidthFactor);
                double availableContentWidth = page.Width - 80;
                double[] colWidths = columnHeaders.Select(ch => ch.WidthFactor * availableContentWidth / totalFactor).ToArray();
                double actualTableWidth = colWidths.Sum();
                contentMarginLeft = (page.Width - actualTableWidth) / 2;
                contentMarginRight = contentMarginLeft;

                int pageNumber = 1;
                double currentY = contentMarginTop;
                double currentX;

                Action DrawTableHeaders = () =>
                {
                    currentX = contentMarginLeft;
                    XSolidBrush grayBrush = new XSolidBrush(XColors.LightGray);
                    for (int i = 0; i < columnHeaders.Length; i++)
                    {
                        gfx.DrawRectangle(grayBrush, currentX, currentY, colWidths[i], rowHeightBase);
                        gfx.DrawRectangle(solidBorderPen, currentX, currentY, colWidths[i], rowHeightBase);
                        gfx.DrawString(columnHeaders[i].Name, tableHeaderFont, XBrushes.Black,
                            new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[i] - cellPadding * 2, rowHeightBase - cellPadding * 2), XStringFormats.Center);
                        currentX += colWidths[i];
                    }
                    currentY += rowHeightBase;
                };

                DrawHeader(gfx, page, "REPORTE DE BIENES FALTANTES", "BIENES FALTANTES", 0, 0, nombreAreaReporte, pageNumber, 1);
                DrawFooter(gfx, page);
                DrawTableHeaders();

                foreach (var item in bienesFaltantesData)
                {
                    string[] textos = {
                    item.NoInventario,
                    item.NombreMarca,
                    item.Serie,
                    item.Modelo,
                    item.Observaciones,
                    item.FechaVerificacion.ToString("dd/MM/yyyy"),
                    //item.AreaReportada
                };

                    double maxLineCount = 1;
                    List<List<string>> wrappedTexts = new List<List<string>>();

                    for (int i = 0; i < textos.Length; i++)
                    {
                        string text = textos[i] ?? string.Empty;
                        double cellWidth = colWidths[i];
                        // Estimate characters per line (adjust 1.0/0.5 based on your font and content)
                        int aproxCharPerLine = (int)(cellWidth / tableContentFont.Size * (1.0 / 0.65));
                        if (aproxCharPerLine <= 0) aproxCharPerLine = 1;

                        var lines = WrapText(text, aproxCharPerLine);
                        wrappedTexts.Add(lines);
                        maxLineCount = Math.Max(maxLineCount, lines.Count);
                    }

                    double adjustedRowHeight = maxLineCount * lineHeight + (cellPadding * 2);

                    if (currentY + adjustedRowHeight > page.Height - contentMarginBottom)
                    {
                        pageNumber++;
                        page = document.AddPage();
                        //page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                        //page.Size = PdfSharpCore.PageSize.Legal;
                        gfx = XGraphics.FromPdfPage(page);
                        availableContentWidth = page.Width - 80;
                        colWidths = columnHeaders.Select(ch => ch.WidthFactor * availableContentWidth / totalFactor).ToArray();
                        actualTableWidth = colWidths.Sum();
                        contentMarginLeft = (page.Width - actualTableWidth) / 2;

                        DrawHeader(gfx, page, "REPORTE DE BIENES FALTANTES", "BIENES FALTANTES", 0, 0, nombreAreaReporte, pageNumber, 1);
                        DrawFooter(gfx, page);
                        currentY = contentMarginTop;
                        DrawTableHeaders();
                    }

                    currentX = contentMarginLeft;
                    for (int colIndex = 0; colIndex < wrappedTexts.Count; colIndex++)
                    {
                        var lines = wrappedTexts[colIndex];
                        double cellWidth = colWidths[colIndex];
                        double textHeight = lines.Count * lineHeight;
                        double yOffset = currentY + (adjustedRowHeight - textHeight) / 2 + 4; // Center vertically

                        gfx.DrawRectangle(solidBorderPen, currentX, currentY, cellWidth, adjustedRowHeight);

                        for (int i = 0; i < lines.Count; i++)
                        {
                            XStringFormat stringFormat = XStringFormats.TopLeft;
                            // Adjust alignment if needed for specific columns (e.g., numbers)
                            if (columnHeaders[colIndex].Name == "FECHA VERIFICACIÓN" || columnHeaders[colIndex].Name == "NO. INVENTARIO")
                            {
                                stringFormat = XStringFormats.TopCenter;
                            }

                            gfx.DrawString(lines[i], tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, yOffset + i * lineHeight, cellWidth - cellPadding * 2, lineHeight),
                                stringFormat);
                        }
                        currentX += cellWidth;
                    }
                    if (item != bienesFaltantesData.Last())
                    {
                        gfx.DrawLine(dottedBottomBorderPen, contentMarginLeft, currentY + adjustedRowHeight, contentMarginLeft + actualTableWidth, currentY + adjustedRowHeight);
                    }
                    currentY += adjustedRowHeight;

                }

                document.Save(ms);
                ms.Position = 0;
                return File(ms.ToArray(), "application/pdf", $"ReporteBienesFaltantes_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar reporte de bienes faltantes: {ex.Message}");
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

        [HttpPost("procesar-levantamientos-masivos")] 
        public async Task<ActionResult> ProcesarLevantamientosMasivos([FromBody] LevantamientoMergeRequest request)
        {
            if (request == null || !request.ListaLevantamientos.Any())
            {
                return BadRequest(new { error = "La solicitud debe contener al menos un levantamiento para procesar." });
            }

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_UPSERT_LEVANTAMIENTOSINVENTARIO_MASIVO";
            command.CommandTimeout = 120;

            command.Parameters.Add(new SqlParameter("@IdPantalla", request.IdPantalla));
            command.Parameters.Add(new SqlParameter("@IdGeneral", request.IdGeneral));
            command.Parameters.Add(new SqlParameter("@idEventoInventario", request.IdEventoInventario));

            DataTable dtLevantamientos = new DataTable();
            dtLevantamientos.Columns.Add("idBien", typeof(long));
            dtLevantamientos.Columns.Add("idLevantamientoInventario", typeof(long));
            dtLevantamientos.Columns.Add("Observaciones", typeof(string));
            dtLevantamientos.Columns.Add("ExisteElBien", typeof(int));
            dtLevantamientos.Columns.Add("FechaVerificacion", typeof(DateTime));
            dtLevantamientos.Columns.Add("FueActualizado", typeof(bool));

            foreach (var item in request.ListaLevantamientos)
            {
                dtLevantamientos.Rows.Add(
                    item.IdBien,
                    (object?)item.IdLevantamientoInventario ?? DBNull.Value,
                    (object)item.Observaciones ?? DBNull.Value,
                    (object?)item.ExisteElBien ?? DBNull.Value,
                    (object?)item.FechaVerificacion ?? DBNull.Value,
                    (object?)item.FueActualizado ?? DBNull.Value
                );
            }

            SqlParameter tvpParam = new SqlParameter("@ListaLevantamientos", dtLevantamientos);
            tvpParam.SqlDbType = SqlDbType.Structured;
            tvpParam.TypeName = "dbo.TipoLevantamientoInventarioMerge"; // El nombre del nuevo TVP
            command.Parameters.Add(tvpParam);

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Levantamientos de inventario procesados masivamente correctamente." });
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

        List<string> WrapText(string text, int maxCharsPerLine)
        {
            var words = text.Split(new char[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries); // Split by space and newline
            var lines = new List<string>();
            var currentLine = "";

            foreach (var word in words)
            {
                // If adding the next word (plus a space) exceeds maxCharsPerLine
                if ((currentLine + word).Length + (currentLine.Length > 0 ? 1 : 0) <= maxCharsPerLine)
                {
                    currentLine += (currentLine.Length > 0 ? " " : "") + word;
                }
                else
                {
                    // Add currentLine to lines, if not empty
                    if (!string.IsNullOrEmpty(currentLine))
                    {
                        lines.Add(currentLine.Trim());
                    }
                    // Start new line with the current word
                    currentLine = word;
                }
            }

            // Add the last line if it's not empty
            if (!string.IsNullOrEmpty(currentLine))
                lines.Add(currentLine.Trim());

            return lines;
        }

        private void DrawHeader(XGraphics gfx, PdfPage page, string reportTitle, string tipoReporte, int anio, int mes, string area, int currentPage, int totalPages)
        {
            string logoPathLeft = Path.Combine(_webHostEnvironment.WebRootPath, "images", "consejo_judicatura.png"); // Asegúrate de que el nombre del archivo sea correcto
            string logoPathRight = Path.Combine(_webHostEnvironment.WebRootPath, "images", "consejo_judicatura.png"); // Si tienes un segundo logo, ajústalo

            XImage logoLeft = null;
            XImage logoRight = null;

            if (System.IO.File.Exists(logoPathLeft))
            {
                logoLeft = XImage.FromFile(logoPathLeft);
            }

            if (System.IO.File.Exists(logoPathRight))
            {
                logoRight = XImage.FromFile(logoPathRight);
            }

            XFont fontNormal = new XFont("Arial", 8, XFontStyle.Regular);
            XFont fontBold = new XFont("Arial", 10, XFontStyle.Bold);
            XFont fontTitle = new XFont("Arial", 12, XFontStyle.Bold);
            XFont fontSubTitle = new XFont("Arial", 11, XFontStyle.Regular);

            double x = 40; // Margen izquierdo
            double y = 30; // Margen superior

            if (logoLeft != null)
            {
                gfx.DrawImage(logoLeft, x, y, 60, 60);
            }
            if (logoRight != null)
            {
                gfx.DrawImage(logoRight, page.Width - x - 50, y, 60, 60);
            }

            string dateTimeString = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            XSize dateTimeSize = gfx.MeasureString(dateTimeString, fontNormal);
            gfx.DrawString(dateTimeString, fontNormal, XBrushes.Black, page.Width - x - dateTimeSize.Width, y + 70);

            gfx.DrawString("PODER JUDICIAL DEL ESTADO DE OAXACA", fontTitle, XBrushes.Black, new XRect(0, y, page.Width, fontTitle.Height), XStringFormats.TopCenter);
            gfx.DrawString("CONSEJO DE LA JUDICATURA", fontTitle, XBrushes.Black, new XRect(0, y + 15, page.Width, fontTitle.Height), XStringFormats.TopCenter);
            gfx.DrawString("DIRECCIÓN DE ADMINISTRACIÓN", fontTitle, XBrushes.Black, new XRect(0, y + 30, page.Width, fontTitle.Height), XStringFormats.TopCenter);
            gfx.DrawString("CONTROL PATRIMONIAL", fontTitle, XBrushes.Black, new XRect(0, y + 45, page.Width, fontTitle.Height), XStringFormats.TopCenter);

            string mesNombre = mes == 0 ? "0" : CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mes);
            string fechaReporte = mes == 0 ? $"{anio}" : $"{mesNombre.ToUpper()} DE {anio}"; // Mes 0 para anual, Mes X para mensual

            if (tipoReporte == "BIENES SOBRANTES")
            {
                gfx.DrawString($"REPORTE DE BIENES SOBRANTES", fontBold, XBrushes.Black, new XRect(0, y + 60, page.Width, fontSubTitle.Height), XStringFormats.TopCenter);
            }
            else if (tipoReporte == "BIENES FALTANTES")
            {
                gfx.DrawString($"REPORETDE DE BIENES FALTANTES", fontBold, XBrushes.Black, new XRect(0, y + 60, page.Width, fontSubTitle.Height), XStringFormats.TopCenter);
            }
            else
            {
                gfx.DrawString($"RESUMEN DE LOS BIENES VERIFICADOS", fontBold, XBrushes.Black, new XRect(0, y + 60, page.Width, fontSubTitle.Height), XStringFormats.TopCenter);
            }
            gfx.DrawString($"ADSCRIPCION: {area.ToUpper()}", fontBold, XBrushes.Black, new XRect(0, y + 75, page.Width, fontSubTitle.Height), XStringFormats.TopCenter);

            gfx.DrawString($"Pág. {currentPage} de {totalPages}", fontBold, XBrushes.Black, new XPoint(page.Width - x - 70, y + 83)); // Paginación (esquina superior derecha)

            gfx.DrawLine(XPens.Black, x, y + 100, page.Width - x, y + 100); // Dibuja una línea después del encabezado
        }

        private void DrawFooter(XGraphics gfx, PdfPage page)
        {
            XFont fontSmall = new XFont("Arial", 7, XFontStyle.Regular);

            double x = 40; // Margen izquierdo
            double y = page.Height - 60; // Posición desde abajo

            string footerText = "Centro Administrativo del Poder Ejecutivo y Judicial \"General Porfirio Díaz, Soldado de la Patria\" Edificios J1 y J2AV. Gerardo Pardal Graf";
            string footerText2 = "NO.1 Agencia de Policía Reyes Mantecón, San Bartolo Coyotepec, Oaxaca C.P. 71257, Conmutador (01 951) 5016680";
            gfx.DrawString(footerText, fontSmall, XBrushes.Black, new XRect(0, y, page.Width, fontSmall.Height), XStringFormats.TopCenter);
            gfx.DrawString(footerText2, fontSmall, XBrushes.Black, new XRect(0, y + 10, page.Width, fontSmall.Height), XStringFormats.TopCenter);
        }
    }
}
