using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Services
{
    /// <summary>
    /// Servicio para generar e imprimir tickets de venta
    /// Optimizado para impresora térmica Gadnic 58mm con comandos ESC/POS
    /// </summary>
    public class TicketService
    {
        private static TicketService _instance;
        private static readonly object _lock = new object();

        // Configuración para papel de 58mm (32 caracteres por línea)
        private const int ANCHO_PAPEL = 32;
        
        // Nombre de la impresora térmica (dejar vacío para usar la predeterminada)
        public string NombreImpresora { get; set; } = "";

        // Comandos ESC/POS para impresoras térmicas
        private static class ESC_POS
        {
            // Inicialización
            public static readonly byte[] INIT = { 0x1B, 0x40 };  // ESC @
            
            // Alineación
            public static readonly byte[] ALIGN_LEFT = { 0x1B, 0x61, 0x00 };    // ESC a 0
            public static readonly byte[] ALIGN_CENTER = { 0x1B, 0x61, 0x01 };  // ESC a 1
            public static readonly byte[] ALIGN_RIGHT = { 0x1B, 0x61, 0x02 };   // ESC a 2
            
            // Formato de texto
            public static readonly byte[] BOLD_ON = { 0x1B, 0x45, 0x01 };       // ESC E 1
            public static readonly byte[] BOLD_OFF = { 0x1B, 0x45, 0x00 };      // ESC E 0
            public static readonly byte[] DOUBLE_HEIGHT_ON = { 0x1B, 0x21, 0x10 };  // Texto doble altura
            public static readonly byte[] DOUBLE_WIDTH_ON = { 0x1B, 0x21, 0x20 };   // Texto doble ancho
            public static readonly byte[] DOUBLE_SIZE_ON = { 0x1B, 0x21, 0x30 };    // Texto doble tamaño
            public static readonly byte[] NORMAL_SIZE = { 0x1B, 0x21, 0x00 };       // Texto normal
            
            // Tamaño de fuente (GS ! n)
            public static readonly byte[] FONT_SMALL = { 0x1D, 0x21, 0x00 };    // Normal
            public static readonly byte[] FONT_MEDIUM = { 0x1D, 0x21, 0x01 };   // Doble altura
            public static readonly byte[] FONT_LARGE = { 0x1D, 0x21, 0x11 };    // Doble ancho y altura
            
            // Corte de papel
            public static readonly byte[] CUT_PARTIAL = { 0x1D, 0x56, 0x01 };   // GS V 1 - Corte parcial
            public static readonly byte[] CUT_FULL = { 0x1D, 0x56, 0x00 };      // GS V 0 - Corte total
            public static readonly byte[] CUT_FEED = { 0x1D, 0x56, 0x42, 0x03 }; // GS V B 3 - Avanza y corta
            
            // Avance de línea
            public static readonly byte[] LINE_FEED = { 0x0A };                  // LF
            public static readonly byte[] FEED_3_LINES = { 0x1B, 0x64, 0x03 };  // ESC d 3
            public static readonly byte[] FEED_5_LINES = { 0x1B, 0x64, 0x05 };  // ESC d 5
        }

        public static TicketService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new TicketService();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Genera un ticket de venta en formato texto (para archivo o vista previa)
        /// </summary>
        public string GenerarTicket(int ventaID, decimal total, string metodoPago, 
            decimal montoRecibido, decimal vuelto, string vendedor, 
            List<TicketItem> items)
        {
            var sb = new StringBuilder();
            string linea = new string('=', ANCHO_PAPEL);
            string lineaSimple = new string('-', ANCHO_PAPEL);

            // Encabezado
            sb.AppendLine(linea);
            sb.AppendLine(CentrarTexto("SANDWICHERIA WALTERIO", ANCHO_PAPEL));
            sb.AppendLine(CentrarTexto("Gracias por su compra!", ANCHO_PAPEL));
            sb.AppendLine(linea);
            sb.AppendLine(CentrarTexto("Tel: 3885148333", ANCHO_PAPEL));
            sb.AppendLine(CentrarTexto("B. Mejias Este N 475", ANCHO_PAPEL));
            sb.AppendLine(CentrarTexto("820 viviendas", ANCHO_PAPEL));
            sb.AppendLine(CentrarTexto("San Salvador de Jujuy", ANCHO_PAPEL));
            sb.AppendLine(linea);

            // Datos de la venta
            sb.AppendLine($"Ticket #: {ventaID}");
            sb.AppendLine($"Fecha: {DateTime.Now:dd/MM/yy HH:mm}");
            sb.AppendLine($"Vendedor: {TruncarTexto(vendedor, 20)}");
            sb.AppendLine($"Pago: {metodoPago}");

            // Detalle de productos
            sb.AppendLine(lineaSimple);
            sb.AppendLine("DETALLE:");
            sb.AppendLine(lineaSimple);

            foreach (var item in items)
            {
                string nombreCorto = TruncarTexto(item.NombreProducto, 18);
                string lineaItem = $"{item.Cantidad}x  {nombreCorto.PadRight(18)}${item.Subtotal:N0}";
                sb.AppendLine(lineaItem);
            }

            sb.AppendLine(lineaSimple);

            // Totales
            sb.AppendLine(FormatearLinea("TOTAL:", $"${total:N0}", ANCHO_PAPEL));
            sb.AppendLine(linea);

            // Vuelto (solo para efectivo)
            if (metodoPago == "Efectivo" && montoRecibido > 0)
            {
                sb.AppendLine(FormatearLinea("Recibido:", $"${montoRecibido:N0}", ANCHO_PAPEL));
                sb.AppendLine(FormatearLinea("VUELTO:", $"${vuelto:N0}", ANCHO_PAPEL));
            }

            sb.AppendLine();
            sb.AppendLine(CentrarTexto("*** CONSERVE SU TICKET ***", ANCHO_PAPEL));
            sb.AppendLine(CentrarTexto("No valido como factura", ANCHO_PAPEL));
            sb.AppendLine();
            sb.AppendLine(CentrarTexto("Vuelva pronto!", ANCHO_PAPEL));
            sb.AppendLine();

            return sb.ToString();
        }

        /// <summary>
        /// Genera los bytes ESC/POS para impresión directa en impresora térmica
        /// Simplificado para compatibilidad con POS-58C
        /// </summary>
        public byte[] GenerarTicketESCPOS(int ventaID, decimal total, string metodoPago,
            decimal montoRecibido, decimal vuelto, string vendedor,
            List<TicketItem> items)
        {
            var buffer = new List<byte>();
            
            // Inicializar impresora
            buffer.AddRange(ESC_POS.INIT);
            
            // Usar solo texto simple para máxima compatibilidad
            buffer.AddRange(ESC_POS.ALIGN_CENTER);
            
            // Encabezado
            buffer.AddRange(TextoABytes("================================\n"));
            buffer.AddRange(TextoABytes("   SANDWICHERIA WALTERIO\n"));
            buffer.AddRange(TextoABytes("    Gracias por su compra!\n"));
            buffer.AddRange(TextoABytes("================================\n"));
            buffer.AddRange(TextoABytes("    Tel: 3885148333\n"));
            buffer.AddRange(TextoABytes("   B. Mejias Este N 475\n"));
            buffer.AddRange(TextoABytes("      820 viviendas\n"));
            buffer.AddRange(TextoABytes("  San Salvador de Jujuy\n"));
            buffer.AddRange(TextoABytes("================================\n"));
            
            // Datos de venta
            buffer.AddRange(ESC_POS.ALIGN_LEFT);
            buffer.AddRange(TextoABytes($"Ticket #: {ventaID}\n"));
            buffer.AddRange(TextoABytes($"Fecha: {DateTime.Now:dd/MM/yy HH:mm}\n"));
            buffer.AddRange(TextoABytes($"Vendedor: {TruncarTexto(vendedor, 20)}\n"));
            buffer.AddRange(TextoABytes($"Pago: {metodoPago}\n"));
            
            // Detalle
            buffer.AddRange(TextoABytes("--------------------------------\n"));
            buffer.AddRange(TextoABytes("DETALLE:\n"));
            buffer.AddRange(TextoABytes("--------------------------------\n"));
            
            foreach (var item in items)
            {
                // Formato: 1x   Nombre                 Precio
                string nombreCorto = TruncarTexto(item.NombreProducto, 18);
                string linea = $"{item.Cantidad}x  {nombreCorto.PadRight(18)}${item.Subtotal:N0}";
                buffer.AddRange(TextoABytes($"{linea}\n"));
            }
            
            buffer.AddRange(TextoABytes("--------------------------------\n"));
            
            // Total
            buffer.AddRange(TextoABytes($"TOTAL: ${total:N0}\n"));
            buffer.AddRange(TextoABytes("================================\n"));
            
            // Vuelto (solo para efectivo)
            if (metodoPago == "Efectivo" && montoRecibido > 0)
            {
                buffer.AddRange(TextoABytes($"Recibido: ${montoRecibido:N0}\n"));
                buffer.AddRange(TextoABytes($"VUELTO: ${vuelto:N0}\n"));
            }
            
            // Pie
            buffer.AddRange(TextoABytes("\n"));
            buffer.AddRange(ESC_POS.ALIGN_CENTER);
            buffer.AddRange(TextoABytes("*** CONSERVE SU TICKET ***\n"));
            buffer.AddRange(TextoABytes("No valido como factura\n"));
            buffer.AddRange(TextoABytes("\n"));
            buffer.AddRange(TextoABytes("Vuelva pronto!\n"));
            
            // Avanzar papel y cortar
            buffer.AddRange(TextoABytes("\n\n\n\n"));
            buffer.AddRange(ESC_POS.CUT_PARTIAL);
            
            return buffer.ToArray();
        }

        /// <summary>
        /// Imprime directamente en la impresora térmica usando ESC/POS
        /// </summary>
        public bool ImprimirEnTermica(int ventaID, decimal total, string metodoPago,
            decimal montoRecibido, decimal vuelto, string vendedor,
            List<TicketItem> items)
        {
            try
            {
                byte[] datosTicket = GenerarTicketESCPOS(ventaID, total, metodoPago, 
                    montoRecibido, vuelto, vendedor, items);
                
                return EnviarAImpresora(datosTicket);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error imprimiendo: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Envía los bytes directamente a la impresora
        /// </summary>
        private bool EnviarAImpresora(byte[] datos)
        {
            string nombreImpresora = string.IsNullOrEmpty(NombreImpresora) 
                ? ObtenerImpresoraPredeterminada() 
                : NombreImpresora;

            if (string.IsNullOrEmpty(nombreImpresora))
            {
                throw new Exception("No se encontró impresora configurada");
            }

            return RawPrinterHelper.SendBytesToPrinter(nombreImpresora, datos);
        }

        /// <summary>
        /// Obtiene el nombre de la impresora predeterminada
        /// </summary>
        private string ObtenerImpresoraPredeterminada()
        {
            var settings = new PrinterSettings();
            return settings.PrinterName;
        }

        /// <summary>
        /// Lista las impresoras disponibles en el sistema
        /// </summary>
        public List<string> ObtenerImpresorasDisponibles()
        {
            var impresoras = new List<string>();
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                impresoras.Add(printer);
            }
            return impresoras;
        }

        /// <summary>
        /// Genera y guarda el ticket como archivo de texto
        /// </summary>
        public string GuardarTicket(int ventaID, decimal total, string metodoPago,
            decimal montoRecibido, decimal vuelto, string vendedor,
            List<TicketItem> items)
        {
            string contenido = GenerarTicket(ventaID, total, metodoPago, montoRecibido, vuelto, vendedor, items);
            
            string carpetaTickets = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "SandwicheriaWalterio",
                "Tickets");
            
            Directory.CreateDirectory(carpetaTickets);
            
            string nombreArchivo = $"Ticket_{ventaID}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string rutaCompleta = Path.Combine(carpetaTickets, nombreArchivo);
            
            File.WriteAllText(rutaCompleta, contenido, Encoding.UTF8);
            
            return rutaCompleta;
        }

        /// <summary>
        /// Abre el ticket en el bloc de notas
        /// </summary>
        public void MostrarTicket(string rutaTicket)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "notepad.exe",
                    Arguments = rutaTicket,
                    UseShellExecute = true
                });
            }
            catch
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = rutaTicket,
                    UseShellExecute = true
                });
            }
        }

        /// <summary>
        /// Imprime usando el método tradicional de Windows (para impresoras no térmicas)
        /// </summary>
        public void ImprimirTicket(string rutaTicket)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = rutaTicket,
                    Verb = "print",
                    UseShellExecute = true,
                    CreateNoWindow = true
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al imprimir: {ex.Message}");
            }
        }

        /// <summary>
        /// Prueba la impresora térmica imprimiendo un ticket de prueba
        /// </summary>
        public bool ImprimirPrueba()
        {
            try
            {
                var buffer = new List<byte>();
                
                buffer.AddRange(ESC_POS.INIT);
                buffer.AddRange(ESC_POS.ALIGN_CENTER);
                buffer.AddRange(TextoABytes("================================\n"));
                buffer.AddRange(TextoABytes("   PRUEBA DE IMPRESION\n"));
                buffer.AddRange(TextoABytes("================================\n"));
                buffer.AddRange(TextoABytes("  Sandwicheria Walterio\n"));
                buffer.AddRange(TextoABytes("   Impresora termica OK\n"));
                buffer.AddRange(TextoABytes($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}\n"));
                buffer.AddRange(TextoABytes("================================\n"));
                buffer.AddRange(TextoABytes("\n\n\n"));
                buffer.AddRange(ESC_POS.CUT_PARTIAL);
                
                return EnviarAImpresora(buffer.ToArray());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en prueba: {ex.Message}");
                return false;
            }
        }

        // ===== MÉTODOS AUXILIARES =====

        private string CentrarTexto(string texto, int ancho)
        {
            if (texto.Length >= ancho) return texto.Substring(0, ancho);
            int espacios = (ancho - texto.Length) / 2;
            return new string(' ', espacios) + texto;
        }

        private string TruncarTexto(string texto, int maxLongitud)
        {
            if (string.IsNullOrEmpty(texto)) return "";
            return texto.Length <= maxLongitud ? texto : texto.Substring(0, maxLongitud);
        }

        private string FormatearLinea(string izquierda, string derecha, int ancho)
        {
            int espacios = ancho - izquierda.Length - derecha.Length;
            if (espacios < 1) espacios = 1;
            return izquierda + new string(' ', espacios) + derecha;
        }

        private byte[] TextoABytes(string texto)
        {
            // Usar ASCII simple para máxima compatibilidad con POS-58C
            // Reemplazar caracteres especiales
            texto = texto.Replace("á", "a").Replace("é", "e").Replace("í", "i")
                        .Replace("ó", "o").Replace("ú", "u").Replace("ñ", "n")
                        .Replace("Á", "A").Replace("É", "E").Replace("Í", "I")
                        .Replace("Ó", "O").Replace("Ú", "U").Replace("Ñ", "N");
            return Encoding.ASCII.GetBytes(texto);
        }
    }

    /// <summary>
    /// Helper para enviar datos raw a la impresora
    /// </summary>
    public class RawPrinterHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType;
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

        public static bool SendBytesToPrinter(string szPrinterName, byte[] pBytes)
        {
            Int32 dwCount = pBytes.Length;
            IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(dwCount);
            Marshal.Copy(pBytes, 0, pUnmanagedBytes, dwCount);

            bool bSuccess = false;
            IntPtr hPrinter;

            if (OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
            {
                DOCINFOA di = new DOCINFOA();
                di.pDocName = "Ticket Sandwicheria";
                di.pDataType = "RAW";

                if (StartDocPrinter(hPrinter, 1, di))
                {
                    if (StartPagePrinter(hPrinter))
                    {
                        Int32 dwWritten;
                        bSuccess = WritePrinter(hPrinter, pUnmanagedBytes, dwCount, out dwWritten);
                        EndPagePrinter(hPrinter);
                    }
                    EndDocPrinter(hPrinter);
                }
                ClosePrinter(hPrinter);
            }

            Marshal.FreeCoTaskMem(pUnmanagedBytes);
            return bSuccess;
        }
    }

    /// <summary>
    /// Item para el ticket de venta
    /// </summary>
    public class TicketItem
    {
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
