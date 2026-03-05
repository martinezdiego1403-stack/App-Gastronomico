using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SandwicheriaWalterio.Data;
using SandwicheriaWalterio.Helpers;
using SandwicheriaWalterio.Models;
using Microsoft.Win32;

namespace SandwicheriaWalterio.ViewModels
{
    /// <summary>
    /// ViewModel para el módulo de Reportes.
    /// 
    /// 🎓 EXPLICACIÓN PARA PRINCIPIANTES:
    /// 
    /// Este archivo es el "cerebro" de la pantalla de reportes.
    /// Contiene toda la LÓGICA (los cálculos, las decisiones, los datos)
    /// mientras que el archivo XAML contiene la APARIENCIA (colores, posiciones, botones).
    /// 
    /// Esto se llama patrón MVVM:
    /// - Model (Modelo): Los datos puros (ReporteModels.cs)
    /// - View (Vista): La interfaz gráfica (ReportesView.xaml)
    /// - ViewModel: Este archivo - conecta el Model con la View
    /// </summary>
    public class ReportesViewModel : ViewModelBase
    {
        // ========================================
        // CAMPOS PRIVADOS
        // ========================================
        // Son como "variables internas" que solo este archivo puede usar
        
        private readonly ReporteRepository _reporteRepository;
        private DateTime _fechaInicio;
        private DateTime _fechaFin;
        private string _periodoSeleccionado;
        private ResumenGeneral _resumen;
        private int _tabSeleccionado;

        // ========================================
        // COLECCIONES PARA GRÁFICOS
        // ========================================
        // ObservableCollection es una lista especial que avisa a la interfaz
        // cada vez que algo cambia (se agrega, elimina o modifica)
        
        public ObservableCollection<ISeries> SeriesVentasPorDia { get; set; }
        public ObservableCollection<ISeries> SeriesMetodosPago { get; set; }
        public ObservableCollection<ISeries> SeriesVentasPorHora { get; set; }
        public ObservableCollection<ISeries> SeriesCategorias { get; set; }
        public ObservableCollection<Axis> EjesX { get; set; }
        public ObservableCollection<Axis> EjesY { get; set; }
        public ObservableCollection<Axis> EjesXHora { get; set; }
        public ObservableCollection<Axis> EjesYHora { get; set; }

        // Colecciones para listas
        public ObservableCollection<ProductoMasVendido> TopProductos { get; set; }
        public ObservableCollection<HistorialCaja> HistorialCajas { get; set; }

        // ========================================
        // PROPIEDADES PÚBLICAS
        // ========================================
        // Estas propiedades están "conectadas" a la interfaz mediante Data Binding
        
        public DateTime FechaInicio
        {
            get => _fechaInicio;
            set
            {
                // SetProperty actualiza el valor Y notifica a la interfaz
                SetProperty(ref _fechaInicio, value);
                CargarDatos(); // Recarga los datos cuando cambia la fecha
            }
        }

        public DateTime FechaFin
        {
            get => _fechaFin;
            set
            {
                SetProperty(ref _fechaFin, value);
                CargarDatos();
            }
        }

        public string PeriodoSeleccionado
        {
            get => _periodoSeleccionado;
            set
            {
                SetProperty(ref _periodoSeleccionado, value);
                AplicarPeriodo(value);
            }
        }

        public ResumenGeneral Resumen
        {
            get => _resumen;
            set => SetProperty(ref _resumen, value);
        }

        public int TabSeleccionado
        {
            get => _tabSeleccionado;
            set => SetProperty(ref _tabSeleccionado, value);
        }

        // Lista de opciones para el ComboBox de períodos
        public List<string> Periodos { get; } = new List<string>
        {
            "Hoy",
            "Ayer",
            "Esta Semana",
            "Semana Pasada",
            "Este Mes",
            "Mes Pasado",
            "Últimos 7 días",
            "Últimos 30 días",
            "Personalizado"
        };

        // ========================================
        // COMANDOS
        // ========================================
        // Los comandos son como "acciones" que se ejecutan cuando
        // el usuario hace clic en un botón
        
        public ICommand ActualizarCommand { get; }
        public ICommand ExportarExcelCommand { get; }
        public ICommand ExportarHistorialCommand { get; }
        public ICommand EnviarEmailCommand { get; }

        // ========================================
        // CONSTRUCTOR
        // ========================================
        // Este método se ejecuta cuando se crea una instancia del ViewModel
        
