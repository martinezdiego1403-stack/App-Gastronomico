using System;
using System.Linq;
using ClosedXML.Excel;
using SandwicheriaWalterio.Data;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Services
{
    /// <summary>
    /// Servicio para generar reportes Excel de ventas.
    /// Genera una tabla con las ventas del período, detalle de productos,
    /// información del empleado y campo para pago.
    /// </summary>
    public class ExcelReportService
    {
        private readonly VentaRepository _ventaRepository;
        private readonly CajaRepository _cajaRepository;

        public ExcelReportService()
        {
            _ventaRepository = new VentaRepository();
            _cajaRepository = new CajaRepository();
        }

        /// <summary>
        /// Genera un reporte Excel con la lista de ventas del período.
        /// </summary>
        public string GenerarReporteCompleto(DateTime fechaInicio, DateTime fechaFin, string rutaArchivo)
        {
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Ventas");

                // ===== ENCABEZADO =====
                ws.Cell("A1").Value = "SANDWICHERÍA WALTERIO - REPORTE DE VENTAS";
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A1").Style.Font.FontSize = 16;
                ws.Cell("A1").Style.Font.FontColor = XLColor.FromHtml("#2C3E50");
                ws.Range("A1:G1").Merge();

                ws.Cell("A2").Value = $"Período: {fechaInicio:dd/MM/yyyy} al {fechaFin:dd/MM/yyyy}";
                ws.Cell("A2").Style.Font.Italic = true;
                ws.Cell("A2").Style.Font.FontColor = XLColor.Gray;
                ws.Range("A2:G2").Merge();

                ws.Cell("A3").Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
                ws.Cell("A3").Style.Font.Italic = true;
                ws.Cell("A3").Style.Font.FontColor = XLColor.Gray;
                ws.Range("A3:G3").Merge();

                // ===== ENCABEZADOS DE TABLA =====
                int fila = 5;
                ws.Cell($"A{fila}").Value = "#";
                ws.Cell($"B{fila}").Value = "FECHA";
                ws.Cell($"C{fila}").Value = "HORA";
                ws.Cell($"D{fila}").Value = "PRODUCTOS";
                ws.Cell($"E{fila}").Value = "MÉTODO PAGO";
                ws.Cell($"F{fila}").Value = "VENDEDOR";
                ws.Cell($"G{fila}").Value = "MONTO";

                // Estilo de encabezados
                var headerRange = ws.Range($"A{fila}:G{fila}");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2C3E50");
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // ===== OBTENER VENTAS =====
                var fechaFinAjustada = fechaFin.Date.AddDays(1).AddSeconds(-1);
                var ventas = _ventaRepository.ObtenerPorRangoFechas(fechaInicio, fechaFinAjustada)
                    .OrderBy(v => v.FechaVenta)
                    .ToList();

                // ===== LLENAR DATOS =====
                fila++;
                int numero = 1;
                decimal totalGeneral = 0;

                foreach (var venta in ventas)
                {
                    ws.Cell($"A{fila}").Value = numero;
                    ws.Cell($"A{fila}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell($"B{fila}").Value = venta.FechaVenta.ToString("dd/MM/yyyy");
                    ws.Cell($"B{fila}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell($"C{fila}").Value = venta.FechaVenta.ToString("HH:mm");
                    ws.Cell($"C{fila}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Detalle de productos vendidos
                    string productos = ObtenerDetalleProductos(venta);
                    ws.Cell($"D{fila}").Value = productos;
                    ws.Cell($"D{fila}").Style.Alignment.WrapText = true;

                    ws.Cell($"E{fila}").Value = venta.MetodoPago;
                    ws.Cell($"E{fila}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Nombre del vendedor
                    string vendedor = venta.Usuario?.NombreCompleto ?? venta.Usuario?.NombreUsuario ?? "N/A";
                    ws.Cell($"F{fila}").Value = vendedor;
                    ws.Cell($"F{fila}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell($"G{fila}").Value = venta.Total;
                    ws.Cell($"G{fila}").Style.NumberFormat.Format = "$#,##0";
                    ws.Cell($"G{fila}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    // Alternar colores de fila
                    if (numero % 2 == 0)
                    {
                        ws.Range($"A{fila}:G{fila}").Style.Fill.BackgroundColor = XLColor.FromHtml("#F8F9FA");
                    }

                    totalGeneral += venta.Total;
                    numero++;
                    fila++;
                }

                // ===== FILA DE TOTAL =====
                ws.Cell($"F{fila}").Value = "TOTAL:";
                ws.Cell($"F{fila}").Style.Font.Bold = true;
                ws.Cell($"F{fila}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                
                ws.Cell($"G{fila}").Value = totalGeneral;
                ws.Cell($"G{fila}").Style.NumberFormat.Format = "$#,##0";
                ws.Cell($"G{fila}").Style.Font.Bold = true;
                ws.Cell($"G{fila}").Style.Font.FontColor = XLColor.FromHtml("#27AE60");
                ws.Cell($"G{fila}").Style.Font.FontSize = 14;
                ws.Cell($"G{fila}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                var totalRange = ws.Range($"A{fila}:G{fila}");
                totalRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#ECF0F1");
                totalRange.Style.Border.TopBorder = XLBorderStyleValues.Double;

                // ===== RESUMEN =====
                fila += 2;
                ws.Cell($"A{fila}").Value = $"Total de ventas: {ventas.Count}";
                ws.Cell($"A{fila}").Style.Font.Bold = true;

                // ===== SECCIÓN DE PAGO AL EMPLEADO =====
                fila += 3;
                ws.Cell($"A{fila}").Value = "PAGO AL EMPLEADO";
                ws.Cell($"A{fila}").Style.Font.Bold = true;
                ws.Cell($"A{fila}").Style.Font.FontSize = 14;
                ws.Cell($"A{fila}").Style.Font.FontColor = XLColor.FromHtml("#2C3E50");
                ws.Range($"A{fila}:G{fila}").Merge();
                ws.Range($"A{fila}:G{fila}").Style.Border.BottomBorder = XLBorderStyleValues.Medium;

                fila++;
                ws.Cell($"A{fila}").Value = "Empleado:";
                ws.Cell($"A{fila}").Style.Font.Bold = true;
                ws.Cell($"B{fila}").Value = "";  // Celda vacía para escribir nombre
                ws.Cell($"B{fila}").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Range($"B{fila}:C{fila}").Merge();

                fila++;
                ws.Cell($"A{fila}").Value = "Monto a pagar:";
                ws.Cell($"A{fila}").Style.Font.Bold = true;
                ws.Cell($"B{fila}").Value = "";  // Celda vacía para escribir monto
                ws.Cell($"B{fila}").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell($"B{fila}").Style.NumberFormat.Format = "$#,##0";
                ws.Range($"B{fila}:C{fila}").Merge();

                fila++;
                ws.Cell($"A{fila}").Value = "Fecha de pago:";
                ws.Cell($"A{fila}").Style.Font.Bold = true;
                ws.Cell($"B{fila}").Value = "";  // Celda vacía para escribir fecha
                ws.Cell($"B{fila}").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Range($"B{fila}:C{fila}").Merge();

                fila++;
                ws.Cell($"A{fila}").Value = "Firma:";
                ws.Cell($"A{fila}").Style.Font.Bold = true;
                ws.Cell($"B{fila}").Value = "";  // Celda vacía para firma
                ws.Cell($"B{fila}").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Range($"B{fila}:D{fila}").Merge();

                // ===== BORDES Y AJUSTES =====
                if (ventas.Count > 0)
                {
                    var dataRange = ws.Range($"A5:G{5 + ventas.Count}");
                    dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                    dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                }

                // Ajustar anchos de columna
                ws.Column("A").Width = 8;
                ws.Column("B").Width = 12;
                ws.Column("C").Width = 8;
                ws.Column("D").Width = 35;
                ws.Column("E").Width = 15;
                ws.Column("F").Width = 18;
                ws.Column("G").Width = 12;

                // Guardar
                workbook.SaveAs(rutaArchivo);
            }

            return rutaArchivo;
        }

        /// <summary>
        /// Genera un reporte Excel de cierre de caja con todos los detalles.
        /// </summary>
        public string GenerarReporteCierreCaja(Caja caja, string rutaArchivo)
        {
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Cierre de Caja");

                // ===== ENCABEZADO =====
                ws.Cell("A1").Value = "SANDWICHERÍA WALTERIO - CIERRE DE CAJA";
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A1").Style.Font.FontSize = 16;
                ws.Cell("A1").Style.Font.FontColor = XLColor.FromHtml("#2C3E50");
                ws.Range("A1:G1").Merge();

                ws.Cell("A2").Value = $"Caja #{caja.CajaID}";
                ws.Cell("A2").Style.Font.Bold = true;
                ws.Range("A2:G2").Merge();

                // ===== INFORMACIÓN DE LA CAJA =====
                int fila = 4;
                
                // Empleado que abrió la caja
                ws.Cell($"A{fila}").Value = "Empleado que abrió caja:";
                ws.Cell($"A{fila}").Style.Font.Bold = true;
                ws.Cell($"C{fila}").Value = caja.UsuarioApertura?.NombreCompleto ?? caja.UsuarioApertura?.NombreUsuario ?? "N/A";
                ws.Range($"A{fila}:B{fila}").Merge();
                ws.Range($"C{fila}:D{fila}").Merge();

                fila++;
                ws.Cell($"A{fila}").Value = "Fecha/Hora Apertura:";
                ws.Cell($"A{fila}").Style.Font.Bold = true;
                ws.Cell($"C{fila}").Value = caja.FechaApertura.ToString("dd/MM/yyyy HH:mm");
                ws.Range($"A{fila}:B{fila}").Merge();
                ws.Range($"C{fila}:D{fila}").Merge();

                fila++;
                ws.Cell($"A{fila}").Value = "Fecha/Hora Cierre:";
                ws.Cell($"A{fila}").Style.Font.Bold = true;
                ws.Cell($"C{fila}").Value = caja.FechaCierre?.ToString("dd/MM/yyyy HH:mm") ?? "En curso";
                ws.Range($"A{fila}:B{fila}").Merge();
                ws.Range($"C{fila}:D{fila}").Merge();

                fila++;
                ws.Cell($"A{fila}").Value = "Monto Inicial:";
                ws.Cell($"A{fila}").Style.Font.Bold = true;
                ws.Cell($"C{fila}").Value = caja.MontoInicial;
                ws.Cell($"C{fila}").Style.NumberFormat.Format = "$#,##0";
                ws.Range($"A{fila}:B{fila}").Merge();

                fila++;
                ws.Cell($"A{fila}").Value = "Total Ventas:";
                ws.Cell($"A{fila}").Style.Font.Bold = true;
                ws.Cell($"C{fila}").Value = caja.TotalVentas ?? 0;
                ws.Cell($"C{fila}").Style.NumberFormat.Format = "$#,##0";
                ws.Cell($"C{fila}").Style.Font.FontColor = XLColor.FromHtml("#27AE60");
                ws.Range($"A{fila}:B{fila}").Merge();

                fila++;
                ws.Cell($"A{fila}").Value = "Monto Esperado:";
                ws.Cell($"A{fila}").Style.Font.Bold = true;
                ws.Cell($"C{fila}").Value = caja.MontoEsperado;
                ws.Cell($"C{fila}").Style.NumberFormat.Format = "$#,##0";
                ws.Range($"A{fila}:B{fila}").Merge();

                fila++;
                ws.Cell($"A{fila}").Value = "Monto Cierre:";
                ws.Cell($"A{fila}").Style.Font.Bold = true;
                ws.Cell($"C{fila}").Value = caja.MontoCierre ?? 0;
                ws.Cell($"C{fila}").Style.NumberFormat.Format = "$#,##0";
                ws.Range($"A{fila}:B{fila}").Merge();

                fila++;
                ws.Cell($"A{fila}").Value = "Diferencia:";
                ws.Cell($"A{fila}").Style.Font.Bold = true;
                decimal diferencia = caja.DiferenciaEsperado ?? 0;
                ws.Cell($"C{fila}").Value = diferencia;
                ws.Cell($"C{fila}").Style.NumberFormat.Format = "$#,##0";
                ws.Cell($"C{fila}").Style.Font.FontColor = diferencia >= 0 ? XLColor.FromHtml("#27AE60") : XLColor.FromHtml("#E74C3C");
                ws.Cell($"C{fila}").Style.Font.Bold = true;
                ws.Range($"A{fila}:B{fila}").Merge();

                // ===== DETALLE DE VENTAS =====
                fila += 2;
                ws.Cell($"A{fila}").Value = "DETALLE DE VENTAS";
                ws.Cell($"A{fila}").Style.Font.Bold = true;
                ws.Cell($"A{fila}").Style.Font.FontSize = 14;
                ws.Cell($"A{fila}").Style.Font.FontColor = XLColor.FromHtml("#2C3E50");
                ws.Range($"A{fila}:G{fila}").Merge();
                ws.Range($"A{fila}:G{fila}").Style.Border.BottomBorder = XLBorderStyleValues.Medium;

                fila++;
                // Encabezados de tabla
                ws.Cell($"A{fila}").Value = "#";
                ws.Cell($"B{fila}").Value = "HORA";
                ws.Cell($"C{fila}").Value = "PRODUCTOS";
                ws.Cell($"D{fila}").Value = "CANT.";
                ws.Cell($"E{fila}").Value = "MÉTODO";
                ws.Cell($"F{fila}").Value = "VENDEDOR";
                ws.Cell($"G{fila}").Value = "MONTO";

                var headerRange = ws.Range($"A{fila}:G{fila}");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2C3E50");
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Obtener ventas de la caja
                var ventas = _ventaRepository.ObtenerPorCaja(caja.CajaID);
                
                fila++;
                int numero = 1;
                int filaInicioData = fila;

                foreach (var venta in ventas.OrderBy(v => v.FechaVenta))
                {
                    ws.Cell($"A{fila}").Value = numero;
                    ws.Cell($"A{fila}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell($"B{fila}").Value = venta.FechaVenta.ToString("HH:mm");
                    ws.Cell($"B{fila}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Productos vendidos
                    string productos = ObtenerDetalleProductos(venta);
                    ws.Cell($"C{fila}").Value = productos;
                    ws.Cell($"C{fila}").Style.Alignment.WrapText = true;

                    // Cantidad total de items
                    int cantidadItems = venta.Detalles?.Sum(d => d.Cantidad) ?? 0;
                    ws.Cell($"D{fila}").Value = cantidadItems;
                    ws.Cell($"D{fila}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell($"E{fila}").Value = venta.MetodoPago;
                    ws.Cell($"E{fila}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Vendedor
                    string vendedor = venta.Usuario?.NombreCompleto ?? venta.Usuario?.NombreUsuario ?? "N/A";
                    ws.Cell($"F{fila}").Value = vendedor;
                    ws.Cell($"F{fila}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell($"G{fila}").Value = venta.Total;
                    ws.Cell($"G{fila}").Style.NumberFormat.Format = "$#,##0";
                    ws.Cell($"G{fila}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    // Alternar colores
                    if (numero % 2 == 0)
                    {
                        ws.Range($"A{fila}:G{fila}").Style.Fill.BackgroundColor = XLColor.FromHtml("#F8F9FA");
                    }

                    numero++;
                    fila++;
                }

                // Bordes de la tabla de ventas
                if (ventas.Any())
                {
                    var dataRange = ws.Range($"A{filaInicioData - 1}:G{fila - 1}");
                    dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                    dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                }

                // ===== RESUMEN POR MÉTODO DE PAGO =====
                fila += 2;
                ws.Cell($"A{fila}").Value = "RESUMEN POR MÉTODO DE PAGO";
                ws.Cell($"A{fila}").Style.Font.Bold = true;
                ws.Cell($"A{fila}").Style.Font.FontSize = 12;
                ws.Range($"A{fila}:C{fila}").Merge();

                var resumenPorMetodo = _cajaRepository.ObtenerResumenVentasPorMetodoPago(caja.CajaID);
                foreach (var metodo in resumenPorMetodo)
                {
                    fila++;
                    ws.Cell($"A{fila}").Value = $"  {metodo.Key}:";
                    ws.Cell($"B{fila}").Value = metodo.Value;
                    ws.Cell($"B{fila}").Style.NumberFormat.Format = "$#,##0";
                }

                // ===== SECCIÓN DE PAGO AL EMPLEADO =====
                fila += 3;
                ws.Cell($"A{fila}").Value = "PAGO AL EMPLEADO";
                ws.Cell($"A{fila}").Style.Font.Bold = true;
                ws.Cell($"A{fila}").Style.Font.FontSize = 14;
                ws.Cell($"A{fila}").Style.Font.FontColor = XLColor.FromHtml("#2C3E50");
                ws.Range($"A{fila}:G{fila}").Merge();
                ws.Range($"A{fila}:G{fila}").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                ws.Range($"A{fila}:G{fila}").Style.Fill.BackgroundColor = XLColor.FromHtml("#FEF9E7");

                fila++;
                ws.Cell($"A{fila}").Value = "Empleado:";
                ws.Cell($"A{fila}").Style.Font.Bold = true;
                ws.Cell($"B{fila}").Value = caja.UsuarioApertura?.NombreCompleto ?? "";
                ws.Cell($"B{fila}").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Range($"B{fila}:D{fila}").Merge();

                fila++;
                ws.Cell($"A{fila}").Value = "Monto a pagar:";
                ws.Cell($"A{fila}").Style.Font.Bold = true;
                ws.Cell($"B{fila}").Value = "";  // CELDA VACÍA PARA ESCRIBIR MONTO
                ws.Cell($"B{fila}").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell($"B{fila}").Style.Border.BottomBorderColor = XLColor.FromHtml("#E67E22");
                ws.Cell($"B{fila}").Style.Fill.BackgroundColor = XLColor.FromHtml("#FDEBD0");
                ws.Range($"B{fila}:C{fila}").Merge();

                fila++;
                ws.Cell($"A{fila}").Value = "Fecha de pago:";
                ws.Cell($"A{fila}").Style.Font.Bold = true;
                ws.Cell($"B{fila}").Value = "";  // Celda vacía para fecha
                ws.Cell($"B{fila}").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Range($"B{fila}:C{fila}").Merge();

                fila++;
                ws.Cell($"A{fila}").Value = "Observaciones:";
                ws.Cell($"A{fila}").Style.Font.Bold = true;
                ws.Cell($"B{fila}").Value = caja.Observaciones ?? "";
                ws.Cell($"B{fila}").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Range($"B{fila}:F{fila}").Merge();

                fila++;
                ws.Cell($"A{fila}").Value = "Firma empleado:";
                ws.Cell($"A{fila}").Style.Font.Bold = true;
                ws.Cell($"B{fila}").Value = "";
                ws.Cell($"B{fila}").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Range($"B{fila}:D{fila}").Merge();

                ws.Cell($"E{fila}").Value = "Firma dueño:";
                ws.Cell($"E{fila}").Style.Font.Bold = true;
                ws.Cell($"F{fila}").Value = "";
                ws.Cell($"F{fila}").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Range($"F{fila}:G{fila}").Merge();

                // Ajustar anchos de columna
                ws.Column("A").Width = 18;
                ws.Column("B").Width = 10;
                ws.Column("C").Width = 35;
                ws.Column("D").Width = 8;
                ws.Column("E").Width = 14;
                ws.Column("F").Width = 18;
                ws.Column("G").Width = 12;

                // Guardar
                workbook.SaveAs(rutaArchivo);
            }

            return rutaArchivo;
        }

        /// <summary>
        /// Obtiene el detalle de productos de una venta como texto.
        /// </summary>
        private string ObtenerDetalleProductos(Venta venta)
        {
            if (venta.Detalles == null || !venta.Detalles.Any())
            {
                return "Sin detalle";
            }

            var productos = venta.Detalles
                .Select(d => $"{d.Cantidad}x {d.ProductoNombre}")
                .ToList();

            return string.Join(", ", productos);
        }
    }
}
