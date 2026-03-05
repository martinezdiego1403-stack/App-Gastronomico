using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SandwicheriaWalterio.Helpers
{
    /// <summary>
    /// Convierte null a Visibility.Collapsed y cualquier valor a Visibility.Visible.
    /// Útil para mostrar/ocultar paneles según si hay un item seleccionado.
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convierte null a Visibility.Visible y cualquier valor a Visibility.Collapsed.
    /// Es el inverso del anterior.
    /// </summary>
    public class NullToVisibilityInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convierte un bool (Activo) a texto "❌ Desactivar" o "✅ Activar".
    /// </summary>
    public class BoolToActivarDesactivarConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool activo)
            {
                return activo ? "❌ Desactivar" : "✅ Activar";
            }
            return "⚡ Cambiar Estado";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convierte bool a color (verde para true, rojo para false).
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "#27AE60" : "#E74C3C";
            }
            return "#7F8C8D";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convierte Stock + UnidadMedida a formato corto: "24U", "4.5Kg", "2L"
    /// </summary>
    public class StockConUnidadConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] == null)
                return "0";

            decimal stock = 0;
            if (values[0] is decimal d) stock = d;
            else if (values[0] is int i) stock = i;
            else if (values[0] is double dbl) stock = (decimal)dbl;

            string unidad = values[1]?.ToString() ?? "U";
            string inicial = ObtenerInicialUnidad(unidad);

            // Formatear stock sin decimales innecesarios
            string stockFormateado = stock == Math.Floor(stock) 
                ? ((int)stock).ToString() 
                : stock.ToString("0.#");

            return $"{stockFormateado}{inicial}";
        }

        private string ObtenerInicialUnidad(string unidad)
        {
            if (string.IsNullOrEmpty(unidad)) return "U";

            return unidad.ToLower() switch
            {
                "unidad" => "U",
                "unidades" => "U",
                "gramo" => "g",
                "gramos" => "g",
                "kg" => "Kg",
                "kilogramo" => "Kg",
                "kilogramos" => "Kg",
                "litro" => "L",
                "litros" => "L",
                "mililitro" => "ml",
                "mililitros" => "ml",
                "ml" => "ml",
                "miligramo" => "mg",
                "miligramos" => "mg",
                "mg" => "mg",
                _ => unidad.Length > 2 ? unidad.Substring(0, 2) : unidad
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
