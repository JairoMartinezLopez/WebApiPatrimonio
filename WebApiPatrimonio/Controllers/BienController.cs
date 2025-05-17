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
            var nuevoEstadoParam = new SqlParameter("@NuevoEstadoPublicar", System.Data.SqlDbType.Bit)
            {
                Direction = System.Data.ParameterDirection.Output
            };

            var sql = "EXEC PA_ETIQUETADO_BIENES @idBien, @IdPantalla, @IdGeneral, @NuevoEstadoEtiquetado OUTPUT";

            await _context.Database.ExecuteSqlRawAsync(sql,
                new SqlParameter("@idBien", idBien),
                new SqlParameter("@IdPantalla", 1),
                new SqlParameter("@IdGeneral", 1),
                nuevoEstadoParam);

            // Obtener el valor del parámetro de salida
            bool nuevoEstado = (bool)nuevoEstadoParam.Value;

            string mensaje = nuevoEstado ? "BIEN Etiquetado correctamente." : "BIEN sin etiquetado correctamente.";
            
            return Ok(new { mensaje });
        }

        [HttpGet("GenerarQRBienesPDF")]
        public async Task<IActionResult> GenerarQRBienesPDF([FromQuery(Name = "idBienes")] string idBienes)
        {
            if (string.IsNullOrEmpty(idBienes))
            {
                return BadRequest("Se debe proporcionar al menos un ID de Bien.");
            }

            var listaBienes = new List<(string NumeroInventario, string Marca, string Modelo, string Serie, string Color)>();

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
                    var numeroInventario = reader["NoInventario"].ToString();
                    var marca = reader["Marca"].ToString();
                    var modelo = reader["Modelo"].ToString();
                    var serie = reader["Serie"].ToString();
                    var color = reader["Color"].ToString();

                    listaBienes.Add((numeroInventario, marca, modelo, serie, color));
                }

                using var ms = new MemoryStream();
                using var document = new PdfSharpCore.Pdf.PdfDocument();
                var font = new XFont("Arial", 12);
                var lineSpace = 15; // Espacio entre líneas de texto

                // Configuración para 6 QR por página (3 filas x 2 columnas)
                int qrPorPagina = 6;
                int filas = 3;
                int columnas = 2;
                double qrAncho = 150;
                double qrAlto = 150;
                double margenX = 20;
                double margenY = 20;

                int bienIndex = 0;
                while (bienIndex < listaBienes.Count)
                {
                    var page = document.AddPage();
                    page.Size = PdfSharpCore.PageSize.A4;
                    var gfx = XGraphics.FromPdfPage(page);
                    double paginaAncho = gfx.PageSize.Width;
                    double paginaAlto = gfx.PageSize.Height;

                    // Calcular el espacio disponible para el QR y el texto
                    double espacioQRTextoY = qrAlto + 5 * lineSpace + 10; // Altura del QR + altura del texto + espacio
                    double espacioX = (paginaAncho - 2 * margenX - columnas * qrAncho) / (columnas - 1);
                    double espacioY = (paginaAlto - 2 * margenY - filas * espacioQRTextoY) / (filas - 1);

                    for (int i = 0; i < filas; i++)
                    {
                        for (int j = 0; j < columnas; j++)
                        {
                            if (bienIndex < listaBienes.Count)
                            {
                                var bien = listaBienes[bienIndex];
                                string contenidoQR = $"Inventario: {bien.NumeroInventario}\nMarca: {bien.Marca}\nModelo: {bien.Modelo}\nSerie: {bien.Serie}\nColor: {bien.Color}";

                                var qrGenerator = new QRCodeGenerator();
                                var qrData = qrGenerator.CreateQrCode(contenidoQR, QRCodeGenerator.ECCLevel.Q);
                                var qrCode = new BitmapByteQRCode(qrData);
                                byte[] qrBytes = qrCode.GetGraphic(20);
                                using var imageStream = new MemoryStream(qrBytes);
                                var image = XImage.FromStream(() => new MemoryStream(imageStream.ToArray()));

                                // Calcula la posición del QR
                                double x = margenX + j * (qrAncho + espacioX);
                                double y = margenY + i * (espacioQRTextoY + espacioY); // Usa espacioQRTextoY

                                gfx.DrawImage(image, x, y, qrAncho, qrAlto);

                                // Dibuja el texto debajo del QR como una lista
                                double textoX = x;
                                double textoY = y + qrAlto + 5;
                                gfx.DrawString($"Inventario: {bien.NumeroInventario}", font, XBrushes.Black, new XRect(textoX, textoY, paginaAncho - x, 0));
                                textoY += lineSpace;
                                gfx.DrawString($"Marca: {bien.Marca}", font, XBrushes.Black, new XRect(textoX, textoY, paginaAncho - x, 0));
                                textoY += lineSpace;
                                gfx.DrawString($"Modelo: {bien.Modelo}", font, XBrushes.Black, new XRect(textoX, textoY, paginaAncho - x, 0));
                                textoY += lineSpace;
                                gfx.DrawString($"Serie: {bien.Serie}", font, XBrushes.Black, new XRect(textoX, textoY, paginaAncho - x, 0));
                                textoY += lineSpace;
                                gfx.DrawString($"Color: {bien.Color}", font, XBrushes.Black, new XRect(textoX, textoY, paginaAncho - x, 0));

                                bienIndex++;
                            }
                            else
                            {
                                break; // No hay más bienes para esta página
                            }
                        }
                        if (bienIndex >= listaBienes.Count)
                        {
                            break; // No hay más bienes
                        }
                    }
                }

                document.Save(ms);
                ms.Position = 0;

                // Fuerza la recolección de basura
                GC.Collect();
                GC.WaitForPendingFinalizers();

                return File(ms.ToArray(), "application/pdf", $"QR_{idBienes.Replace(",", "_")}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, $"Error al generar el PDF: {ex.Message}");
            }
        }

        private bool BienExists(long id)
        {
            return _context.BIENES.Any(e => e.IdBien == id);
        }
    }
}