        public ReportesViewModel()
        {
            _reporteRepository = new ReporteRepository();

            // Inicializar todas las colecciones (listas vacías)
            SeriesVentasPorDia = new ObservableCollection<ISeries>();
            SeriesMetodosPago = new ObservableCollection<ISeries>();
            SeriesVentasPorHora = new ObservableCollection<ISeries>();
            SeriesCategorias = new ObservableCollection<ISeries>();
            EjesX = new ObservableCollection<Axis>();
            EjesY = new ObservableCollection<Axis>();
            EjesXHora = new ObservableCollection<Axis>();
            EjesYHora = new ObservableCollection<Axis>();
            TopProductos = new ObservableCollection<ProductoMasVendido>();
            HistorialCajas = new ObservableCollection<HistorialCaja>();

            // Inicializar comandos (conectar botones con métodos)
            ActualizarCommand = new RelayCommand(_ => CargarDatos());
            ExportarExcelCommand = new RelayCommand(_ => ExportarAExcel());
            ExportarHistorialCommand = new RelayCommand(_ => ExportarHistorialAExcel());
            EnviarEmailCommand = new RelayCommand(_ => EnviarPorEmail());

            // Configurar período inicial
            _periodoSeleccionado = "Hoy";
            _fechaInicio = DateTime.Today;
            _fechaFin = DateTime.Today;

            // Cargar datos al iniciar
            CargarDatos();
            CargarHistorialCajas();
        }

        // ========================================
        // MÉTODOS PRIVADOS
        // ========================================

        /// <summary>
        /// Aplica el período seleccionado y calcula las fechas correspondientes.
        /// 
        /// 🎓 EXPLICACIÓN:
        /// Cuando el usuario selecciona "Esta Semana" en el ComboBox,
        /// este método calcula automáticamente cuál es el primer día
        /// de la semana y cuál es hoy.
        /// </summary>
        private void AplicarPeriodo(string periodo)
        {
            DateTime hoy = DateTime.Today;

            switch (periodo)
            {
                case "Hoy":
                    _fechaInicio = hoy;
                    _fechaFin = hoy;
                    break;

                case "Ayer":
                    _fechaInicio = hoy.AddDays(-1);
                    _fechaFin = hoy.AddDays(-1);
                    break;

                case "Esta Semana":
                    // DayOfWeek devuelve 0=Domingo, 1=Lunes, etc.
                    // Restamos esos días para llegar al domingo (inicio de semana)
                    int diasDesdeInicio = (int)hoy.DayOfWeek;
                    _fechaInicio = hoy.AddDays(-diasDesdeInicio);
                    _fechaFin = hoy;
                    break;

                case "Semana Pasada":
                    int diasDesdeLunes = (int)hoy.DayOfWeek;
                    DateTime inicioSemanaActual = hoy.AddDays(-diasDesdeLunes);
                    _fechaInicio = inicioSemanaActual.AddDays(-7);
                    _fechaFin = inicioSemanaActual.AddDays(-1);
                    break;

                case "Este Mes":
                    // Primer día del mes actual
                    _fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
                    _fechaFin = hoy;
                    break;

                case "Mes Pasado":
                    DateTime primerDiaMesAnterior = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-1);
                    _fechaInicio = primerDiaMesAnterior;
                    // Último día del mes anterior
                    _fechaFin = primerDiaMesAnterior.AddMonths(1).AddDays(-1);
                    break;

                case "Últimos 7 días":
                    _fechaInicio = hoy.AddDays(-6);
                    _fechaFin = hoy;
                    break;

                case "Últimos 30 días":
                    _fechaInicio = hoy.AddDays(-29);
                    _fechaFin = hoy;
                    break;

                case "Personalizado":
                    // No cambiar fechas - el usuario las seleccionará manualmente
                    return;
            }

            // Notificar que las fechas cambiaron (para actualizar la interfaz)
            OnPropertyChanged(nameof(FechaInicio));
            OnPropertyChanged(nameof(FechaFin));
            
            CargarDatos();
        }

