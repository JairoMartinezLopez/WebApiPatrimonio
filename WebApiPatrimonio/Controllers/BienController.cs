using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using WebApiPatrimonio.Context;
using WebApiPatrimonio.Models;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using System.IO;
using PdfSharpCore.Fonts;

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

        [HttpGet("datosPorNoInventario")] // New route for this specific search
        public async Task<ActionResult<dynamic>> ObtenerBienPorNoInventario(string noInventario)
        {
            // Basic validation for the input
            if (string.IsNullOrWhiteSpace(noInventario))
            {
                return BadRequest(new { error = "El número de inventario no puede estar vacío." });
            }

            await using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_SEL_BIENES_BYNOINVENTARIO"; // Your stored procedure name

            // Add the parameter for the stored procedure
            command.Parameters.Add(new SqlParameter("@NoInventario", noInventario));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    // Return a single JSON object with the fetched data
                    return Ok(new
                    {
                        idBien = reader["idBien"],
                        NombreColor = reader["NombreColor"],
                        FechaRegistro = reader["FechaRegistro"],
                        FechaAlta = reader["FechaAlta"],
                        Aviso = reader["Aviso"],
                        Serie = reader["Serie"],
                        Modelo = reader["Modelo"],
                        NombreEstadoFisico = reader["NombreEstadoFisico"],
                        NombreMarca = reader["NombreMarca"],
                        Costo = reader["Costo"],
                        Etiquetado = reader["Etiquetado"],
                        FechaEtiquetado = reader["FechaEtiquetado"],
                        Disponibilidad = reader["Disponibilidad"],
                        FechaBaja = reader["FechaBaja"],
                        NombreCausalBaja = reader["NombreCausalBaja"],
                        NombreDisposicionFinal = reader["NombreDisposicionFinal"],
                        NumeroFactura = reader["NumeroFactura"],
                        NoInventario = reader["NoInventario"],
                        NombreCatalogoBien = reader["NombreCatalogoBien"],
                        Observaciones = reader["Observaciones"],
                        AplicaUMAS = reader["AplicaUMAS"],
                        Salida = reader["Salida"],
                        Activo = reader["Activo"]
                    });
                }
                else
                {
                    // Return 404 Not Found if no bien matches the inventory number
                    return NotFound(new { error = $"No se encontró ningún bien con el número de inventario: {noInventario}." });
                }
            }
            catch (SqlException ex)
            {
                // Handle SQL-specific errors
                return StatusCode(500, new { error = "Ocurrió un error en la base de datos.", details = ex.Message });
            }
            catch (Exception ex)
            {
                // Handle any other unexpected errors
                return StatusCode(500, new { error = "Ocurrió un error inesperado en el servidor.", details = ex.Message });
            }
            finally
            {
                // Ensure the database connection is closed
                if (_context.Database.GetDbConnection().State == ConnectionState.Open)
                {
                    await _context.Database.CloseConnectionAsync();
                }
            }
        }

        [HttpGet("datosPorId/{idBien}")]
        public async Task<ActionResult<dynamic>> ObtenerDatosBienPorId(long idBien)
        {
            if (idBien <= 0)
            {
                return BadRequest(new { error = "El ID de Bien proporcionado no es válido." });
            }

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_OBTENER_DATOS_BIENES_PARA_QR"; 
            command.Parameters.Add(new SqlParameter("@IdsBienes", idBien.ToString())); 

            try
            {
                await _context.Database.OpenConnectionAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    // Devuelve un solo objeto JSON con los datos del bien
                    return Ok(new
                    {
                        idBien = reader["idBien"],
                        NoInventario = reader["NoInventario"],
                        Serie = reader["Serie"],
                        Modelo = reader["Modelo"],
                        Marca = reader["Marca"],
                        Color = reader["Color"],
                        NoFactura = reader["NumeroFactura"]
                    });
                }
                else
                {
                    return NotFound(new { error = $"No se encontró ningún bien con el ID: {idBien}." });
                }
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

        [HttpGet("filtrar")]
        public async Task<ActionResult<IEnumerable<Bien>>> FiltrarBienes(
            [FromQuery] int? idColor,
            [FromQuery] DateTime? fechaRegistro,
            [FromQuery] DateTime? fechaAlta,
            [FromQuery] string? serie,
            [FromQuery] string? modelo,
            [FromQuery] int? idEstadoFisico,
            [FromQuery] int? idMarca,
            [FromQuery] double? costo,
            [FromQuery] bool? etiquetado,
            [FromQuery] DateTime? fechaEtiquetado,
            [FromQuery] bool? activo,
            [FromQuery] bool? disponibilidad,
            [FromQuery] DateTime? fechaBaja,
            [FromQuery] int? idCausalBaja,
            [FromQuery] int? idDisposicionFinal,
            [FromQuery] long? idFactura,
            [FromQuery] string? noInventario,
            [FromQuery] int? idCatalogoBien,
            [FromQuery] bool? aplicaUMAS,
            [FromQuery] string? salida
        )
        {
            var query = _context.BIENES.AsQueryable();

            if (idColor.HasValue)
                query = query.Where(b => b.IdColor == idColor);

            if (fechaRegistro.HasValue)
                query = query.Where(b => b.FechaRegistro.Value.Date == fechaRegistro.Value.Date);

            if (fechaAlta.HasValue)
                query = query.Where(b => b.FechaAlta == fechaAlta);

            if (!string.IsNullOrEmpty(serie))
                query = query.Where(b => b.Serie.Contains(serie));

            if (!string.IsNullOrEmpty(modelo))
                query = query.Where(b => b.Modelo.Contains(modelo));

            if (idEstadoFisico.HasValue)
                query = query.Where(b => b.IdEstadoFisico == idEstadoFisico);

            if (idMarca.HasValue)
                query = query.Where(b => b.IdMarca == idMarca);

            if (costo.HasValue)
                query = query.Where(b => b.Costo == costo);

            if (etiquetado.HasValue)
                query = query.Where(b => b.Etiquetado == etiquetado);

            if (fechaEtiquetado.HasValue)
                query = query.Where(b => b.FechaEtiquetado.Value.Date == fechaEtiquetado.Value.Date);

            if (activo.HasValue)
                query = query.Where(b => b.Activo == activo);

            if (disponibilidad.HasValue)
                query = query.Where(b => b.Disponibilidad == disponibilidad);

            if (fechaBaja.HasValue)
                query = query.Where(b => b.FechaBaja == fechaBaja);

            if (idCausalBaja.HasValue)
                query = query.Where(b => b.IdCausalBaja == idCausalBaja);

            if (idDisposicionFinal.HasValue)
                query = query.Where(b => b.IdDisposicionFinal == idDisposicionFinal);

            if (idFactura.HasValue)
                query = query.Where(b => b.IdFactura == idFactura);

            if (!string.IsNullOrEmpty(noInventario))
                query = query.Where(b => b.NoInventario.Contains(noInventario));

            if (idCatalogoBien.HasValue)
                query = query.Where(b => b.IdCatalogoBien == idCatalogoBien);

            if (aplicaUMAS.HasValue)
                query = query.Where(b => b.AplicaUMAS == aplicaUMAS);

            if (!string.IsNullOrEmpty(salida))
                query = query.Where(b => b.Salida.Contains(salida));

            var bienes = await query.ToListAsync();

            return Ok(bienes);
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
                    "@FechaBaja, @idCausalBaja, @idDisposicionFinal, @idFactura, @PartidaContable, @idCatalogoBien, " +
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
                    new SqlParameter("@PartidaContable", bien.NoInventario ?? (object)DBNull.Value),
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

        [HttpDelete("baja-bienes")]
        public async Task<ActionResult> DarBajaMasivaBienes([FromBody] BienesBaja request)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_BAJA_BIENES_MASIVA";

            // Parámetros de entrada del procedimiento almacenado
            command.Parameters.Add(new SqlParameter("@IdPantalla", 1));
            command.Parameters.Add(new SqlParameter("@IdGeneral", 1115));
            command.Parameters.Add(new SqlParameter("@idCausalBaja", request.IdCausalBaja));
            command.Parameters.Add(new SqlParameter("@FechaBaja", request.FechaBaja));
            command.Parameters.Add(new SqlParameter("@idDisposicionFinal", request.IdDisposicionFinal));

            // Crear el DataTable para el parámetro con valores de tabla
            DataTable dtNoInventarios = new DataTable();
            dtNoInventarios.Columns.Add("NoInventario", typeof(string));

            foreach (var bien in request.BienesABajar)
            {
                dtNoInventarios.Rows.Add(bien.NoInventario);
            }

            // Agregar el parámetro con valores de tabla
            SqlParameter tvpParam = new SqlParameter("@ListaNoInventario", dtNoInventarios);
            tvpParam.SqlDbType = SqlDbType.Structured; // Indicar que es un Table-Valued Parameter
            tvpParam.TypeName = "dbo.TipoListaNoInventario"; // El nombre del tipo de tabla que creaste en SQL Server
            command.Parameters.Add(tvpParam);

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Baja masiva de bienes realizada con éxito." });
            }
            catch (SqlException ex)
            {
                // Puedes parsear el mensaje de error para dar una respuesta más específica si es necesario
                // Por ejemplo, si el RAISERROR del SP devuelve un mensaje específico
                return BadRequest(new { error = ex.Message });
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
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

        [HttpPut("Etiquetado")]
        public async Task<IActionResult> Etiquetado(int idBien)
        {
            var nuevoEstadoParam = new SqlParameter("@NuevoEstadoEtiquetado", System.Data.SqlDbType.Bit)
            {
                Direction = System.Data.ParameterDirection.Output
            };

            var sql = "EXEC PA_ETIQUETADO_BIENES @idBien, @IdPantalla, @IdGeneral, @NuevoEstadoEtiquetado OUTPUT";

            try
            {
                await _context.Database.ExecuteSqlRawAsync(sql,
                    new SqlParameter("@idBien", idBien),
                    new SqlParameter("@IdPantalla", 1),
                    new SqlParameter("@IdGeneral", 1),
                    nuevoEstadoParam); 

                bool nuevoEstado = nuevoEstadoParam.Value != DBNull.Value ? (bool)nuevoEstadoParam.Value : false;

                string mensaje = nuevoEstado ? "BIEN Etiquetado correctamente." : "BIEN sin etiquetado correctamente.";

                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al actualizar el estado de etiquetado: {ex.Message}");
            }
        }

        // GET: api/Bien/GenerarQRBienesPDF
        [HttpGet("GenerarQRBienesPDF")]
        public async Task<IActionResult> GenerarQRBienesPDF([FromQuery(Name = "idBienes")] string idBienes)
        {
            if (string.IsNullOrEmpty(idBienes))
                return BadRequest("Se debe proporcionar al menos un ID de Bien.");

            var listaBienes = new List<(string idBien, string NumeroInventario, string Marca, string Modelo, string Serie, string Color, string NoFactura)>();

            try
            {
                using var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = "PA_OBTENER_DATOS_BIENES_PARA_QR";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@IdsBienes", idBienes));

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    listaBienes.Add((
                        reader["idBien"]?.ToString() ?? "N/A",
                        reader["NoInventario"]?.ToString() ?? "N/A",
                        reader["Marca"]?.ToString() ?? "N/A",
                        reader["Modelo"]?.ToString() ?? "N/A",
                        reader["Serie"]?.ToString() ?? "N/A",
                        reader["Color"]?.ToString() ?? "N/A",
                        reader["NumeroFactura"]?.ToString() ?? "N/A"
                    ));
                }

                using var ms = new MemoryStream();
                using var document = new PdfDocument();

                var font = new XFont("Arial", 8, XFontStyle.Regular);
                var bold = new XFont("Arial", 8, XFontStyle.Bold);
                var titleFont = new XFont("Arial", 10, XFontStyle.Bold);

                double anchoPaginaA4 = PdfSharpCore.PageSizeConverter.ToSize(PdfSharpCore.PageSize.A4).Width;

                int columnas = 2;
                int filasPorColumna = 4;
                int etiquetasPorPagina = columnas * filasPorColumna;

                double etiquetaAncho = 260;
                double etiquetaAlto = 180;

                double margenX = 30;
                double margenY = 30;

                double espaciadoHorizontal = (anchoPaginaA4 - (2 * margenX) - (columnas * etiquetaAncho)) / (columnas - 1);
                double espaciadoVertical = 10;

                int index = 0;

                while (index < listaBienes.Count)
                {
                    var page = document.AddPage();
                    page.Size = PdfSharpCore.PageSize.A4;
                    var gfx = XGraphics.FromPdfPage(page);

                    for (int i = 0; i < etiquetasPorPagina && index < listaBienes.Count; i++)
                    {
                        int columnaActual = i % columnas;
                        int filaActual = i / columnas;

                        double x = margenX + (columnaActual * (etiquetaAncho + espaciadoHorizontal));
                        double y = margenY + (filaActual * (etiquetaAlto + espaciadoVertical));

                        // Fondo gris MUY claro
                        var fondoClaro = new XSolidBrush(XColor.FromArgb(240, 240, 240));
                        gfx.DrawRectangle(fondoClaro, x, y, etiquetaAncho, etiquetaAlto);

                        // Borde
                        gfx.DrawRectangle(XPens.Black, x, y, etiquetaAncho, etiquetaAlto);

                        // Encabezado
                        gfx.DrawString("PODER JUDICIAL DEL ESTADO", titleFont, XBrushes.Black, new XRect(x + 10, y + 10, etiquetaAncho - 20, 10), XStringFormats.TopLeft);

                        var bien = listaBienes[index];
                        string contenidoQR = $"idBien: {bien.idBien}\nInventario: {bien.NumeroInventario}\nMarca: {bien.Marca}\nModelo: {bien.Modelo}\nSerie: {bien.Serie}\nColor: {bien.Color}\nNumeroFactura: {bien.NoFactura}";

                        var qrGen = new QRCodeGenerator();
                        var qrData = qrGen.CreateQrCode(contenidoQR, QRCodeGenerator.ECCLevel.Q);
                        var qrCode = new BitmapByteQRCode(qrData);
                        byte[] qrImage = qrCode.GetGraphic(20);
                        using var qrStream = new MemoryStream(qrImage);
                        var image = XImage.FromStream(() => new MemoryStream(qrStream.ToArray()));
                        gfx.DrawImage(image, x + 10, y + 45, 100, 100);

                        // Texto con ajuste si el No. Inventario es muy largo
                        double textoX = x + 120;
                        double textoY = y + 40;

                        void DrawText(string label, string value)
                        {
                            gfx.DrawString($"{label}", bold, XBrushes.Black, new XPoint(textoX, textoY));
                            textoY += 10;

                            const int maxLineLength = 30;
                            while (value.Length > maxLineLength)
                            {
                                gfx.DrawString(value.Substring(0, maxLineLength), font, XBrushes.Black, new XPoint(textoX + 10, textoY));
                                value = value.Substring(maxLineLength);
                                textoY += 10;
                            }
                            gfx.DrawString(value, font, XBrushes.Black, new XPoint(textoX + 10, textoY));
                            textoY += 12;
                        }

                        DrawText("Inventario:", bien.NumeroInventario);
                        DrawText("Marca:", bien.Marca);
                        DrawText("Modelo:", bien.Modelo);
                        DrawText("Serie:", bien.Serie);
                        DrawText("Color:", bien.Color);
                        DrawText("No. Factura:", bien.NoFactura);

                        index++;
                    }
                }

                document.Save(ms);
                ms.Position = 0;
                return File(ms.ToArray(), "application/pdf", $"Etiquetas_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar etiquetas: {ex.Message}");
            }
        }

        private bool BienExists(long id)
        {
            return _context.BIENES.Any(e => e.IdBien == id);
        }
    }
}
