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
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApiPatrimonio.Context;

namespace WebApiPatrimonio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReportesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: api/Reportes/ReporteAltasConcentradoPDF
        [HttpGet("ReporteAltasConcentradoPDF")]
        public async Task<IActionResult> GenerarReporteAltasConcentradoPDF(
            [FromQuery] int idGeneral,
            [FromQuery] int idPantalla,
            [FromQuery] int idFinanciamiento,
            [FromQuery] int anio,
            [FromQuery] int mes,
            [FromQuery] int unidadResponsable,
            [FromQuery] bool umas)
        {
            try
            {
                var concentradoData = new List<(string TipoBienClave, string Nombre, double Importe)>();

                using (var connection = _context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "PA_SEL_ReporteAltasConcentrado";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@IdGeneral", idGeneral));
                        command.Parameters.Add(new SqlParameter("@IdPantalla", idPantalla));
                        command.Parameters.Add(new SqlParameter("@idFinanciamiento", idFinanciamiento));
                        command.Parameters.Add(new SqlParameter("@Anio", anio));
                        command.Parameters.Add(new SqlParameter("@Mes", mes));
                        command.Parameters.Add(new SqlParameter("@UnidadResponsable", unidadResponsable));
                        command.Parameters.Add(new SqlParameter("@Umas", umas));

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                concentradoData.Add((
                                    reader["TipoBien"]?.ToString(),
                                    reader["Nombre"]?.ToString(),
                                    Convert.ToDouble(reader["Importe"])
                                ));
                            }
                        }
                    }
                }

                using var ms = new MemoryStream();
                using var document = new PdfDocument();
                document.Info.Title = "Reporte de Altas Concentrado";

                double contentMarginTop = 150;
                double contentMarginBottom = 70;
                double contentMarginLeft = 40;
                double contentMarginRight = 40;

                XFont tableHeaderFont = new XFont("Arial", 9, XFontStyle.Bold);
                XFont tableContentFont = new XFont("Arial", 8, XFontStyle.Regular);

                double col1Width = 70;
                double col2Width = 350;
                double col3Width = 95;
                double cellPadding = 5;
                double rowHeight = 20;

                double currentY = contentMarginTop;
                int pageNumber = 1;

                PdfPage page = document.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);

                DrawHeader(gfx, page, "REPORTE DE ALTAS CONCENTRADO", "ALTAS", anio, mes, idFinanciamiento, pageNumber, 1);
                DrawFooter(gfx, page);

                gfx.DrawRectangle(XPens.Black, contentMarginLeft, currentY, col1Width + col2Width + col3Width, rowHeight);
                gfx.DrawString("TIPO BIEN", tableHeaderFont, XBrushes.Black, new XRect(contentMarginLeft + cellPadding, currentY + cellPadding, col1Width, rowHeight), XStringFormats.TopCenter);
                gfx.DrawString("DESCRIPCIÓN", tableHeaderFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + cellPadding, currentY + cellPadding, col2Width, rowHeight), XStringFormats.TopLeft);
                gfx.DrawString("IMPORTE", tableHeaderFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + col2Width + cellPadding, currentY + cellPadding, col3Width, rowHeight), XStringFormats.TopCenter);

                gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width, currentY, contentMarginLeft + col1Width, currentY + rowHeight);
                gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width + col2Width, currentY, contentMarginLeft + col1Width + col2Width, currentY + rowHeight);

                currentY += rowHeight;
                double totalImporteGeneral = 0;

                foreach (var item in concentradoData)
                {
                    if (currentY + rowHeight > page.Height - contentMarginBottom)
                    {
                        pageNumber++;
                        page = document.AddPage();
                        gfx = XGraphics.FromPdfPage(page);

                        DrawHeader(gfx, page, "REPORTE DE ALTAS CONCENTRADO", "ALTAS", anio, mes, idFinanciamiento, pageNumber, 1);
                        DrawFooter(gfx, page);

                        currentY = contentMarginTop;
                        gfx.DrawRectangle(XPens.Black, contentMarginLeft, currentY, col1Width + col2Width + col3Width, rowHeight);
                        gfx.DrawString("TIPO BIEN", tableHeaderFont, XBrushes.Black, new XRect(contentMarginLeft + cellPadding, currentY + cellPadding, col1Width, rowHeight), XStringFormats.TopCenter);
                        gfx.DrawString("DESCRIPCIÓN", tableHeaderFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + cellPadding, currentY + cellPadding, col2Width, rowHeight), XStringFormats.TopLeft);
                        gfx.DrawString("IMPORTE", tableHeaderFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + col2Width + cellPadding, currentY + cellPadding, col3Width, rowHeight), XStringFormats.TopCenter);
                        gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width, currentY, contentMarginLeft + col1Width, currentY + rowHeight);
                        gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width + col2Width, currentY, contentMarginLeft + col1Width + col2Width, currentY + rowHeight);
                        currentY += rowHeight;
                    }

                    gfx.DrawRectangle(XPens.Black, contentMarginLeft, currentY, col1Width + col2Width + col3Width, rowHeight);
                    gfx.DrawString(item.TipoBienClave, tableContentFont, XBrushes.Black, new XRect(contentMarginLeft + cellPadding, currentY + cellPadding, col1Width, rowHeight), XStringFormats.TopCenter);
                    gfx.DrawString(item.Nombre, tableContentFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + cellPadding, currentY + cellPadding, col2Width, rowHeight), XStringFormats.TopLeft);
                    gfx.DrawString($"{item.Importe:C2}", tableContentFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + col2Width + cellPadding, currentY + cellPadding, col3Width, rowHeight), XStringFormats.TopCenter);

                    gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width, currentY, contentMarginLeft + col1Width, currentY + rowHeight);
                    gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width + col2Width, currentY, contentMarginLeft + col1Width + col2Width, currentY + rowHeight);

                    totalImporteGeneral += item.Importe;
                    currentY += rowHeight;
                }

                if (concentradoData.Any())
                {
                    if (currentY + rowHeight > page.Height - contentMarginBottom)
                    {
                        pageNumber++;
                        page = document.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        DrawHeader(gfx, page, "REPORTE DE ALTAS CONCENTRADO", "ALTAS", anio, mes, idFinanciamiento, pageNumber, 1);
                        DrawFooter(gfx, page);
                        currentY = contentMarginTop;
                    }

                    gfx.DrawRectangle(XPens.Black, contentMarginLeft, currentY, col1Width + col2Width + col3Width, rowHeight);
                    gfx.DrawString("TOTAL GENERAL", tableHeaderFont, XBrushes.Black, new XRect(contentMarginLeft - 5, currentY + cellPadding, col1Width + col2Width, rowHeight), XStringFormats.TopRight);
                    gfx.DrawString($"{totalImporteGeneral:C2}", tableHeaderFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + col2Width + cellPadding, currentY + cellPadding, col3Width, rowHeight), XStringFormats.TopCenter);
                    gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width + col2Width, currentY, contentMarginLeft + col1Width + col2Width, currentY + rowHeight);
                }

                document.Save(ms);
                ms.Position = 0;
                return File(ms.ToArray(), "application/pdf", $"ReporteAltasConcentrado_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar reporte de altas concentrado: {ex.Message}");
            }
        }

        // GET: api/Reportes/ReporteAltasDesglozadoPDF
        [HttpGet("ReporteAltasDesglozadoPDF")]
        public async Task<IActionResult> GenerarReporteAltasDesglozadoPDF(
            [FromQuery] int idGeneral,
            [FromQuery] int idPantalla,
            [FromQuery] int idFinanciamiento,
            [FromQuery] int anio,
            [FromQuery] int mes,
            [FromQuery] int unidadResponsable,
            [FromQuery] bool umas)
        {
            try
            {
                var desglosadoData = new List<(string ClaveTipoBien, string TipoBienNombre, string ClaveBien, string BienDesc, string Unidad, string NoInventario, string Aviso, string Marca, string Serie, string Modelo, double Costo, DateTime FechaAlta, string Observaciones, string FolioFiscal, string NumeroFactura)>();

                using (var connection = _context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "PA_SEL_ReporteAltasDesglozado";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@IdGeneral", idGeneral));
                        command.Parameters.Add(new SqlParameter("@IdPantalla", idPantalla));
                        command.Parameters.Add(new SqlParameter("@idFinanciamiento", idFinanciamiento));
                        command.Parameters.Add(new SqlParameter("@Anio", anio));
                        command.Parameters.Add(new SqlParameter("@Mes", mes));
                        command.Parameters.Add(new SqlParameter("@UnidadResponsable", unidadResponsable));
                        command.Parameters.Add(new SqlParameter("@Umas", umas));

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                desglosadoData.Add((
                                    reader["ClaveTipoBien"]?.ToString(),
                                    reader["TipoBienNombre"]?.ToString(),
                                    reader["ClaveBien"]?.ToString(),
                                    reader["BienDesc"]?.ToString(),
                                    reader["Unidad"]?.ToString(),
                                    reader["NoInventario"]?.ToString(),
                                    reader["Aviso"]?.ToString(),
                                    reader["Marca"]?.ToString(),
                                    reader["Serie"]?.ToString(),
                                    reader["Modelo"]?.ToString(),
                                    Convert.ToDouble(reader["Costo"]),
                                    Convert.ToDateTime(reader["FechaAlta"]),
                                    reader["Observaciones"]?.ToString(),
                                    reader["FolioFiscal"]?.ToString(),
                                    reader["NumeroFactura"]?.ToString()
                                ));
                            }
                        }
                    }
                }

                // Agrupar los datos para la clasificación
                var groupedData = desglosadoData
                    .GroupBy(item => new { item.ClaveTipoBien, item.TipoBienNombre })
                    .Select(g1 => new
                    {
                        TipoBienKey = g1.Key,
                        TipoBienItems = g1.GroupBy(item => new { item.ClaveBien, item.BienDesc })
                                          .Select(g2 => new
                                          {
                                              BienKey = g2.Key,
                                              BienItems = g2.OrderBy(item => item.NoInventario).ToList(),
                                              TotalArticulosBien = g2.Count(),
                                              CostoTotalBien = g2.Sum(item => item.Costo)
                                          }).ToList(),
                        SubtotalTipoBien = g1.Sum(item => item.Costo)
                    }).ToList();

                using var ms = new MemoryStream();
                using var document = new PdfDocument();
                document.Info.Title = "Reporte de Altas Detallado";

                double contentMarginTop = 150;
                double contentMarginBottom = 70;
                double contentMarginLeft = 40;
                double contentMarginRight = 40;

                PdfPage page = document.AddPage();
                page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                page.Size = PdfSharpCore.PageSize.Legal;
                XGraphics gfx = XGraphics.FromPdfPage(page); // Create XGraphics object once per page
                double contentWidth = page.Width - contentMarginLeft - contentMarginRight;

                XFont tableHeaderFont = new XFont("Arial", 8, XFontStyle.Bold);
                XFont tableContentFont = new XFont("Arial", 7, XFontStyle.Regular);
                XFont groupHeaderFont = new XFont("Arial", 9, XFontStyle.Bold);
                XFont summaryFont = new XFont("Arial", 8, XFontStyle.Regular);
                double rowHeight = 18;
                double cellPadding = 3;

                XPen dottedBottomBorderPen = new XPen(XColor.FromArgb(150, 0, 0, 0), 0.5); // Lighter black, thinner line
                dottedBottomBorderPen.DashStyle = XDashStyle.Dot;


                var columnHeaders = new (string Name, double WidthFactor)[]
                {
                    ("UNIDAD", 0.04),
                    ("NO. INVENT", 0.10),
                    ("AVISO", 0.14),
                    ("FACTURA", 0.05),
                    ("FOLIO FIS.", 0.10),
                    ("MARCA", 0.04),
                    ("SERIE", 0.09),
                    ("MODELO", 0.09),
                    ("COSTO", 0.04),
                    ("OBSERVACIONES", 0.31)
                };

                double[] colWidths = columnHeaders.Select(ch => ch.WidthFactor * contentWidth).ToArray();

                int pageNumber = 1;
                double currentY = contentMarginTop;
                double currentX;

                double totalGeneralPesos = 0;

                Action DrawTableHeaders = () =>
                {
                    currentX = contentMarginLeft;

                    // 1. Fila de encabezado agrupado: "DESCRIPCIÓN BIEN"
                    double descripcionBienWidth = colWidths[0] + colWidths[1]; // Agrupa NO. INVENT + COLOR
                    gfx.DrawRectangle(XPens.Black, currentX, currentY, descripcionBienWidth, rowHeight);
                    gfx.DrawString("DESCRIPCIÓN BIEN", tableHeaderFont, XBrushes.Black,
                        new XRect(currentX, currentY, descripcionBienWidth, rowHeight), XStringFormats.Center);

                    currentY += rowHeight;

                    // 2. Segunda fila: encabezados normales
                    currentX = contentMarginLeft;
                    for (int i = 0; i < columnHeaders.Length; i++)
                    {
                        gfx.DrawRectangle(XPens.Black, currentX, currentY, colWidths[i], rowHeight);
                        gfx.DrawString(columnHeaders[i].Name, tableHeaderFont, XBrushes.Black,
                            new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[i] - cellPadding * 2, rowHeight - cellPadding * 2), XStringFormats.Center);
                        currentX += colWidths[i];
                    }

                    currentY += rowHeight;
                };

                DrawHeader(gfx, page, "REPORTE DE ALTAS DETALLADO", "ALTAS", anio, mes, idFinanciamiento, pageNumber, 1);
                DrawFooter(gfx, page);
                DrawTableHeaders(); // Dibuja los encabezados de la tabla por primera vez

                foreach (var tipoBienGroup in groupedData)
                {
                    if (currentY + rowHeight * 2 > page.Height - contentMarginBottom)
                    {
                        pageNumber++;
                        page = document.AddPage();
                        page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                        page.Size = PdfSharpCore.PageSize.Legal;
                        gfx = XGraphics.FromPdfPage(page); // Re-create XGraphics for the new page
                        DrawHeader(gfx, page, "REPORTE DE ALTAS DETALLADO", "ALTAS", anio, mes, idFinanciamiento, pageNumber, 1);
                        DrawFooter(gfx, page);
                        currentY = contentMarginTop;
                        DrawTableHeaders();
                    }

                    // Dibuja el encabezado del tipo de bien (CAT_TIPOSBIENES)
                    string tipoBienHeader = $"{tipoBienGroup.TipoBienKey.ClaveTipoBien} {tipoBienGroup.TipoBienKey.TipoBienNombre}";
                    gfx.DrawString(tipoBienHeader, groupHeaderFont, XBrushes.Black,
                        new XRect(contentMarginLeft, currentY + cellPadding, contentWidth, rowHeight), XStringFormats.TopLeft);
                    currentY += rowHeight;

                    foreach (var bienGroup in tipoBienGroup.TipoBienItems)
                    {
                        if (currentY + rowHeight * 3 > page.Height - contentMarginBottom)
                        {
                            pageNumber++;
                            page = document.AddPage();
                            page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                            page.Size = PdfSharpCore.PageSize.Legal;
                            gfx = XGraphics.FromPdfPage(page); // Re-create XGraphics for the new page
                            DrawHeader(gfx, page, "REPORTE DE ALTAS DETALLADO", "ALTAS", anio, mes, idFinanciamiento, pageNumber, 1);
                            DrawFooter(gfx, page);
                            currentY = contentMarginTop;
                            DrawTableHeaders();
                            gfx.DrawString(tipoBienHeader, groupHeaderFont, XBrushes.Black,
                                new XRect(contentMarginLeft, currentY + cellPadding, contentWidth, rowHeight), XStringFormats.TopLeft);
                            currentY += rowHeight;
                        }

                        // Dibuja el encabezado del bien (CAT_BIENES)
                        string bienHeader = $"{bienGroup.BienKey.ClaveBien} {bienGroup.BienKey.BienDesc}";
                        gfx.DrawString(bienHeader, summaryFont, XBrushes.Black,
                            new XRect(contentMarginLeft + 10, currentY + cellPadding, contentWidth - 10, rowHeight), XStringFormats.TopLeft);
                        currentY += rowHeight;

                        foreach (var item in bienGroup.BienItems)
                        {
                            if (currentY + rowHeight > page.Height - contentMarginBottom)
                            {
                                pageNumber++;
                                page = document.AddPage();
                                page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                                page.Size = PdfSharpCore.PageSize.Legal;
                                gfx = XGraphics.FromPdfPage(page); // Re-create XGraphics for the new page
                                DrawHeader(gfx, page, "REPORTE DE ALTAS DETALLADO", "ALTAS", anio, mes, idFinanciamiento, pageNumber, 1);
                                DrawFooter(gfx, page);
                                currentY = contentMarginTop;
                                DrawTableHeaders();
                                gfx.DrawString(tipoBienHeader, groupHeaderFont, XBrushes.Black,
                                    new XRect(contentMarginLeft, currentY + cellPadding, contentWidth, rowHeight), XStringFormats.TopLeft);
                                currentY += rowHeight;
                                gfx.DrawString(bienHeader, groupHeaderFont, XBrushes.Black,
                                    new XRect(contentMarginLeft + 10, currentY + cellPadding, contentWidth - 10, rowHeight), XStringFormats.TopLeft);
                                currentY += rowHeight;
                            }

                            gfx.DrawLine(dottedBottomBorderPen, contentMarginLeft, currentY + rowHeight, contentMarginLeft + contentWidth, currentY + rowHeight);

                            currentX = contentMarginLeft; 

                            gfx.DrawString(item.Unidad, tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[0] - cellPadding * 2, rowHeight - cellPadding * 2), XStringFormats.TopLeft);
                            currentX += colWidths[0];

                            gfx.DrawString(item.NoInventario, tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[1] - cellPadding * 2, rowHeight - cellPadding * 2), XStringFormats.TopLeft);
                            currentX += colWidths[1];

                            gfx.DrawString(item.Aviso, tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[2] - cellPadding * 2, rowHeight - cellPadding * 2), XStringFormats.TopLeft);
                            currentX += colWidths[2];

                            gfx.DrawString(item.NumeroFactura, tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[3] - cellPadding * 2, rowHeight - cellPadding * 2), XStringFormats.TopCenter);
                            currentX += colWidths[3];

                            gfx.DrawString(item.FolioFiscal, tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[4] - cellPadding * 2, rowHeight - cellPadding * 2), XStringFormats.TopCenter);
                            currentX += colWidths[4];

                            gfx.DrawString(item.Marca, tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[5] - cellPadding * 2, rowHeight - cellPadding * 2), XStringFormats.TopLeft);
                            currentX += colWidths[5];

                            gfx.DrawString(item.Serie, tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[6] - cellPadding * 2, rowHeight - cellPadding * 2), XStringFormats.TopLeft);
                            currentX += colWidths[6];

                            gfx.DrawString(item.Modelo, tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[7] - cellPadding * 2, rowHeight - cellPadding * 2), XStringFormats.TopLeft);
                            currentX += colWidths[7];

                            gfx.DrawString($"{item.Costo:C2}", tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[8] - cellPadding * 2, rowHeight - cellPadding * 2), XStringFormats.TopRight); // Se mantiene a la derecha para números
                            currentX += colWidths[8];

                            gfx.DrawString(item.Observaciones, tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[9] - cellPadding * 2, rowHeight - cellPadding * 2), XStringFormats.TopLeft);
                            currentX += colWidths[9];

                            currentY += rowHeight;
                        }

                        // Dibuja el "Total de artículos del bien"
                        if (currentY + rowHeight > page.Height - contentMarginBottom)
                        {
                            pageNumber++;
                            page = document.AddPage();
                            page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                            page.Size = PdfSharpCore.PageSize.Legal;
                            gfx = XGraphics.FromPdfPage(page); // Re-create XGraphics for the new page
                            DrawHeader(gfx, page, "REPORTE DE ALTAS DETALLADO", "ALTAS", anio, mes, idFinanciamiento, pageNumber, 1);
                            DrawFooter(gfx, page);
                            currentY = contentMarginTop;
                            DrawTableHeaders();
                            gfx.DrawString(tipoBienHeader, groupHeaderFont, XBrushes.Black,
                                new XRect(contentMarginLeft, currentY + cellPadding, contentWidth, rowHeight), XStringFormats.TopLeft);
                            currentY += rowHeight;
                            gfx.DrawString(bienHeader, groupHeaderFont, XBrushes.Black,
                                new XRect(contentMarginLeft + 10, currentY + cellPadding, contentWidth - 10, rowHeight), XStringFormats.TopLeft);
                            currentY += rowHeight;
                        }

                        // Dibuja el total de artículos del bien y el costo total del bien
                        string totalArticulosBien = $"TOTAL DE ARTÍCULOS DEL BIEN: {bienGroup.TotalArticulosBien}";

                        gfx.DrawString(totalArticulosBien, summaryFont, XBrushes.Black,
                            new XRect(contentMarginLeft + 20, currentY + cellPadding, contentWidth - 20, rowHeight - cellPadding * 2), XStringFormats.TopLeft); // Alineado a la izquierda
                        currentY += rowHeight;

                        if (currentY + rowHeight > page.Height - contentMarginBottom)
                        {
                            pageNumber++;
                            page = document.AddPage();
                            page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                            page.Size = PdfSharpCore.PageSize.Legal;
                            gfx = XGraphics.FromPdfPage(page); // Re-create XGraphics for the new page
                            DrawHeader(gfx, page, "REPORTE DE ALTAS DETALLADO", "ALTAS", anio, mes, idFinanciamiento, pageNumber, 1);
                            DrawFooter(gfx, page);
                            currentY = contentMarginTop;
                            DrawTableHeaders();
                            gfx.DrawString(tipoBienHeader, groupHeaderFont, XBrushes.Black,
                                new XRect(contentMarginLeft, currentY + cellPadding, contentWidth, rowHeight), XStringFormats.TopLeft);
                            currentY += rowHeight;
                            gfx.DrawString(bienHeader, groupHeaderFont, XBrushes.Black,
                                new XRect(contentMarginLeft + 10, currentY + cellPadding, contentWidth - 10, rowHeight), XStringFormats.TopLeft);
                            currentY += rowHeight;
                        }
                        currentY += rowHeight + 5;
                    }

                    // Dibuja el "Subtotal por tipo de bien"
                    if (currentY + rowHeight > page.Height - contentMarginBottom)
                    {
                        pageNumber++;
                        page = document.AddPage();
                        page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                        page.Size = PdfSharpCore.PageSize.Legal;
                        gfx = XGraphics.FromPdfPage(page); // Re-create XGraphics for the new page
                        DrawHeader(gfx, page, "REPORTE DE ALTAS DETALLADO", "ALTAS", anio, mes, idFinanciamiento, pageNumber, 1);
                        DrawFooter(gfx, page);
                        currentY = contentMarginTop;
                        DrawTableHeaders();
                    }

                    string subtotalTipoBienText = $"SUBTOTAL POR TIPO DE BIEN: {tipoBienGroup.SubtotalTipoBien:C2}";
                    gfx.DrawString(subtotalTipoBienText, summaryFont, XBrushes.Black,
                        new XRect(contentMarginLeft, currentY + cellPadding, contentWidth, rowHeight - cellPadding * 2), XStringFormats.TopLeft); // Alineado a la izquierda
                    currentY += rowHeight + 10;

                    totalGeneralPesos += tipoBienGroup.SubtotalTipoBien;
                }

                // Dibuja el "Total general en pesos"
                if (currentY + rowHeight > page.Height - contentMarginBottom)
                {
                    pageNumber++;
                    page = document.AddPage();
                    page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                    page.Size = PdfSharpCore.PageSize.Legal;
                    gfx = XGraphics.FromPdfPage(page); // Re-create XGraphics for the new page
                    DrawHeader(gfx, page, "REPORTE DE ALTAS DETALLADO", "ALTAS", anio, mes, idFinanciamiento, pageNumber, 1);
                    DrawFooter(gfx, page);
                    currentY = contentMarginTop;
                    DrawTableHeaders();
                }
                string totalGeneralText = $"TOTAL GENERAL EN PESOS: {totalGeneralPesos:C2}";
                gfx.DrawString(totalGeneralText, groupHeaderFont, XBrushes.Black,
                    new XRect(contentMarginLeft, currentY + cellPadding, contentWidth, rowHeight - cellPadding * 2), XStringFormats.TopLeft); // Alineado a la izquierda

                document.Save(ms);
                ms.Position = 0;
                return File(ms.ToArray(), "application/pdf", $"ReporteAltasDesglosado_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar reporte de altas desglosado: {ex.Message}");
            }
        }

        // GET: api/Reportes/ReporteBajasConcentradoPDF
        [HttpGet("ReporteBajasConcentradoPDF")]
        public async Task<IActionResult> GenerarReporteBajasConcentradoPDF(
            [FromQuery] int idGeneral,
            [FromQuery] int idPantalla,
            [FromQuery] int idFinanciamiento,
            [FromQuery] int anio,
            [FromQuery] int mes,
            [FromQuery] int idUnidadResponsable, // Renombrado para coincidir con el SP
            [FromQuery] int idArea,
            [FromQuery] bool aplicaUMAS) // Renombrado para coincidir con el SP
        {
            try
            {
                // La estructura de datos se actualiza para reflejar las columnas del SP
                var concentradoData = new List<(string TipoBien, string Bien, string Financiamiento, int Cantidad, double CostoTotal)>();

                using (var connection = _context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "PA_SEL_ReporteBajasConcentrado";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@IdGeneral", idGeneral));
                        command.Parameters.Add(new SqlParameter("@IdPantalla", idPantalla));
                        command.Parameters.Add(new SqlParameter("@idFinanciamiento", idFinanciamiento));
                        command.Parameters.Add(new SqlParameter("@Anio", anio));
                        command.Parameters.Add(new SqlParameter("@Mes", mes));
                        command.Parameters.Add(new SqlParameter("@IdUnidadResponsable", idUnidadResponsable)); // Coincide con el nuevo nombre
                        command.Parameters.Add(new SqlParameter("@IdArea", idArea)); // Coincide con el nuevo nombre
                        command.Parameters.Add(new SqlParameter("@AplicaUMAS", aplicaUMAS)); // Coincide con el nuevo nombre

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                concentradoData.Add((
                                    reader["TipoBien"]?.ToString(),
                                    reader["Bien"]?.ToString(),
                                    reader["Financiamiento"]?.ToString(), // Nuevo campo
                                    Convert.ToInt32(reader["Cantidad"]),   // Nuevo campo
                                    Convert.ToDouble(reader["CostoTotal"])// Coincide con el nuevo nombre del SP
                                ));
                            }
                        }
                    }
                }

                using var ms = new MemoryStream();
                using var document = new PdfDocument();
                document.Info.Title = "Reporte de Bajas Concentrado"; // Título del documento

               
                PdfPage page = document.AddPage();
                page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                page.Size = PdfSharpCore.PageSize.A3;

                // Obtener las dimensiones de la página actual (ahora en landscape)
                double pageWidth = page.Width;
                double pageHeight = page.Height;

                // Márgenes verticales fijos
                double contentMarginTop = 150;
                double contentMarginBottom = 70;

                double col1Width = 330;  // TipoBien
                double col2Width = 250; // Bien
                double col3Width = 235;  // Financiamiento
                double col4Width = 60;  // Cantidad
                double col5Width = 75;  // CostoTotal
                double totalTableWidth = col1Width + col2Width + col3Width + col4Width + col5Width;

                // Calcular el margen izquierdo para centrar la tabla
                double contentMarginLeft = (pageWidth - totalTableWidth) / 2;


                XFont tableHeaderFont = new XFont("Arial", 9, XFontStyle.Bold);
                XFont tableContentFont = new XFont("Arial", 8, XFontStyle.Regular);
                XFont summaryFont = new XFont("Arial", 8, XFontStyle.Bold); // Para el total

                double cellPadding = 5;
                double rowHeight = 20;

                double currentY = contentMarginTop;
                int pageNumber = 1;

                XGraphics gfx = XGraphics.FromPdfPage(page);

                // Cambia el título del reporte en el encabezado
                DrawHeader(gfx, page, "REPORTE DE BAJAS CONCENTRADO", "BAJAS", anio, mes, idFinanciamiento, pageNumber, 1);
                DrawFooter(gfx, page);

                // Encabezados de la tabla
                gfx.DrawRectangle(XPens.Black, contentMarginLeft, currentY, totalTableWidth, rowHeight);
                gfx.DrawString("TIPO BIEN", tableHeaderFont, XBrushes.Black, new XRect(contentMarginLeft + cellPadding, currentY + cellPadding, col1Width, rowHeight), XStringFormats.TopCenter);
                gfx.DrawString("DESCRIPCIÓN BIEN", tableHeaderFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + cellPadding, currentY + cellPadding, col2Width, rowHeight), XStringFormats.TopCenter);
                gfx.DrawString("FINANCIAMIENTO", tableHeaderFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + col2Width + cellPadding, currentY + cellPadding, col3Width, rowHeight), XStringFormats.TopCenter);
                gfx.DrawString("CANTIDAD", tableHeaderFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + col2Width + col3Width + cellPadding, currentY + cellPadding, col4Width, rowHeight), XStringFormats.TopCenter);
                gfx.DrawString("COSTO TOTAL", tableHeaderFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + col2Width + col3Width + col4Width + cellPadding, currentY + cellPadding, col5Width, rowHeight), XStringFormats.TopCenter);

                gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width, currentY, contentMarginLeft + col1Width, currentY + rowHeight);
                gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width + col2Width, currentY, contentMarginLeft + col1Width + col2Width, currentY + rowHeight);
                gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width + col2Width + col3Width, currentY, contentMarginLeft + col1Width + col2Width + col3Width, currentY + rowHeight);
                gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width + col2Width + col3Width + col4Width, currentY, contentMarginLeft + col1Width + col2Width + col3Width + col4Width, currentY + rowHeight);

                currentY += rowHeight;
                double totalCostoGeneral = 0; // Renombrado para claridad

                foreach (var item in concentradoData)
                {
                    // Nota: page.Height ya reflejará la altura de la página en orientación Landscape.
                    if (currentY + rowHeight > pageHeight - contentMarginBottom) // Usar pageHeight
                    {
                        pageNumber++;
                        page = document.AddPage();
                        page.Orientation = PdfSharpCore.PageOrientation.Landscape; // Asegurarse de que las nuevas páginas también sean landscape
                        page.Size = PdfSharpCore.PageSize.A3;
                        gfx = XGraphics.FromPdfPage(page);

                        DrawHeader(gfx, page, "REPORTE DE BAJAS CONCENTRADO", "BAJAS", anio, mes, idFinanciamiento, pageNumber, 1);
                        DrawFooter(gfx, page);

                        currentY = contentMarginTop;
                        gfx.DrawRectangle(XPens.Black, contentMarginLeft, currentY, totalTableWidth, rowHeight);
                        gfx.DrawString("TIPO BIEN", tableHeaderFont, XBrushes.Black, new XRect(contentMarginLeft + cellPadding, currentY + cellPadding, col1Width, rowHeight), XStringFormats.TopCenter);
                        gfx.DrawString("DESCRIPCIÓN BIEN", tableHeaderFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + cellPadding, currentY + cellPadding, col2Width, rowHeight), XStringFormats.TopLeft);
                        gfx.DrawString("FINANCIAMIENTO", tableHeaderFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + col2Width + cellPadding, currentY + cellPadding, col3Width, rowHeight), XStringFormats.TopCenter);
                        gfx.DrawString("CANTIDAD", tableHeaderFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + col2Width + col3Width + cellPadding, currentY + cellPadding, col4Width, rowHeight), XStringFormats.TopLeft);
                        gfx.DrawString("COSTO TOTAL", tableHeaderFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + col2Width + col3Width + col4Width + cellPadding, currentY + cellPadding, col5Width, rowHeight), XStringFormats.TopLeft);

                        gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width, currentY, contentMarginLeft + col1Width, currentY + rowHeight);
                        gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width + col2Width, currentY, contentMarginLeft + col1Width + col2Width, currentY + rowHeight);
                        gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width + col2Width + col3Width, currentY, contentMarginLeft + col1Width + col2Width + col3Width, currentY + rowHeight);
                        gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width + col2Width + col3Width + col4Width, currentY, contentMarginLeft + col1Width + col2Width + col3Width + col4Width, currentY + rowHeight);
                        currentY += rowHeight;
                    }

                    gfx.DrawRectangle(XPens.Black, contentMarginLeft, currentY, totalTableWidth, rowHeight);
                    gfx.DrawString(item.TipoBien, tableContentFont, XBrushes.Black, new XRect(contentMarginLeft + cellPadding, currentY + cellPadding, col1Width, rowHeight), XStringFormats.TopLeft);
                    gfx.DrawString(item.Bien, tableContentFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + cellPadding, currentY + cellPadding, col2Width, rowHeight), XStringFormats.TopLeft);
                    gfx.DrawString(item.Financiamiento, tableContentFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + col2Width + cellPadding, currentY + cellPadding, col3Width, rowHeight), XStringFormats.TopLeft);
                    gfx.DrawString(item.Cantidad.ToString(), tableContentFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + col2Width + col3Width + cellPadding, currentY + cellPadding, col4Width, rowHeight), XStringFormats.TopCenter);
                    gfx.DrawString($"{item.CostoTotal:C2}", tableContentFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + col2Width + col3Width + col4Width - cellPadding, currentY + cellPadding, col5Width, rowHeight), XStringFormats.TopRight);

                    gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width, currentY, contentMarginLeft + col1Width, currentY + rowHeight);
                    gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width + col2Width, currentY, contentMarginLeft + col1Width + col2Width, currentY + rowHeight);
                    gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width + col2Width + col3Width, currentY, contentMarginLeft + col1Width + col2Width + col3Width, currentY + rowHeight);
                    gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width + col2Width + col3Width + col4Width, currentY, contentMarginLeft + col1Width + col2Width + col3Width + col4Width, currentY + rowHeight);

                    totalCostoGeneral += item.CostoTotal;
                    currentY += rowHeight;
                }

                if (concentradoData.Any())
                {
                    if (currentY + rowHeight > pageHeight - contentMarginBottom) // Usar pageHeight
                    {
                        pageNumber++;
                        page = document.AddPage();
                        page.Orientation = PdfSharpCore.PageOrientation.Landscape; // Asegurarse de que las nuevas páginas también sean landscape
                        page.Size = PdfSharpCore.PageSize.A3;
                        gfx = XGraphics.FromPdfPage(page);
                        DrawHeader(gfx, page, "REPORTE DE BAJAS CONCENTRADO", "BAJAS", anio, mes, idFinanciamiento, pageNumber, 1);
                        DrawFooter(gfx, page);
                        currentY = contentMarginTop;
                    }

                    gfx.DrawRectangle(XPens.Black, contentMarginLeft, currentY, totalTableWidth, rowHeight);
                    // El "TOTAL GENERAL" abarca las primeras 4 columnas
                    gfx.DrawString("TOTAL GENERAL", summaryFont, XBrushes.Black, new XRect(contentMarginLeft - 5, currentY + cellPadding, col1Width + col2Width + col3Width + col4Width, rowHeight), XStringFormats.TopRight);
                    gfx.DrawString($"{totalCostoGeneral:C2}", summaryFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + col2Width + col3Width + col4Width + cellPadding, currentY + cellPadding, col5Width, rowHeight), XStringFormats.TopCenter);
                    gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width + col2Width + col3Width + col4Width, currentY, contentMarginLeft + col1Width + col2Width + col3Width + col4Width, currentY + rowHeight);
                }

                document.Save(ms);
                ms.Position = 0;
                return File(ms.ToArray(), "application/pdf", $"ReporteBajasConcentrado_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar reporte de bajas concentrado: {ex.Message}");
            }
        }


        // GET: api/Reportes/ReporteBajasDetalladoPDF
        [HttpGet("ReporteBajasDetalladoPDF")]
        public async Task<IActionResult> GenerarReporteBajasDetalladoPDF(
            [FromQuery] int idGeneral,
            [FromQuery] int idPantalla,
            [FromQuery] int idFinanciamiento,
            [FromQuery] int anio,
            [FromQuery] int mes,
            [FromQuery] int unidadResponsable,
            [FromQuery] int idArea, // Nuevo parámetro para el procedimiento almacenado PA_SEL_ReporteBajasDetallado
            [FromQuery] bool umas)
        {
            try
            {
                // Se actualiza la tupla para reflejar los datos devueltos por el nuevo procedimiento almacenado
                var desglosadoData = new List<(string ClaveTipoBien, string TipoBienNombre, string ClaveBien, string BienDesc, string NoInventario, string Color, string Estado, string Aviso, string Marca, string Serie, string Modelo, double Costo, int IdCausal, string Disposicion, string FolioFiscal, string Observaciones)>();

                using (var connection = _context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        // Se cambia el nombre del procedimiento almacenado a PA_SEL_ReporteBajasDetallado
                        command.CommandText = "PA_SEL_ReporteBajasDetallado";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@IdGeneral", idGeneral));
                        command.Parameters.Add(new SqlParameter("@IdPantalla", idPantalla));
                        command.Parameters.Add(new SqlParameter("@idFinanciamiento", idFinanciamiento));
                        command.Parameters.Add(new SqlParameter("@Anio", anio));
                        command.Parameters.Add(new SqlParameter("@Mes", mes));
                        command.Parameters.Add(new SqlParameter("@UnidadResponsable", unidadResponsable));
                        command.Parameters.Add(new SqlParameter("@idArea", idArea)); // Se añade el nuevo parámetro
                        command.Parameters.Add(new SqlParameter("@Umas", umas));

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                desglosadoData.Add((
                                    reader["ClaveTipoBien"]?.ToString(),
                                    reader["TipoBienNombre"]?.ToString(),
                                    reader["ClaveBien"]?.ToString(),
                                    reader["BienDesc"]?.ToString(),
                                    reader["NoInventario"]?.ToString(),
                                    reader["Color"]?.ToString(), // Nuevo campo
                                    reader["Estado"]?.ToString(), // Nuevo campo
                                    reader["Aviso"]?.ToString(),
                                    reader["Marca"]?.ToString(),
                                    reader["Serie"]?.ToString(),
                                    reader["Modelo"]?.ToString(),
                                    Convert.ToDouble(reader["Costo"]),
                                    Convert.ToInt32(reader["idCausal"]), // Nuevo campo (int)
                                    reader["Disposicion"]?.ToString(), // Nuevo campo
                                    reader["FolioFiscal"]?.ToString(),
                                    reader["Observaciones"]?.ToString()
                                ));
                            }
                        }
                    }
                }

                // Agrupar los datos para la clasificación
                var groupedData = desglosadoData
                    .GroupBy(item => new { item.ClaveTipoBien, item.TipoBienNombre })
                    .Select(g1 => new
                    {
                        TipoBienKey = g1.Key,
                        TipoBienItems = g1.GroupBy(item => new { item.ClaveBien, item.BienDesc })
                                             .Select(g2 => new
                                             {
                                                 BienKey = g2.Key,
                                                 BienItems = g2.OrderBy(item => item.NoInventario).ToList(),
                                                 TotalArticulosBien = g2.Count(),
                                                 CostoTotalBien = g2.Sum(item => item.Costo)
                                             }).ToList(),
                        SubtotalTipoBien = g1.Sum(item => item.Costo)
                    }).ToList();

                using var ms = new MemoryStream();
                using var document = new PdfDocument();
                document.Info.Title = "Reporte de Bajas Detallado"; // Se actualiza el título del documento

                double contentMarginTop = 150;
                double contentMarginBottom = 70;
                double contentMarginLeft = 40;
                double contentMarginRight = 40;

                PdfPage page = document.AddPage();
                page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                page.Size = PdfSharpCore.PageSize.Legal;
                XGraphics gfx = XGraphics.FromPdfPage(page); // Create XGraphics object once per page
                double contentWidth = page.Width - contentMarginLeft - contentMarginRight;

                XFont tableHeaderFont = new XFont("Arial", 8, XFontStyle.Bold);
                XFont tableContentFont = new XFont("Arial", 7, XFontStyle.Regular);
                XFont groupHeaderFont = new XFont("Arial", 9, XFontStyle.Bold);
                XFont summaryFont = new XFont("Arial", 8, XFontStyle.Regular);
                double rowHeight = 18;
                double cellPadding = 3;

                XPen dottedBottomBorderPen = new XPen(XColor.FromArgb(150, 0, 0, 0), 0.5); // Lighter black, thinner line
                dottedBottomBorderPen.DashStyle = XDashStyle.Dot;

                // Se actualizan las columnas y sus anchos para el reporte de bajas
                var columnHeaders = new (string Name, double WidthFactor)[]
                {
                    //("DESCRIPCIÓN BIEN", 0.12),
                    ("NO. INVENT", 0.10),
                    ("COLOR", 0.05),
                    ("ESTADO", 0.05),
                    ("AVISO", 0.10),
                    ("FOLIO FIS.", 0.08),
                    ("MARCA", 0.05),
                    ("SERIE", 0.09),
                    ("MODELO", 0.09),
                    ("COSTO", 0.04),
                    ("CAUSAL", 0.05),
                    ("DISPOSICIÓN FINAL", 0.10),
                    ("OBSERVACIONES", 0.20)
                };

                double[] colWidths = columnHeaders.Select(ch => ch.WidthFactor * contentWidth).ToArray();

                int pageNumber = 1;
                double currentY = contentMarginTop;
                double currentX;

                double totalGeneralPesos = 0;

                Action DrawTableHeaders = () =>
                {
                    currentX = contentMarginLeft;

                    // 1. Fila de encabezado agrupado: "DESCRIPCIÓN BIEN"
                    double descripcionBienWidth = colWidths[0] + colWidths[1]; // Agrupa NO. INVENT + COLOR
                    gfx.DrawRectangle(XPens.Black, currentX, currentY, descripcionBienWidth, rowHeight);
                    gfx.DrawString("DESCRIPCIÓN BIEN", tableHeaderFont, XBrushes.Black,
                        new XRect(currentX, currentY, descripcionBienWidth, rowHeight), XStringFormats.Center);

                    currentY += rowHeight;

                    // 2. Segunda fila: encabezados normales
                    currentX = contentMarginLeft;
                    for (int i = 0; i < columnHeaders.Length; i++)
                    {
                        gfx.DrawRectangle(XPens.Black, currentX, currentY, colWidths[i], rowHeight);
                        gfx.DrawString(columnHeaders[i].Name, tableHeaderFont, XBrushes.Black,
                            new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[i] - cellPadding * 2, rowHeight - cellPadding * 2), XStringFormats.Center);
                        currentX += colWidths[i];
                    }

                    currentY += rowHeight;
                };

                // Se actualiza el título del encabezado del reporte
                DrawHeader(gfx, page, "REPORTE DE BAJAS DETALLADO", "BAJAS", anio, mes, idFinanciamiento, pageNumber, 1);
                DrawFooter(gfx, page);
                DrawTableHeaders(); // Dibuja los encabezados de la tabla por primera vez

                foreach (var tipoBienGroup in groupedData)
                {
                    if (currentY + rowHeight * 2 > page.Height - contentMarginBottom)
                    {
                        pageNumber++;
                        page = document.AddPage();
                        page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                        page.Size = PdfSharpCore.PageSize.Legal;
                        gfx = XGraphics.FromPdfPage(page); // Re-create XGraphics for the new page
                        DrawHeader(gfx, page, "REPORTE DE BAJAS DETALLADO", "BAJAS", anio, mes, idFinanciamiento, pageNumber, 1);
                        DrawFooter(gfx, page);
                        currentY = contentMarginTop;
                        DrawTableHeaders();
                    }

                    // Dibuja el encabezado del tipo de bien (CAT_TIPOSBIENES)
                    string tipoBienHeader = $"{tipoBienGroup.TipoBienKey.ClaveTipoBien} {tipoBienGroup.TipoBienKey.TipoBienNombre}";
                    gfx.DrawString(tipoBienHeader, groupHeaderFont, XBrushes.Black,
                        new XRect(contentMarginLeft, currentY + cellPadding, contentWidth, rowHeight), XStringFormats.TopLeft);
                    currentY += rowHeight;

                    foreach (var bienGroup in tipoBienGroup.TipoBienItems)
                    {
                        if (currentY + rowHeight * 3 > page.Height - contentMarginBottom)
                        {
                            pageNumber++;
                            page = document.AddPage();
                            page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                            page.Size = PdfSharpCore.PageSize.Legal;
                            gfx = XGraphics.FromPdfPage(page); // Re-create XGraphics for the new page
                            DrawHeader(gfx, page, "REPORTE DE BAJAS DETALLADO", "BAJAS", anio, mes, idFinanciamiento, pageNumber, 1);
                            DrawFooter(gfx, page);
                            currentY = contentMarginTop;
                            DrawTableHeaders();
                            gfx.DrawString(tipoBienHeader, groupHeaderFont, XBrushes.Black,
                                new XRect(contentMarginLeft, currentY + cellPadding, contentWidth, rowHeight), XStringFormats.TopLeft);
                            currentY += rowHeight;
                        }

                        // Dibuja el encabezado del bien (CAT_BIENES)
                        string bienHeader = $"{bienGroup.BienKey.ClaveBien} {bienGroup.BienKey.BienDesc}";
                        gfx.DrawString(bienHeader, summaryFont, XBrushes.Black,
                            new XRect(contentMarginLeft + 10, currentY + cellPadding, contentWidth - 10, rowHeight), XStringFormats.TopLeft);
                        currentY += rowHeight;

                        foreach (var item in bienGroup.BienItems)
                        {
                            if (currentY + rowHeight > page.Height - contentMarginBottom)
                            {
                                pageNumber++;
                                page = document.AddPage();
                                page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                                page.Size = PdfSharpCore.PageSize.Legal;
                                gfx = XGraphics.FromPdfPage(page); // Re-create XGraphics for the new page
                                DrawHeader(gfx, page, "REPORTE DE BAJAS DETALLADO", "BAJAS", anio, mes, idFinanciamiento, pageNumber, 1);
                                DrawFooter(gfx, page);
                                currentY = contentMarginTop;
                                DrawTableHeaders();
                                gfx.DrawString(tipoBienHeader, groupHeaderFont, XBrushes.Black,
                                    new XRect(contentMarginLeft, currentY + cellPadding, contentWidth, rowHeight), XStringFormats.TopLeft);
                                currentY += rowHeight;
                                gfx.DrawString(bienHeader, groupHeaderFont, XBrushes.Black,
                                    new XRect(contentMarginLeft + 10, currentY + cellPadding, contentWidth - 10, rowHeight), XStringFormats.TopLeft);
                                currentY += rowHeight;
                            }

                            gfx.DrawLine(dottedBottomBorderPen, contentMarginLeft, currentY + rowHeight, contentMarginLeft + contentWidth, currentY + rowHeight);

                            currentX = contentMarginLeft;

                            /*/ Se actualiza el orden y los campos dibujados para que coincidan con la nueva estructura
                            gfx.DrawString(item.BienDesc, tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[0] - cellPadding * 2, rowHeight - cellPadding * 2), XStringFormats.TopLeft);
                            currentX += colWidths[0];*/

                            gfx.DrawString(item.NoInventario, tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[0] - cellPadding * 2, rowHeight - cellPadding * 2), XStringFormats.TopLeft);
                            currentX += colWidths[0];

                            gfx.DrawString(item.Color, tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[1] - cellPadding * 2, rowHeight - cellPadding * 2), XStringFormats.TopLeft);
                            currentX += colWidths[1];

                            gfx.DrawString(item.Estado, tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[2] - cellPadding * 2, rowHeight - cellPadding * 2), XStringFormats.TopCenter);
                            currentX += colWidths[2];

                            gfx.DrawString(item.Aviso, tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[3] - cellPadding * 2, rowHeight - cellPadding * 2), XStringFormats.TopLeft);
                            currentX += colWidths[3];

                            gfx.DrawString(item.FolioFiscal, tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[4] - cellPadding * 2, rowHeight - cellPadding * 2), XStringFormats.TopCenter);
                            currentX += colWidths[4];

                            gfx.DrawString(item.Marca, tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[5] - cellPadding * 2, rowHeight - cellPadding * 2), XStringFormats.TopCenter);
                            currentX += colWidths[5];

                            gfx.DrawString(item.Serie, tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[6] - cellPadding * 2, rowHeight - cellPadding * 2), XStringFormats.TopLeft);
                            currentX += colWidths[6];

                            gfx.DrawString(item.Modelo, tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[7] - cellPadding * 2, rowHeight - cellPadding * 2), XStringFormats.TopLeft);
                            currentX += colWidths[7];

                            gfx.DrawString($"{item.Costo:C2}", tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[8] - cellPadding * 2, rowHeight - cellPadding * 2), XStringFormats.TopRight); // Se mantiene a la derecha para números
                            currentX += colWidths[8];

                            gfx.DrawString(item.IdCausal.ToString(), tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[9] - cellPadding * 2, rowHeight - cellPadding * 2), XStringFormats.TopCenter);
                            currentX += colWidths[9];

                            gfx.DrawString(item.Disposicion, tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[10] - cellPadding * 2, rowHeight - cellPadding * 2), XStringFormats.TopLeft);
                            currentX += colWidths[10];

                            gfx.DrawString(item.Observaciones, tableContentFont, XBrushes.Black,
                                new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[11] - cellPadding * 2, rowHeight - cellPadding * 2), XStringFormats.TopLeft);
                            currentX += colWidths[11];

                            currentY += rowHeight;
                        }

                        // Dibuja el "Total de artículos del bien"
                        if (currentY + rowHeight > page.Height - contentMarginBottom)
                        {
                            pageNumber++;
                            page = document.AddPage();
                            page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                            page.Size = PdfSharpCore.PageSize.Legal;
                            gfx = XGraphics.FromPdfPage(page); // Re-create XGraphics for the new page
                            DrawHeader(gfx, page, "REPORTE DE BAJAS DETALLADO", "BAJAS", anio, mes, idFinanciamiento, pageNumber, 1);
                            DrawFooter(gfx, page);
                            currentY = contentMarginTop;
                            DrawTableHeaders();
                            gfx.DrawString(tipoBienHeader, groupHeaderFont, XBrushes.Black,
                                new XRect(contentMarginLeft, currentY + cellPadding, contentWidth, rowHeight), XStringFormats.TopLeft);
                            currentY += rowHeight;
                            gfx.DrawString(bienHeader, groupHeaderFont, XBrushes.Black,
                                new XRect(contentMarginLeft + 10, currentY + cellPadding, contentWidth - 10, rowHeight), XStringFormats.TopLeft);
                            currentY += rowHeight;
                        }

                        // Dibuja el total de artículos del bien y el costo total del bien
                        string totalArticulosBien = $"TOTAL DE ARTÍCULOS DEL BIEN: {bienGroup.TotalArticulosBien}";

                        gfx.DrawString(totalArticulosBien, summaryFont, XBrushes.Black,
                            new XRect(contentMarginLeft + 20, currentY + cellPadding, contentWidth - 20, rowHeight - cellPadding * 2), XStringFormats.TopLeft); // Alineado a la izquierda
                        currentY += rowHeight;

                        if (currentY + rowHeight > page.Height - contentMarginBottom)
                        {
                            pageNumber++;
                            page = document.AddPage();
                            page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                            page.Size = PdfSharpCore.PageSize.Legal;
                            gfx = XGraphics.FromPdfPage(page); // Re-create XGraphics for the new page
                            DrawHeader(gfx, page, "REPORTE DE BAJAS DETALLADO", "BAJAS", anio, mes, idFinanciamiento, pageNumber, 1);
                            DrawFooter(gfx, page);
                            currentY = contentMarginTop;
                            DrawTableHeaders();
                            gfx.DrawString(tipoBienHeader, groupHeaderFont, XBrushes.Black,
                                new XRect(contentMarginLeft, currentY + cellPadding, contentWidth, rowHeight), XStringFormats.TopLeft);
                            currentY += rowHeight;
                            gfx.DrawString(bienHeader, groupHeaderFont, XBrushes.Black,
                                new XRect(contentMarginLeft + 10, currentY + cellPadding, contentWidth - 10, rowHeight), XStringFormats.TopLeft);
                            currentY += rowHeight;
                        }
                        currentY += rowHeight + 5;
                    }

                    // Dibuja el "Subtotal por tipo de bien"
                    if (currentY + rowHeight > page.Height - contentMarginBottom)
                    {
                        pageNumber++;
                        page = document.AddPage();
                        page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                        page.Size = PdfSharpCore.PageSize.Legal;
                        gfx = XGraphics.FromPdfPage(page); // Re-create XGraphics for the new page
                        DrawHeader(gfx, page, "REPORTE DE BAJAS DETALLADO", "BAJAS", anio, mes, idFinanciamiento, pageNumber, 1);
                        DrawFooter(gfx, page);
                        currentY = contentMarginTop;
                        DrawTableHeaders();
                    }

                    string subtotalTipoBienText = $"SUBTOTAL POR TIPO DE BIEN: {tipoBienGroup.SubtotalTipoBien:C2}";
                    gfx.DrawString(subtotalTipoBienText, summaryFont, XBrushes.Black,
                        new XRect(contentMarginLeft, currentY + cellPadding, contentWidth, rowHeight - cellPadding * 2), XStringFormats.TopLeft); // Alineado a la izquierda
                    currentY += rowHeight + 10;

                    totalGeneralPesos += tipoBienGroup.SubtotalTipoBien;
                }

                // Dibuja el "Total general en pesos"
                if (currentY + rowHeight > page.Height - contentMarginBottom)
                {
                    pageNumber++;
                    page = document.AddPage();
                    page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                    page.Size = PdfSharpCore.PageSize.Legal;
                    gfx = XGraphics.FromPdfPage(page); // Re-create XGraphics for the new page
                    DrawHeader(gfx, page, "REPORTE DE BAJAS DETALLADO", "BAJAS", anio, mes, idFinanciamiento, pageNumber, 1);
                    DrawFooter(gfx, page);
                    currentY = contentMarginTop;
                    DrawTableHeaders();
                }
                string totalGeneralText = $"TOTAL GENERAL EN PESOS: {totalGeneralPesos:C2}";
                gfx.DrawString(totalGeneralText, groupHeaderFont, XBrushes.Black,
                    new XRect(contentMarginLeft, currentY + cellPadding, contentWidth, rowHeight - cellPadding * 2), XStringFormats.TopLeft); // Alineado a la izquierda

                document.Save(ms);
                ms.Position = 0;
                // Se actualiza el nombre del archivo PDF generado
                return File(ms.ToArray(), "application/pdf", $"ReporteBajasDetallado_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar reporte de bajas detallado: {ex.Message}");
            }
        }

        private void DrawHeader(XGraphics gfx, PdfPage page, string reportTitle, string tipoReporte, int anio, int mes, int idFinanciamiento, int currentPage, int totalPages)
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

            string mesNombre = mes == 0 ? "0" :CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mes);
            string fechaReporte = mes == 0 ? $"{anio}" : $"{mesNombre.ToUpper()} DE {anio}"; // Mes 0 para anual, Mes X para mensual
            gfx.DrawString($"CONCENTRADO DE {tipoReporte} CORRESPONDIENTE A: {fechaReporte}", fontBold, XBrushes.Black, new XRect(0, y + 60, page.Width, fontSubTitle.Height), XStringFormats.TopCenter);

            string financiamientoText = idFinanciamiento == 0 ? "TODOS" : idFinanciamiento.ToString();
            gfx.DrawString($"FUENTE DE FINANCIAMIENTO: {financiamientoText}", fontBold, XBrushes.Black, new XRect(0, y + 75, page.Width, fontSubTitle.Height), XStringFormats.TopCenter);

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