        /// <summary>
        /// Carga todos los datos de reportes desde la base de datos.
        /// Este es el método principal que actualiza toda la pantalla.
        /// </summary>
        private void CargarDatos()
        {
            try
            {
                // Cargar resumen general (los 4 cuadros de arriba)
                Resumen = _reporteRepository.ObtenerResumenGeneral(FechaInicio, FechaFin);

                // Cargar cada gráfico
                CargarGraficoVentasPorDia();
                CargarTopProductos();
                CargarGraficoMetodosPago();
                CargarGraficoVentasPorHora();
                CargarGraficoCategorias();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar reportes: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Crea el gráfico de líneas mostrando las ventas por día.
        /// 
        /// 🎓 EXPLICACIÓN DE LiveCharts:
        /// - LineSeries: Dibuja una línea conectando puntos
        /// - Values: Los números que queremos graficar
        /// - Stroke: El color y grosor de la línea
        /// - GeometryFill/Stroke: El círculito en cada punto
        /// </summary>
        private void CargarGraficoVentasPorDia()
        {
            var datos = _reporteRepository.ObtenerVentasPorDia(FechaInicio, FechaFin);

            SeriesVentasPorDia.Clear();
            EjesX.Clear();
            EjesY.Clear();

            if (datos.Count == 0)
            {
                return;
            }

            // Crear la serie de línea
            var serie = new LineSeries<double>
            {
                Values = datos.Select(d => (double)d.TotalVentas).ToArray(),
                Name = "Ventas",
                Fill = null, // Sin relleno debajo de la línea
                Stroke = new SolidColorPaint(SKColors.DodgerBlue) { StrokeThickness = 3 },
                GeometrySize = 10,
                GeometryFill = new SolidColorPaint(SKColors.White),
                GeometryStroke = new SolidColorPaint(SKColors.DodgerBlue) { StrokeThickness = 3 }
            };

            SeriesVentasPorDia.Add(serie);

            // Configurar el eje X (las fechas)
            EjesX.Add(new Axis
            {
                Labels = datos.Select(d => d.FechaDisplay).ToArray(),
                LabelsRotation = 45 // Rotar etiquetas 45 grados para que se lean mejor
            });

            // Configurar el eje Y (los valores en pesos)
            EjesY.Add(new Axis
            {
                Name = "Ventas ($)",
                Labeler = value => $"${value:N0}" // Formato: $1,234
            });
        }

        /// <summary>
        /// Carga la lista de los 10 productos más vendidos.
        /// </summary>
        private void CargarTopProductos()
        {
            var productos = _reporteRepository.ObtenerProductosMasVendidos(FechaInicio, FechaFin, 10);

            TopProductos.Clear();
            foreach (var producto in productos)
            {
                TopProductos.Add(producto);
            }
        }

        /// <summary>
        /// Crea el gráfico de torta (pie) mostrando ventas por método de pago.
        /// 
        /// 🎓 EXPLICACIÓN:
        /// Un gráfico de torta muestra "partes de un todo".
        /// Por ejemplo: 60% efectivo, 30% tarjeta, 10% transferencia.
        /// Cada "pedazo" de la torta representa un porcentaje.
        /// </summary>
        private void CargarGraficoMetodosPago()
        {
            var datos = _reporteRepository.ObtenerVentasPorMetodoPago(FechaInicio, FechaFin);

            SeriesMetodosPago.Clear();

            if (datos.Count == 0)
            {
                return;
            }

            // Colores para cada método de pago
            var colores = new[]
            {
                SKColors.Green,      // Efectivo
                SKColors.DodgerBlue, // Tarjeta
                SKColors.Orange,     // Transferencia
                SKColors.Purple      // Otros
            };

            int colorIndex = 0;
            foreach (var dato in datos)
            {
                var serie = new PieSeries<double>
                {
                    Values = new[] { (double)dato.TotalVentas },
                    Name = $"{dato.MetodoPago} ({dato.Porcentaje:F1}%)",
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point => $"{dato.Porcentaje:F1}%"
                };

                if (colorIndex < colores.Length)
                {
                    serie.Fill = new SolidColorPaint(colores[colorIndex]);
                }

                SeriesMetodosPago.Add(serie);
                colorIndex++;
            }
        }

        /// <summary>
        /// Crea el gráfico de barras mostrando ventas por hora del día.
        /// 
        /// 🎓 EXPLICACIÓN:
        /// Este gráfico te ayuda a identificar las "horas pico".
        /// Por ejemplo: si la barra de las 13:00 es muy alta,
        /// significa que a esa hora tienes muchos clientes (hora del almuerzo).
        /// </summary>
        private void CargarGraficoVentasPorHora()
        {
            var datos = _reporteRepository.ObtenerVentasPorHora(FechaInicio, FechaFin);

            SeriesVentasPorHora.Clear();
            EjesXHora.Clear();
            EjesYHora.Clear();

            if (datos.Count == 0)
            {
                return;
            }

            // Crear serie de barras
            var serie = new ColumnSeries<double>
            {
                Values = datos.Select(d => (double)d.CantidadVentas).ToArray(),
                Name = "Ventas por Hora",
                Fill = new SolidColorPaint(SKColors.Coral),
                MaxBarWidth = 30
            };

            SeriesVentasPorHora.Add(serie);

            // Eje X: las horas
            EjesXHora.Add(new Axis
            {
                Labels = datos.Select(d => d.HoraDisplay).ToArray(),
                LabelsRotation = 45
            });

            // Eje Y: cantidad de ventas
            EjesYHora.Add(new Axis
            {
                Name = "Cantidad"
            });
        }

        /// <summary>
        /// Crea el gráfico de torta mostrando ventas por categoría.
        /// </summary>
        private void CargarGraficoCategorias()
        {
            var datos = _reporteRepository.ObtenerVentasPorCategoria(FechaInicio, FechaFin);

            SeriesCategorias.Clear();

            if (datos.Count == 0)
            {
                return;
            }

            var colores = new[]
            {
                SKColors.Coral,
                SKColors.MediumSeaGreen,
                SKColors.DodgerBlue,
                SKColors.Gold,
                SKColors.MediumPurple,
                SKColors.Tomato
            };

            int colorIndex = 0;
            foreach (var dato in datos)
            {
                var serie = new PieSeries<double>
                {
                    Values = new[] { (double)dato.TotalVentas },
                    Name = $"{dato.Categoria} ({dato.Porcentaje:F1}%)",
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 12,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsFormatter = _ => $"{dato.Porcentaje:F0}%"
                };

                if (colorIndex < colores.Length)
                {
                    serie.Fill = new SolidColorPaint(colores[colorIndex]);
                }

                SeriesCategorias.Add(serie);
                colorIndex++;
            }
        }

        /// <summary>
        /// Carga el historial de cajas cerradas.
        /// </summary>
        private void CargarHistorialCajas()
        {
            try
            {
                var historial = _reporteRepository.ObtenerHistorialCajas(50);
                
                HistorialCajas.Clear();
                foreach (var caja in historial)
                {
                    HistorialCajas.Add(caja);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar historial: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ========================================
        // EXPORTACIÓN A EXCEL (CSV)
        // ========================================

        /// <summary>
        /// Exporta el reporte actual a un archivo Excel profesional.
        /// 
        /// 🎓 EXPLICACIÓN:
        /// Ahora usamos ClosedXML para crear un Excel con formato profesional:
        /// - Colores corporativos
        /// - Tablas con bordes
        /// - Gráficos de barras (usando texto)
        /// - Múltiples hojas organizadas
        /// - KPIs con diseño de tarjetas
        /// </summary>
        private void ExportarAExcel()
        {
            try
            {
                // Mostrar diálogo para elegir dónde guardar el archivo
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Archivo Excel|*.xlsx",
                    Title = "Exportar Reporte Profesional",
                    FileName = $"Reporte_Walterio_{FechaInicio:yyyyMMdd}_a_{FechaFin:yyyyMMdd}"
                };

                if (saveDialog.ShowDialog() != true)
                    return;

                // Usar el nuevo servicio de Excel profesional
                var excelService = new SandwicheriaWalterio.Services.ExcelReportService();
                excelService.GenerarReporteCompleto(FechaInicio, FechaFin, saveDialog.FileName);

                // Preguntar si desea abrir el archivo
                var resultado = MessageBox.Show(
                    $"✅ Reporte exportado exitosamente a:\n{saveDialog.FileName}\n\n¿Desea abrir el archivo ahora?",
                    "Exportación Exitosa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (resultado == MessageBoxResult.Yes)
                {
                    // Abrir el archivo con Excel
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = saveDialog.FileName,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Exporta el historial de cajas a Excel.
        /// </summary>
        private void ExportarHistorialAExcel()
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Archivo CSV|*.csv",
                    Title = "Exportar Historial de Cajas",
                    FileName = $"Historial_Cajas_{DateTime.Now:yyyyMMdd}"
                };

                if (saveDialog.ShowDialog() != true)
                    return;

                var sb = new StringBuilder();

                sb.AppendLine("HISTORIAL DE CAJAS - SANDWICHERÍA WALTERIO");
                sb.AppendLine($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}");
                sb.AppendLine();

                sb.AppendLine("ID,Apertura,Cierre,Usuario,Monto Inicial,Total Ventas,Monto Final,Diferencia,Ventas,Estado");
                
                foreach (var caja in HistorialCajas)
                {
                    sb.AppendLine($"{caja.CajaID}," +
                                  $"{caja.FechaAperturaDisplay}," +
                                  $"{caja.FechaCierreDisplay}," +
                                  $"{caja.Usuario}," +
                                  $"${caja.MontoInicial:N2}," +
                                  $"${caja.TotalVentas:N2}," +
                                  $"${caja.MontoFinal:N2}," +
                                  $"${caja.Diferencia:N2}," +
                                  $"{caja.CantidadVentas}," +
                                  $"{caja.Estado}");
                }

                File.WriteAllText(saveDialog.FileName, sb.ToString(), Encoding.UTF8);

                MessageBox.Show(
                    $"Historial exportado exitosamente a:\n{saveDialog.FileName}",
                    "Exportación Exitosa",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar historial: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Abre la ventana para enviar el reporte por email.
        /// </summary>
        private void EnviarPorEmail()
        {
            try
            {
                var ventanaEmail = new SandwicheriaWalterio.Views.EnviarEmailWindow(
                    FechaInicio,
                    FechaFin,
                    Resumen?.TotalVentas ?? 0,
                    Resumen?.CantidadVentas ?? 0);

                ventanaEmail.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir ventana de email: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
