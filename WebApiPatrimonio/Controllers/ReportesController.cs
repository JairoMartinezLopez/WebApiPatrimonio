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
using WebApiPatrimonio.Models;

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
            [FromQuery] int idUnidadResponsable,
            [FromQuery] int idArea,
            [FromQuery] bool aplicaUMAS)
        {
            try
            {
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
                        command.Parameters.Add(new SqlParameter("@IdUnidadResponsable", idUnidadResponsable));
                        command.Parameters.Add(new SqlParameter("@IdArea", idArea));
                        command.Parameters.Add(new SqlParameter("@AplicaUMAS", aplicaUMAS));

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                concentradoData.Add((
                                    reader["TipoBien"]?.ToString(),
                                    reader["Bien"]?.ToString(),
                                    reader["Financiamiento"]?.ToString(), 
                                    Convert.ToInt32(reader["Cantidad"]),  
                                    Convert.ToDouble(reader["CostoTotal"])
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

                // Obtener las dimensiones de la página actual
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
                double totalCostoGeneral = 0;

                foreach (var item in concentradoData)
                {
                    if (currentY + rowHeight > pageHeight - contentMarginBottom)
                    {
                        pageNumber++;
                        page = document.AddPage();
                        page.Orientation = PdfSharpCore.PageOrientation.Landscape;
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

        // GET: api/Reportes/ReporteTransferenciaConcentradoPDF
        [HttpGet("ReporteTransferenciaConcentradoPDF")]
        public async Task<IActionResult> GenerarReporteTransferenciaConcentradoPDF(
            [FromQuery] int idGeneral,
            [FromQuery] int idPantalla,
            [FromQuery] int idFinanciamiento,
            [FromQuery] int anio,
            [FromQuery] int mes,
            [FromQuery] int idUnidadResponsable, // Corresponds to @UnidadResponsable in SP
            [FromQuery] bool aplicaUMAS) // Corresponds to @Umas in SP
        {
            try
            {
                // La estructura de datos se actualiza para reflejar las columnas del SP
                var concentradoData = new List<(string TipoBien, string Bien, double CostoTotal)>();

                using (var connection = _context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "PA_SEL_ReporteTransferenciaConcentrado"; // Nombre del SP actualizado
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@IdGeneral", idGeneral));
                        command.Parameters.Add(new SqlParameter("@IdPantalla", idPantalla));
                        command.Parameters.Add(new SqlParameter("@idFinanciamiento", idFinanciamiento));
                        command.Parameters.Add(new SqlParameter("@Anio", anio));
                        command.Parameters.Add(new SqlParameter("@Mes", mes));
                        command.Parameters.Add(new SqlParameter("@UnidadResponsable", idUnidadResponsable)); // Renombrado para coincidir con el SP
                        command.Parameters.Add(new SqlParameter("@Umas", aplicaUMAS)); // Renombrado para coincidir con el SP

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                concentradoData.Add((
                                    reader["TipoBien"]?.ToString(),
                                    reader["Nombre"]?.ToString(), // 'Nombre' del SP mapea a 'Bien'
                                    Convert.ToDouble(reader["Importe"]) // 'Importe' del SP mapea a 'CostoTotal'
                                ));
                            }
                        }
                    }
                }

                using var ms = new MemoryStream();
                using var document = new PdfDocument();
                document.Info.Title = "Reporte de Transferencias Concentrado"; // Título del documento actualizado


                PdfPage page = document.AddPage();
                page.Size = PdfSharpCore.PageSize.A4;

                // Obtener las dimensiones de la página actual (ahora en landscape)
                double pageWidth = page.Width;
                double pageHeight = page.Height;

                // Márgenes verticales fijos
                double contentMarginTop = 150;
                double contentMarginBottom = 70;

                // Anchos de columna ajustados para las columnas existentes en el SP
                double col1Width = 80;  // TipoBien
                double col2Width = 350;  // Bien 
                double col3Width = 85;  // CostoTotal
                double totalTableWidth = col1Width + col2Width + col3Width;

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
                DrawHeader(gfx, page, "REPORTE DE TRANSFERENCIAS CONCENTRADO", "TRANSFERENCIAS", anio, mes, idFinanciamiento, pageNumber, 1);
                DrawFooter(gfx, page);

                // Encabezados de la tabla
                gfx.DrawRectangle(XPens.Black, contentMarginLeft, currentY, totalTableWidth, rowHeight);
                gfx.DrawString("TIPO BIEN", tableHeaderFont, XBrushes.Black, new XRect(contentMarginLeft + cellPadding, currentY + cellPadding, col1Width, rowHeight), XStringFormats.TopCenter);
                gfx.DrawString("DESCRIPCIÓN BIEN", tableHeaderFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + cellPadding, currentY + cellPadding, col2Width, rowHeight), XStringFormats.TopCenter);
                gfx.DrawString("IMPORTE", tableHeaderFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + col2Width + cellPadding, currentY + cellPadding, col3Width, rowHeight), XStringFormats.TopCenter);

                gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width, currentY, contentMarginLeft + col1Width, currentY + rowHeight);
                gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width + col2Width, currentY, contentMarginLeft + col1Width + col2Width, currentY + rowHeight);

                currentY += rowHeight;
                double totalCostoGeneral = 0; 

                foreach (var item in concentradoData)
                {
                    if (currentY + rowHeight > pageHeight - contentMarginBottom)
                    {
                        pageNumber++;
                        page = document.AddPage();
                        page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                        page.Size = PdfSharpCore.PageSize.A3;
                        gfx = XGraphics.FromPdfPage(page);

                        DrawHeader(gfx, page, "REPORTE DE TRANSFERENCIAS CONCENTRADO", "TRANSFERENCIAS", anio, mes, idFinanciamiento, pageNumber, 1);
                        DrawFooter(gfx, page);

                        currentY = contentMarginTop;
                        gfx.DrawRectangle(XPens.Black, contentMarginLeft, currentY, totalTableWidth, rowHeight);
                        gfx.DrawString("TIPO BIEN", tableHeaderFont, XBrushes.Black, new XRect(contentMarginLeft + cellPadding, currentY + cellPadding, col1Width, rowHeight), XStringFormats.TopCenter);
                        gfx.DrawString("DESCRIPCIÓN BIEN", tableHeaderFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + cellPadding, currentY + cellPadding, col2Width, rowHeight), XStringFormats.TopLeft);
                        gfx.DrawString("IMPORTE", tableHeaderFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + col2Width + cellPadding, currentY + cellPadding, col3Width, rowHeight), XStringFormats.TopLeft);

                        gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width, currentY, contentMarginLeft + col1Width, currentY + rowHeight);
                        gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width + col2Width, currentY, contentMarginLeft + col1Width + col2Width, currentY + rowHeight);

                        currentY += rowHeight;
                    }

                    gfx.DrawRectangle(XPens.Black, contentMarginLeft, currentY, totalTableWidth, rowHeight);
                    gfx.DrawString(item.TipoBien, tableContentFont, XBrushes.Black, new XRect(contentMarginLeft + cellPadding, currentY + cellPadding, col1Width, rowHeight), XStringFormats.TopCenter);
                    gfx.DrawString(item.Bien, tableContentFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + cellPadding, currentY + cellPadding, col2Width, rowHeight), XStringFormats.TopLeft);
                    gfx.DrawString($"{item.CostoTotal:C2}", tableContentFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + col2Width + cellPadding, currentY + cellPadding, col3Width, rowHeight), XStringFormats.TopCenter);

                    gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width, currentY, contentMarginLeft + col1Width, currentY + rowHeight);
                    gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width + col2Width, currentY, contentMarginLeft + col1Width + col2Width, currentY + rowHeight);

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
                        DrawHeader(gfx, page, "REPORTE DE TRANSFERENCIAS CONCENTRADO", "TRANSFERENCIAS", anio, mes, idFinanciamiento, pageNumber, 1);
                        DrawFooter(gfx, page);
                        currentY = contentMarginTop;
                    }

                    gfx.DrawRectangle(XPens.Black, contentMarginLeft, currentY, totalTableWidth, rowHeight);
                    // El "TOTAL GENERAL" abarca las primeras 2 columnas (TipoBien y Descripción Bien)
                    gfx.DrawString("TOTAL GENERAL", summaryFont, XBrushes.Black, new XRect(contentMarginLeft - cellPadding, currentY + cellPadding, col1Width + col2Width, rowHeight), XStringFormats.TopRight);
                    gfx.DrawString($"{totalCostoGeneral:C2}", summaryFont, XBrushes.Black, new XRect(contentMarginLeft + col1Width + col2Width + cellPadding, currentY + cellPadding, col3Width, rowHeight), XStringFormats.TopCenter);
                    gfx.DrawLine(XPens.Black, contentMarginLeft + col1Width + col2Width, currentY, contentMarginLeft + col1Width + col2Width, currentY + rowHeight);
                }

                document.Save(ms);
                ms.Position = 0;
                return File(ms.ToArray(), "application/pdf", $"ReporteTransferenciaConcentrado_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar reporte de transferencias concentrado: {ex.Message}");
            }
        }

        // GET: api/Reportes/ReporteTransferenciaDesarrolladoPDF
        [HttpGet("ReporteTransferenciaDesarrolladoPDF")]
        public async Task<IActionResult> GenerarReporteTransferenciaDesarrolladoPDF(
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
                // Se actualiza la tupla para reflejar los datos devueltos por el SP PA_SEL_ReporteTransferenciaDesarrollado
                var desglosadoData = new List<(string ClaveTipoBien, string NombreTipoBien, string ClaveBien, string BienDesc, string Unidad, string NoInventario, string Aviso, string Marca, string Serie, string Modelo, double Costo, DateTime FechaAlta, string Observaciones, string FolioFiscal, string NumeroFactura, string Adscripcion)>();

                using (var connection = _context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "PA_SEL_ReporteTransferenciaDesarrollado"; // Nombre del SP actualizado
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@IdGeneral", idGeneral));
                        command.Parameters.Add(new SqlParameter("@IdPantalla", idPantalla));
                        command.Parameters.Add(new SqlParameter("@idFinanciamiento", idFinanciamiento));
                        command.Parameters.Add(new SqlParameter("@Anio", anio));
                        command.Parameters.Add(new SqlParameter("@Mes", mes));
                        command.Parameters.Add(new SqlParameter("@UnidadResponsable", unidadResponsable));
                        command.Parameters.Add(new SqlParameter("@Umas", umas));
                        // El SP PA_SEL_ReporteTransferenciaDesarrollado no tiene el parámetro @idArea, por lo que lo eliminamos

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                desglosadoData.Add((
                                    reader["Clave"]?.ToString(), // Clave de TipoBien
                                    reader["NombreTipoBien"]?.ToString(), // Nombre de TipoBien
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
                                    reader["NumeroFactura"]?.ToString(),
                                    reader["Adscripcion"]?.ToString() // Nuevo campo
                                ));
                            }
                        }
                    }
                }

                // Agrupar los datos para la clasificación
                var groupedData = desglosadoData
                    .GroupBy(item => new { item.ClaveTipoBien, item.NombreTipoBien })
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
                document.Info.Title = "Reporte de Transferencias Detallado"; // Actualizado el título del documento

                // Definir el tamaño de página y la orientación a Landscape (Legal horizontal)
                PdfPage page = document.AddPage();
                page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                page.Size = PdfSharpCore.PageSize.Legal; // PageSize.Legal para un ancho mayor

                XGraphics gfx = XGraphics.FromPdfPage(page);

                // Recalcular el contentWidth real después de establecer el tamaño y orientación de la página
                double contentMarginTop = 150;
                double contentMarginBottom = 70;
                // Estos serán ajustados para centrar la tabla
                double contentMarginLeft = 0;
                double contentMarginRight = 0;

                XFont tableHeaderFont = new XFont("Arial", 8, XFontStyle.Bold);
                XFont tableContentFont = new XFont("Arial", 7, XFontStyle.Regular);
                XFont groupHeaderFont = new XFont("Arial", 9, XFontStyle.Bold);
                XFont summaryFont = new XFont("Arial", 8, XFontStyle.Regular);
                double rowHeight = 18;
                double cellPadding = 3;
                double lineHeight = 10;

                XPen dottedBottomBorderPen = new XPen(XColor.FromArgb(150, 0, 0, 0), 0.5);
                dottedBottomBorderPen.DashStyle = XDashStyle.Dot;

                // Definir las columnas y sus anchos para el reporte de transferencias detallado
                var columnHeaders = new (string Name, double WidthFactor)[]
                {
                ("UNIDAD", 0.04),
                ("NO. INVENT", 0.10),
                ("AVISO", 0.11),
                ("FACTURA", 0.05), // Asumo que se quiere NumeroFactura aquí
                ("FOLIO FIS.", 0.09),
                ("MARCA", 0.04),
                ("SERIE", 0.09),
                ("MODELO", 0.09),
                ("COSTO", 0.05),
                ("FECHA ALTA", 0.08), // FechaAlta del SP
                ("OBSERVACIONES", 0.15),
                ("ADSCRIPCIÓN", 0.10) // Nuevo campo del SP
                };

                // Sumar los factores para obtener el total para las columnas mostradas
                double totalFactor = columnHeaders.Sum(ch => ch.WidthFactor);
                // Definir el ancho total máximo disponible para la tabla, dejando espacio para márgenes razonables.
                double availableContentWidth = page.Width - 80;

                // Calcular los anchos de columna absolutos, normalizando para el ancho disponible
                double[] colWidths = columnHeaders.Select(ch => ch.WidthFactor * availableContentWidth / totalFactor).ToArray();

                // Calcular el ancho total de la tabla (real)
                double actualTableWidth = colWidths.Sum();

                // Recalcular contentMarginLeft para centrar la tabla
                contentMarginLeft = (page.Width - actualTableWidth) / 2;
                contentMarginRight = contentMarginLeft; // Para ser simétrico

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
                DrawHeader(gfx, page, "REPORTE DE TRANSFERENCIAS DETALLADO", "TRANSFERENCIAS", anio, mes, idFinanciamiento, pageNumber, 1);
                DrawFooter(gfx, page);
                DrawTableHeaders(); // Dibuja los encabezados de la tabla por primera vez

                foreach (var tipoBienGroup in groupedData)
                {
                    // Check for page break before drawing TipoBien header
                    if (currentY + rowHeight * 2 > page.Height - contentMarginBottom)
                    {
                        pageNumber++;
                        page = document.AddPage();
                        page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                        page.Size = PdfSharpCore.PageSize.Legal;
                        gfx = XGraphics.FromPdfPage(page);

                        // Recalculate margins and widths for the new page
                        availableContentWidth = page.Width - 80;
                        colWidths = columnHeaders.Select(ch => ch.WidthFactor * availableContentWidth / totalFactor).ToArray();
                        actualTableWidth = colWidths.Sum();
                        contentMarginLeft = (page.Width - actualTableWidth) / 2;

                        DrawHeader(gfx, page, "REPORTE DE TRANSFERENCIAS DETALLADO", "TRANSFERENCIAS", anio, mes, idFinanciamiento, pageNumber, 1);
                        DrawFooter(gfx, page);
                        currentY = contentMarginTop;
                        DrawTableHeaders();
                    }

                    // Dibuja el encabezado del tipo de bien (CAT_TIPOSBIENES)
                    string tipoBienHeader = $"{tipoBienGroup.TipoBienKey.ClaveTipoBien} {tipoBienGroup.TipoBienKey.NombreTipoBien}";
                    gfx.DrawString(tipoBienHeader, groupHeaderFont, XBrushes.Black,
                        new XRect(contentMarginLeft, currentY + cellPadding, actualTableWidth, rowHeight), XStringFormats.TopLeft);
                    currentY += rowHeight;

                    foreach (var bienGroup in tipoBienGroup.TipoBienItems)
                    {
                        // Check for page break before drawing Bien header
                        if (currentY + rowHeight * 3 > page.Height - contentMarginBottom)
                        {
                            pageNumber++;
                            page = document.AddPage();
                            page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                            page.Size = PdfSharpCore.PageSize.Legal;
                            gfx = XGraphics.FromPdfPage(page);

                            // Recalculate margins and widths for the new page
                            availableContentWidth = page.Width - 80;
                            colWidths = columnHeaders.Select(ch => ch.WidthFactor * availableContentWidth / totalFactor).ToArray();
                            actualTableWidth = colWidths.Sum();
                            contentMarginLeft = (page.Width - actualTableWidth) / 2;

                            DrawHeader(gfx, page, "REPORTE DE TRANSFERENCIAS DETALLADO", "TRANSFERENCIAS", anio, mes, idFinanciamiento, pageNumber, 1);
                            DrawFooter(gfx, page);
                            currentY = contentMarginTop;
                            DrawTableHeaders();
                            gfx.DrawString(tipoBienHeader, groupHeaderFont, XBrushes.Black,
                                new XRect(contentMarginLeft, currentY + cellPadding, actualTableWidth, rowHeight), XStringFormats.TopLeft);
                            currentY += rowHeight;
                        }

                        // Dibuja el encabezado del bien (CAT_BIENES)
                        string bienHeader = $"{bienGroup.BienKey.ClaveBien} {bienGroup.BienKey.BienDesc}";
                        gfx.DrawString(bienHeader, summaryFont, XBrushes.Black,
                            new XRect(contentMarginLeft + 10, currentY + cellPadding, actualTableWidth - 10, rowHeight), XStringFormats.TopLeft);
                        currentY += rowHeight;

                        // Dibujar la línea horizontal debajo de la descripción del bien.
                        gfx.DrawLine(XPens.Black, contentMarginLeft, currentY, contentMarginLeft + actualTableWidth, currentY);


                        foreach (var item in bienGroup.BienItems)
                        {
                            // Calcular altura de fila en función del contenido
                            string[] textos = {
                                item.Unidad,
                                item.NoInventario,
                                item.Aviso,
                                item.NumeroFactura,
                                item.FolioFiscal,
                                item.Marca,
                                item.Serie,
                                item.Modelo,
                                $"{item.Costo:C2}",
                                item.FechaAlta.ToString("dd/MM/yyyy"),
                                item.Observaciones,
                                item.Adscripcion
                            };

                            double maxLineCount = 1;

                            List<List<string>> wrappedTexts = new List<List<string>>();

                            for (int i = 0; i < textos.Length; i++)
                            {
                                string text = textos[i] ?? string.Empty;
                                double cellWidth = colWidths[i] - cellPadding * 2;
                                // Calcular el número de caracteres por línea basándose en el ancho real de la fuente
                                int aproxCharPerLine = (int)(cellWidth / tableContentFont.Size * (1.0 / 0.6)); // Ajuste para un ancho más preciso
                                if (aproxCharPerLine <= 0) aproxCharPerLine = 1;

                                var lines = wrapText(text, aproxCharPerLine);
                                wrappedTexts.Add(lines);
                                maxLineCount = Math.Max(maxLineCount, lines.Count);
                            }

                            double adjustedRowHeight = maxLineCount * 10;

                            // Saltar página si no hay espacio suficiente
                            if (currentY + adjustedRowHeight > page.Height - contentMarginBottom)
                            {
                                pageNumber++;
                                page = document.AddPage();
                                page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                                page.Size = PdfSharpCore.PageSize.Legal;
                                gfx = XGraphics.FromPdfPage(page);
                                availableContentWidth = page.Width - 80;
                                colWidths = columnHeaders.Select(ch => ch.WidthFactor * availableContentWidth / totalFactor).ToArray();
                                actualTableWidth = colWidths.Sum();
                                contentMarginLeft = (page.Width - actualTableWidth) / 2;

                                DrawHeader(gfx, page, "REPORTE DE TRANSFERENCIAS DETALLADO", "TRANSFERENCIAS", anio, mes, idFinanciamiento, pageNumber, 1);
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
                                double yOffset = currentY + (adjustedRowHeight - textHeight) / 2; // Calculate Y-offset for vertical centering

                                for (int i = 0; i < lines.Count; i++)
                                {
                                    gfx.DrawString(lines[i], tableContentFont, XBrushes.Black,
                                        new XRect(currentX + cellPadding, (yOffset + i * lineHeight)+4, cellWidth - cellPadding * 2, lineHeight),
                                        XStringFormats.TopLeft);
                                }
                                currentX += cellWidth;
                            }

                            currentY += adjustedRowHeight;
                        }

                        // Dibuja el "Total de artículos del bien" y "Costo Total del Bien"
                        if (currentY + rowHeight * 2 > page.Height - contentMarginBottom) // Considerar espacio para ambos totales
                        {
                            pageNumber++;
                            page = document.AddPage();
                            page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                            page.Size = PdfSharpCore.PageSize.Legal;
                            gfx = XGraphics.FromPdfPage(page);

                            // Recalculate margins and widths for the new page
                            availableContentWidth = page.Width - 80;
                            colWidths = columnHeaders.Select(ch => ch.WidthFactor * availableContentWidth / totalFactor).ToArray();
                            actualTableWidth = colWidths.Sum();
                            contentMarginLeft = (page.Width - actualTableWidth) / 2;

                            DrawHeader(gfx, page, "REPORTE DE TRANSFERENCIAS DETALLADO", "TRANSFERENCIAS", anio, mes, idFinanciamiento, pageNumber, 1);
                            DrawFooter(gfx, page);
                            currentY = contentMarginTop;
                            DrawTableHeaders();
                        }

                        string totalArticulosBien = $"TOTAL DE ARTÍCULOS DEL BIEN: {bienGroup.TotalArticulosBien}";
                        string costoTotalBien = $"COSTO TOTAL DEL BIEN: {bienGroup.CostoTotalBien:C2}";

                        gfx.DrawString(totalArticulosBien, summaryFont, XBrushes.Black,
                            new XRect(contentMarginLeft + 20, currentY + cellPadding, actualTableWidth - 20, rowHeight - cellPadding * 2), XStringFormats.TopLeft);
                        currentY += rowHeight;

                        // Check for page break for the Costo Total del Bien if needed
                        if (currentY + rowHeight > page.Height - contentMarginBottom)
                        {
                            pageNumber++;
                            page = document.AddPage();
                            page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                            page.Size = PdfSharpCore.PageSize.Legal;
                            gfx = XGraphics.FromPdfPage(page);

                            // Recalculate margins and widths for the new page
                            availableContentWidth = page.Width - 80;
                            colWidths = columnHeaders.Select(ch => ch.WidthFactor * availableContentWidth / totalFactor).ToArray();
                            actualTableWidth = colWidths.Sum();
                            contentMarginLeft = (page.Width - actualTableWidth) / 2;

                            DrawHeader(gfx, page, "REPORTE DE TRANSFERENCIAS DETALLADO", "TRANSFERENCIAS", anio, mes, idFinanciamiento, pageNumber, 1);
                            DrawFooter(gfx, page);
                            currentY = contentMarginTop;
                            DrawTableHeaders();
                        }

                        gfx.DrawString(costoTotalBien, summaryFont, XBrushes.Black,
                            new XRect(contentMarginLeft + 20, currentY + cellPadding, actualTableWidth - 20, rowHeight - cellPadding * 2), XStringFormats.TopLeft);
                        currentY += rowHeight + 5;
                    }

                    // Dibuja el "Subtotal por tipo de bien"
                    if (currentY + rowHeight > page.Height - contentMarginBottom)
                    {
                        pageNumber++;
                        page = document.AddPage();
                        page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                        page.Size = PdfSharpCore.PageSize.Legal;
                        gfx = XGraphics.FromPdfPage(page);

                        // Recalculate margins and widths for the new page
                        availableContentWidth = page.Width - 80;
                        colWidths = columnHeaders.Select(ch => ch.WidthFactor * availableContentWidth / totalFactor).ToArray();
                        actualTableWidth = colWidths.Sum();
                        contentMarginLeft = (page.Width - actualTableWidth) / 2;

                        DrawHeader(gfx, page, "REPORTE DE TRANSFERENCIAS DETALLADO", "TRANSFERENCIAS", anio, mes, idFinanciamiento, pageNumber, 1);
                        DrawFooter(gfx, page);
                        currentY = contentMarginTop;
                        DrawTableHeaders();
                    }

                    string subtotalTipoBienText = $"SUBTOTAL POR TIPO DE BIEN: {tipoBienGroup.SubtotalTipoBien:C2}";
                    gfx.DrawString(subtotalTipoBienText, groupHeaderFont, XBrushes.Black,
                        new XRect(contentMarginLeft, currentY + cellPadding, actualTableWidth, rowHeight - cellPadding * 2), XStringFormats.TopLeft);
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
                    gfx = XGraphics.FromPdfPage(page);

                    // Recalculate margins and widths for the new page
                    availableContentWidth = page.Width - 80;
                    colWidths = columnHeaders.Select(ch => ch.WidthFactor * availableContentWidth / totalFactor).ToArray();
                    actualTableWidth = colWidths.Sum();
                    contentMarginLeft = (page.Width - actualTableWidth) / 2;

                    DrawHeader(gfx, page, "REPORTE DE TRANSFERENCIAS DETALLADO", "TRANSFERENCIAS", anio, mes, idFinanciamiento, pageNumber, 1);
                    DrawFooter(gfx, page);
                    currentY = contentMarginTop;
                    DrawTableHeaders();
                }
                string totalGeneralText = $"TOTAL GENERAL EN PESOS: {totalGeneralPesos:C2}";
                gfx.DrawString(totalGeneralText, groupHeaderFont, XBrushes.Black,
                    new XRect(contentMarginLeft, currentY + cellPadding, actualTableWidth, rowHeight - cellPadding * 2), XStringFormats.TopLeft);

                document.Save(ms);
                ms.Position = 0;
                return File(ms.ToArray(), "application/pdf", $"ReporteTransferenciaDesarrollado_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar reporte de transferencias detallado: {ex.Message}");
            }
        }

        // GET: api/Reportes/ReporteTipoBienPDF
        [HttpGet("ReporteTipoBienPDF")]
        public async Task<IActionResult> GenerarReporteTipoBienPDF(
            [FromQuery] int idGeneral,
            [FromQuery] int idPantalla,
            [FromQuery] int idFinanciamiento,
            [FromQuery] int idTipoBien,
            [FromQuery] int idBien,
            [FromQuery] int idArea,
            [FromQuery] bool umas,
            [FromQuery] int unidadResponsable)
        {
            try
            {
                var desglosadoData = new List<(string Adscripcion, string ClaveTipoBien, string NombreTipoBien, string ClaveBien, string BienDesc, double Costo, string NoInventario, string Aviso, string Marca, string Serie, string Modelo, string NumeroFactura, string FolioFiscal, string EdoFisico)>();

                using (var connection = _context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "PA_SEL_ReporteTipoBien";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@IdGeneral", idGeneral));
                        command.Parameters.Add(new SqlParameter("@IdPantalla", idPantalla));
                        command.Parameters.Add(new SqlParameter("@idFinanciamiento", idFinanciamiento));
                        command.Parameters.Add(new SqlParameter("@idTipoBien", idTipoBien));
                        command.Parameters.Add(new SqlParameter("@idBien", idBien));
                        command.Parameters.Add(new SqlParameter("@idArea", idArea));
                        command.Parameters.Add(new SqlParameter("@Umas", umas));
                        command.Parameters.Add(new SqlParameter("@UnidadResponsable", unidadResponsable));

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                desglosadoData.Add((
                                    reader["Adscripcion"]?.ToString(),
                                    reader["Clave"]?.ToString(),
                                    reader["NombreTipoBien"]?.ToString(),
                                    reader["ClaveBien"]?.ToString(),
                                    reader["BienDesc"]?.ToString(),
                                    Convert.ToDouble(reader["Costo"]),
                                    reader["NoInventario"]?.ToString(),
                                    reader["Aviso"]?.ToString(),
                                    reader["Marca"]?.ToString(),
                                    reader["Serie"]?.ToString(),
                                    reader["Modelo"]?.ToString(),
                                    reader["NumeroFactura"]?.ToString(),
                                    reader["FolioFiscal"]?.ToString(),
                                    reader["EdoFisico"]?.ToString()
                                ));
                            }
                        }
                    }
                }

                var groupedData = desglosadoData
                    .GroupBy(item => new { item.ClaveTipoBien, item.NombreTipoBien })
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
                        SubtotalTipoBien = g1.Sum(item => item.Costo),
                        TotalArticulosTipoBien = g1.Count()
                    }).ToList();

                using var ms = new MemoryStream();
                using var document = new PdfDocument();
                document.Info.Title = "Reporte de Bienes por Tipo Detallado";

                PdfPage page = document.AddPage();
                page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                page.Size = PdfSharpCore.PageSize.Legal;

                XGraphics gfx = XGraphics.FromPdfPage(page);

                double contentMarginTop = 150;
                double contentMarginBottom = 70; // Aumentamos para el espacio de firmas
                double currentY = contentMarginTop;

                XFont tableHeaderFont = new XFont("Arial", 8, XFontStyle.Bold);
                XFont tableContentFont = new XFont("Arial", 7, XFontStyle.Regular);
                XFont groupHeaderFont = new XFont("Arial", 9, XFontStyle.Bold);
                XFont summaryFont = new XFont("Arial", 8, XFontStyle.Regular);
                double rowHeightBase = 18;
                double cellPadding = 3;
                double lineHeight = 10;

                XPen solidBorderPen = XPens.Black;
                XPen dottedBottomBorderPen = new XPen(XColor.FromArgb(150, 0, 0, 0), 0.5);
                dottedBottomBorderPen.DashStyle = XDashStyle.Dot;

                var columnHeaders = new (string Name, double WidthFactor)[]
                {
                ("UBICACIÓN", 0.24),
                ("COSTO", 0.05),
                ("NO. INVENT", 0.11),
                ("AVISO", 0.05),
                ("MARCA", 0.05),
                ("SERIE", 0.12),
                ("MODELO", 0.09),
                ("FACTURA", 0.06),
                ("EDO. FÍSICO", 0.05)
                };

                double totalFactor = columnHeaders.Sum(ch => ch.WidthFactor);
                double availableContentWidth = page.Width - 80;
                double[] colWidths = columnHeaders.Select(ch => ch.WidthFactor * availableContentWidth / totalFactor).ToArray();
                double actualTableWidth = colWidths.Sum();
                double contentMarginLeft = (page.Width - actualTableWidth) / 2;

                int pageNumber = 1;
                int totalPages = 1; // Para el cálculo final de páginas, se podría hacer un pre-render o estimación. Por ahora, asumimos 1.

                double totalGeneralPesos = 0;
                int totalGeneralArticulos = 0;

                // Función para dibujar los encabezados de la tabla (las dos filas: "DESCRIPCIÓN TIPO BIEN" y los nombres de columna)
                Action DrawTableContentHeaders = () =>
                {
                    double currentX = contentMarginLeft;

                    // 1. Fila de encabezado agrupado: "DESCRIPCIÓN BIEN"
                    double descripcionBienWidth = colWidths[0] + colWidths[1]; // Agrupa UBICACIÓN + COSTO
                    gfx.DrawRectangle(XPens.Black, currentX, currentY, descripcionBienWidth, rowHeightBase);
                    gfx.DrawString("DESCRIPCIÓN TIPO BIEN", tableHeaderFont, XBrushes.Black,
                        new XRect(currentX, currentY, descripcionBienWidth, rowHeightBase), XStringFormats.Center);

                    currentY += rowHeightBase;

                    // 2. Segunda fila: encabezados normales
                    currentX = contentMarginLeft;
                    for (int i = 0; i < columnHeaders.Length; i++)
                    {
                        gfx.DrawRectangle(XPens.Black, currentX, currentY, colWidths[i], rowHeightBase);
                        gfx.DrawString(columnHeaders[i].Name, tableHeaderFont, XBrushes.Black,
                            new XRect(currentX + cellPadding, currentY + cellPadding, colWidths[i] - cellPadding * 2, rowHeightBase - cellPadding * 2), XStringFormats.Center);
                        currentX += colWidths[i];
                    }
                    currentY += rowHeightBase;
                };

                DrawHeader(gfx, page, "REPORTE DE BIENES POR TIPO DETALLADO", "BIENES POR TIPO", 0, 0, idFinanciamiento, pageNumber, totalPages);
                DrawFooter(gfx, page);
                DrawTableContentHeaders(); // Dibuja los encabezados de la tabla al inicio de la primera página

                string currentTipoBienHeader = "";
                string currentBienHeader = "";

                foreach (var tipoBienGroup in groupedData)
                {
                    // Verifica si el Tipo de Bien actual es diferente al de la página anterior
                    // Si es un nuevo Tipo de Bien, o es la primera vez que se dibuja
                    if (currentTipoBienHeader != $"{tipoBienGroup.TipoBienKey.ClaveTipoBien} {tipoBienGroup.TipoBienKey.NombreTipoBien}")
                    {
                        currentTipoBienHeader = $"{tipoBienGroup.TipoBienKey.ClaveTipoBien} {tipoBienGroup.TipoBienKey.NombreTipoBien}";
                        // Si el espacio es insuficiente para el encabezado del Tipo de Bien y al menos un Bien
                        if (currentY + rowHeightBase * 3 > page.Height - contentMarginBottom)
                        {
                            pageNumber++;
                            page = document.AddPage();
                            page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                            page.Size = PdfSharpCore.PageSize.Legal;
                            gfx = XGraphics.FromPdfPage(page);
                            DrawHeader(gfx, page, "REPORTE DE BIENES POR TIPO DETALLADO", "BIENES POR TIPO", 0, 0, idFinanciamiento, pageNumber, totalPages);
                            DrawFooter(gfx, page);
                            currentY = contentMarginTop;
                            DrawTableContentHeaders(); // Siempre dibuja los encabezados de la tabla en una nueva página
                        }

                        // Dibuja el encabezado del tipo de bien (CAT_TIPOSBIENES)
                        gfx.DrawString(currentTipoBienHeader, groupHeaderFont, XBrushes.Black,
                            new XRect(contentMarginLeft, currentY + cellPadding, actualTableWidth, rowHeightBase), XStringFormats.TopLeft);
                        currentY += rowHeightBase;
                        currentBienHeader = ""; // Resetear el encabezado del bien cuando hay un nuevo tipo de bien
                    }

                    foreach (var bienGroup in tipoBienGroup.TipoBienItems)
                    {
                        // Verifica si el Bien actual es diferente al de la página anterior
                        // Si es un nuevo Bien, o es la primera vez que se dibuja
                        if (currentBienHeader != $"{bienGroup.BienKey.ClaveBien} {bienGroup.BienKey.BienDesc}")
                        {
                            currentBienHeader = $"{bienGroup.BienKey.ClaveBien} {bienGroup.BienKey.BienDesc}";
                            // Si el espacio es insuficiente para el encabezado del Bien y al menos un item
                            if (currentY + rowHeightBase * 3 > page.Height - contentMarginBottom)
                            {
                                pageNumber++;
                                page = document.AddPage();
                                page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                                page.Size = PdfSharpCore.PageSize.Legal;
                                gfx = XGraphics.FromPdfPage(page);
                                DrawHeader(gfx, page, "REPORTE DE BIENES POR TIPO DETALLADO", "BIENES POR TIPO", 0, 0, idFinanciamiento, pageNumber, totalPages);
                                DrawFooter(gfx, page);
                                currentY = contentMarginTop;
                                DrawTableContentHeaders(); // Siempre dibuja los encabezados de la tabla en una nueva página

                                // También redibuja el encabezado del Tipo de Bien si cambiamos de página
                                gfx.DrawString(currentTipoBienHeader, groupHeaderFont, XBrushes.Black,
                                    new XRect(contentMarginLeft, currentY + cellPadding, actualTableWidth, rowHeightBase), XStringFormats.TopLeft);
                                currentY += rowHeightBase;
                            }

                            // Dibuja el encabezado del bien (CAT_BIENES)
                            gfx.DrawString(currentBienHeader, summaryFont, XBrushes.Black,
                                new XRect(contentMarginLeft + 10, currentY + cellPadding, actualTableWidth - 10, rowHeightBase), XStringFormats.TopLeft);
                            currentY += rowHeightBase;

                            gfx.DrawLine(solidBorderPen, contentMarginLeft, currentY, contentMarginLeft + actualTableWidth, currentY);
                        }

                        foreach (var item in bienGroup.BienItems)
                        {
                            string[] textos = {
                            item.Adscripcion,
                            $"{item.Costo:C2}",
                            item.NoInventario,
                            item.Aviso,
                            item.Marca,
                            item.Serie,
                            item.Modelo,
                            item.NumeroFactura,
                            item.EdoFisico
                        };

                            double maxLineCount = 1;
                            List<List<string>> wrappedTexts = new List<List<string>>();

                            for (int i = 0; i < textos.Length; i++)
                            {
                                string text = textos[i] ?? string.Empty;
                                double cellWidth = colWidths[i];
                                int aproxCharPerLine = (int)(cellWidth / tableContentFont.Size * (1.0 / 0.6));
                                if (aproxCharPerLine <= 0) aproxCharPerLine = 1;

                                var lines = wrapText(text, aproxCharPerLine);
                                wrappedTexts.Add(lines);
                                maxLineCount = Math.Max(maxLineCount, lines.Count);
                            }

                            double adjustedRowHeight = maxLineCount * lineHeight + (cellPadding * 2);

                            if (currentY + adjustedRowHeight > page.Height - contentMarginBottom)
                            {
                                pageNumber++;
                                page = document.AddPage();
                                page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                                page.Size = PdfSharpCore.PageSize.Legal;
                                gfx = XGraphics.FromPdfPage(page);
                                DrawHeader(gfx, page, "REPORTE DE BIENES POR TIPO DETALLADO", "BIENES POR TIPO", 0, 0, idFinanciamiento, pageNumber, totalPages);
                                DrawFooter(gfx, page);
                                currentY = contentMarginTop;
                                DrawTableContentHeaders(); // Siempre dibuja los encabezados de la tabla en una nueva página
                            }

                            double currentX = contentMarginLeft;
                            for (int colIndex = 0; colIndex < wrappedTexts.Count; colIndex++)
                            {
                                var lines = wrappedTexts[colIndex];
                                double cellWidth = colWidths[colIndex];
                                double textHeight = lines.Count * lineHeight;
                                double yOffset = currentY + (adjustedRowHeight - textHeight) / 2 + 4;

                                for (int i = 0; i < lines.Count; i++)
                                {
                                    XStringFormat stringFormat = XStringFormats.TopLeft;
                                    if (columnHeaders[colIndex].Name == "COSTO")
                                    {
                                        stringFormat = XStringFormats.TopRight;
                                    }
                                    else if (columnHeaders[colIndex].Name == "NO. INVENT" || columnHeaders[colIndex].Name == "FACTURA" || columnHeaders[colIndex].Name == "MARCA")
                                    {
                                        stringFormat = XStringFormats.TopCenter;
                                    }

                                    gfx.DrawString(lines[i], tableContentFont, XBrushes.Black,
                                        new XRect(currentX + cellPadding, yOffset + i * lineHeight, cellWidth - cellPadding * 2, lineHeight),
                                        stringFormat);
                                }
                                currentX += cellWidth;
                            }
                            if (item != bienGroup.BienItems.Last())
                            {
                                gfx.DrawLine(dottedBottomBorderPen, contentMarginLeft, currentY + adjustedRowHeight, contentMarginLeft + actualTableWidth, currentY + adjustedRowHeight);
                            }
                            currentY += adjustedRowHeight;
                        }

                        string totalArticulosBien = $"TOTAL DE ARTÍCULOS DEL BIEN: {bienGroup.TotalArticulosBien}";
                        string costoTotalBien = $"COSTO TOTAL DEL BIEN: {bienGroup.CostoTotalBien:C2}";

                        if (currentY + rowHeightBase * 2 > page.Height - contentMarginBottom)
                        {
                            pageNumber++;
                            page = document.AddPage();
                            page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                            page.Size = PdfSharpCore.PageSize.Legal;
                            gfx = XGraphics.FromPdfPage(page);
                            DrawHeader(gfx, page, "REPORTE DE BIENES POR TIPO DETALLADO", "BIENES POR TIPO", 0, 0, idFinanciamiento, pageNumber, totalPages);
                            DrawFooter(gfx, page);
                            currentY = contentMarginTop;
                            DrawTableContentHeaders(); // Siempre dibuja los encabezados de la tabla en una nueva página

                            // También redibuja el encabezado del Tipo de Bien si cambiamos de página
                            gfx.DrawString(currentTipoBienHeader, groupHeaderFont, XBrushes.Black,
                                new XRect(contentMarginLeft, currentY + cellPadding, actualTableWidth, rowHeightBase), XStringFormats.TopLeft);
                            currentY += rowHeightBase;
                        }

                        gfx.DrawString(totalArticulosBien, summaryFont, XBrushes.Black,
                            new XRect(contentMarginLeft + 20, currentY + cellPadding, actualTableWidth - 20, rowHeightBase - cellPadding * 2), XStringFormats.TopLeft);
                        currentY += rowHeightBase;

                        if (currentY + rowHeightBase > page.Height - contentMarginBottom)
                        {
                            pageNumber++;
                            page = document.AddPage();
                            page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                            page.Size = PdfSharpCore.PageSize.Legal;
                            gfx = XGraphics.FromPdfPage(page);
                            DrawHeader(gfx, page, "REPORTE DE BIENES POR TIPO DETALLADO", "BIENES POR TIPO", 0, 0, idFinanciamiento, pageNumber, totalPages);
                            DrawFooter(gfx, page);
                            currentY = contentMarginTop;
                            DrawTableContentHeaders(); // Siempre dibuja los encabezados de la tabla en una nueva página

                            // También redibuja el encabezado del Tipo de Bien si cambiamos de página
                            gfx.DrawString(currentTipoBienHeader, groupHeaderFont, XBrushes.Black,
                                new XRect(contentMarginLeft, currentY + cellPadding, actualTableWidth, rowHeightBase), XStringFormats.TopLeft);
                            currentY += rowHeightBase;
                        }

                        gfx.DrawString(costoTotalBien, summaryFont, XBrushes.Black,
                            new XRect(contentMarginLeft + 20, currentY + cellPadding, actualTableWidth - 20, rowHeightBase - cellPadding * 2), XStringFormats.TopLeft);
                        currentY += rowHeightBase + 5;
                    }

                    if (currentY + rowHeightBase > page.Height - contentMarginBottom)
                    {
                        pageNumber++;
                        page = document.AddPage();
                        page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                        page.Size = PdfSharpCore.PageSize.Legal;
                        gfx = XGraphics.FromPdfPage(page);
                        DrawHeader(gfx, page, "REPORTE DE BIENES POR TIPO DETALLADO", "BIENES POR TIPO", 0, 0, idFinanciamiento, pageNumber, totalPages);
                        DrawFooter(gfx, page);
                        currentY = contentMarginTop;
                        DrawTableContentHeaders(); // Siempre dibuja los encabezados de la tabla en una nueva página
                    }

                    string subtotalTipoBienText = $"SUBTOTAL POR TIPO DE BIEN: {tipoBienGroup.SubtotalTipoBien:C2}";
                    gfx.DrawString(subtotalTipoBienText, groupHeaderFont, XBrushes.Black,
                        new XRect(contentMarginLeft, currentY + cellPadding, actualTableWidth, rowHeightBase - cellPadding * 2), XStringFormats.TopLeft);
                    currentY += rowHeightBase + 10;

                    totalGeneralPesos += tipoBienGroup.SubtotalTipoBien;
                    totalGeneralArticulos += tipoBienGroup.TotalArticulosTipoBien;
                }

                // TOTAL GENERAL
                if (currentY + rowHeightBase * 2 > page.Height - contentMarginBottom) // Verifica si hay espacio para el total general y las firmas
                {
                    pageNumber++;
                    page = document.AddPage();
                    page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                    page.Size = PdfSharpCore.PageSize.Legal;
                    gfx = XGraphics.FromPdfPage(page);
                    DrawHeader(gfx, page, "REPORTE DE BIENES POR TIPO DETALLADO", "BIENES POR TIPO", 0, 0, idFinanciamiento, pageNumber, totalPages);
                    DrawFooter(gfx, page);
                    currentY = contentMarginTop; // O ajusta según donde quieras que aparezca en la nueva página
                }

                string totalGeneralText = $"TOTAL GENERAL EN PESOS: {totalGeneralPesos:C2}           CANTIDAD TOTAL DE BIENES: {totalGeneralArticulos}";
                gfx.DrawString(totalGeneralText, groupHeaderFont, XBrushes.Black,
                    new XRect(contentMarginLeft, currentY + cellPadding, actualTableWidth, rowHeightBase - cellPadding * 2), XStringFormats.TopLeft);
                currentY += rowHeightBase + 10; // Espacio después de los totales generales

                // Dibuja las firmas al final del documento
                DrawSignatures(gfx, page, currentY);

                document.Save(ms);
                ms.Position = 0;
                return File(ms.ToArray(), "application/pdf", $"ReporteTipoBien_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar reporte de bienes por tipo detallado: {ex.Message}");
            }
        }

        List<string> wrapText(string text, int maxCharsPerLine)
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

            if (tipoReporte != "BIENES POR TIPO")
            {
                gfx.DrawString($"CONCENTRADO DE {tipoReporte} CORRESPONDIENTE A: {fechaReporte}", fontBold, XBrushes.Black, new XRect(0, y + 60, page.Width, fontSubTitle.Height), XStringFormats.TopCenter);
            }
            else {
                gfx.DrawString($"REPORTE CONSECUTIVO POR TIPO Y NOMBRE DE BIEN", fontBold, XBrushes.Black, new XRect(0, y + 60, page.Width, fontSubTitle.Height), XStringFormats.TopCenter);
            }

                string financiamientoText = idFinanciamiento == 0 ? "TODOS" : idFinanciamiento.ToString();
            gfx.DrawString($"FUENTE DE FINANCIAMIENTO: {financiamientoText}", fontBold, XBrushes.Black, new XRect(0, y + 75, page.Width, fontSubTitle.Height), XStringFormats.TopCenter);

            gfx.DrawString($"Pág. {currentPage} de {totalPages}", fontBold, XBrushes.Black, new XPoint(page.Width - x - 70, y + 83)); // Paginación (esquina superior derecha)

            gfx.DrawLine(XPens.Black, x, y + 100, page.Width - x, y + 100); // Dibuja una línea después del encabezado
        }

        private void DrawSignatures(XGraphics gfx, PdfPage page, double currentY)
        {
            XFont signatureLabelFont = new XFont("Arial", 8, XFontStyle.Regular);
            XFont signatureNameFont = new XFont("Arial", 9, XFontStyle.Bold);
            XPen linePen = XPens.Black;

            double signatureBlockWidth = page.Width / 3.5; // Ancho para cada bloque de firma
            double spacing = 20; // Espacio entre bloques
            double startX = (page.Width - (signatureBlockWidth * 3 + spacing * 2)) / 2; // Centrar los bloques

            currentY += 20; // Espacio después de los totales

            // ELABORÓ
            XRect elaboradoRect = new XRect(startX, currentY, signatureBlockWidth, 15);
            gfx.DrawString("ELABORÓ", signatureNameFont, XBrushes.Black, elaboradoRect, XStringFormats.Center);
            currentY += 70; // Espacio para la línea y el nombre
            gfx.DrawLine(linePen, startX, currentY, startX + signatureBlockWidth, currentY);
            currentY += 5; // Espacio entre línea y texto
            XRect deptoElaboroRect = new XRect(startX, currentY, signatureBlockWidth, 30);
            gfx.DrawString("DEPTO. DE CONTROL PATRIMONIAL Y BIENES EN\nCUSTODIA", signatureLabelFont, XBrushes.Black, deptoElaboroRect, XStringFormats.Center);

            // Restablecer currentY para el siguiente bloque
            currentY -= 75; // Vuelve a la altura inicial del bloque

            // DICTAMEN
            startX += signatureBlockWidth + spacing;
            XRect dictamenRect = new XRect(startX, currentY, signatureBlockWidth, 15);
            gfx.DrawString("DICTAMEN", signatureNameFont, XBrushes.Black, dictamenRect, XStringFormats.Center);
            currentY += 70;
            gfx.DrawLine(linePen, startX, currentY, startX + signatureBlockWidth, currentY);
            currentY += 5;
            XRect deptoDictamenRect = new XRect(startX, currentY, signatureBlockWidth, 30);
            gfx.DrawString("DEPTO. DE SERVICIOS GENERALES", signatureLabelFont, XBrushes.Black, deptoDictamenRect, XStringFormats.Center);

            // Restablecer currentY para el siguiente bloque
            currentY -= 75;

            // AUTORIZÓ
            startX += signatureBlockWidth + spacing;
            XRect autorizoRect = new XRect(startX, currentY, signatureBlockWidth, 15);
            gfx.DrawString("AUTORIZÓ", signatureNameFont, XBrushes.Black, autorizoRect, XStringFormats.Center);
            currentY += 70;
            gfx.DrawLine(linePen, startX, currentY, startX + signatureBlockWidth, currentY);
            currentY += 5;
            XRect direccionAutorizoRect = new XRect(startX, currentY, signatureBlockWidth, 30);
            gfx.DrawString("DIRECCIÓN DE ADMINISTRACIÓN", signatureLabelFont, XBrushes.Black, direccionAutorizoRect, XStringFormats.Center);
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
