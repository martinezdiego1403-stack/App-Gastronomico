using System;
using System.Collections.Generic;

namespace SandwicheriaWalterio.Services
{
    /// <summary>
    /// Servicio para convertir entre diferentes unidades de medida.
    /// Permite que las recetas usen unidades diferentes a las de mercadería.
    /// Ejemplo: Receta usa 300 gramos, Mercadería tiene 3 Kg → convierte y descuenta 0.3 Kg
    /// </summary>
    public static class UnidadMedidaService
    {
        // Diccionario case-insensitive para unidades
        private static readonly Dictionary<string, (string grupo, decimal factorABase)> _unidades;

        static UnidadMedidaService()
        {
            _unidades = new Dictionary<string, (string grupo, decimal factorABase)>(StringComparer.OrdinalIgnoreCase);
            
            // Grupo PESO - Unidad base: Gramo
            _unidades["Miligramo"] = ("peso", 0.001m);
            _unidades["Gramo"] = ("peso", 1m);
            _unidades["gramos"] = ("peso", 1m);
            _unidades["Kg"] = ("peso", 1000m);
            _unidades["kg"] = ("peso", 1000m);
            _unidades["Kilogramo"] = ("peso", 1000m);
            
            // Grupo VOLUMEN - Unidad base: Mililitro
            _unidades["Mililitro"] = ("volumen", 1m);
            _unidades["ml"] = ("volumen", 1m);
            _unidades["Litro"] = ("volumen", 1000m);
            _unidades["litros"] = ("volumen", 1000m);
            _unidades["Litros"] = ("volumen", 1000m);
            
            // Grupo UNIDAD - Sin conversión
            _unidades["Unidad"] = ("unidad", 1m);
            _unidades["unidad"] = ("unidad", 1m);
            _unidades["Caja"] = ("caja", 1m);
            _unidades["Metro"] = ("metro", 1m);
        }

        /// <summary>
        /// Verifica si dos unidades son compatibles (del mismo grupo)
        /// </summary>
        public static bool SonCompatibles(string unidadOrigen, string unidadDestino)
        {
            if (string.IsNullOrEmpty(unidadOrigen) || string.IsNullOrEmpty(unidadDestino))
                return false;

            // Si son iguales (case-insensitive), son compatibles
            if (unidadOrigen.Equals(unidadDestino, StringComparison.OrdinalIgnoreCase))
                return true;

            // Buscar en el diccionario
            if (_unidades.TryGetValue(unidadOrigen, out var infoOrigen) &&
                _unidades.TryGetValue(unidadDestino, out var infoDestino))
            {
                return infoOrigen.grupo == infoDestino.grupo;
            }

            return false;
        }

        /// <summary>
        /// Convierte una cantidad de una unidad a otra.
        /// Ejemplo: Convertir(300, "Gramo", "Kg") → 0.3
        /// </summary>
        public static decimal Convertir(decimal cantidad, string unidadOrigen, string unidadDestino)
        {
            // Si son iguales, no hay conversión
            if (string.IsNullOrEmpty(unidadOrigen) || string.IsNullOrEmpty(unidadDestino))
                return cantidad;

            if (unidadOrigen.Equals(unidadDestino, StringComparison.OrdinalIgnoreCase))
                return cantidad;

            // Buscar factores de conversión
            if (!_unidades.TryGetValue(unidadOrigen, out var infoOrigen) ||
                !_unidades.TryGetValue(unidadDestino, out var infoDestino))
            {
                return cantidad;
            }

            // Verificar que sean del mismo grupo
            if (infoOrigen.grupo != infoDestino.grupo)
            {
                return cantidad;
            }

            // Convertir: primero a unidad base, luego a unidad destino
            decimal cantidadEnBase = cantidad * infoOrigen.factorABase;
            decimal cantidadConvertida = cantidadEnBase / infoDestino.factorABase;

            return cantidadConvertida;
        }

        /// <summary>
        /// Obtiene el nombre del grupo de una unidad
        /// </summary>
        public static string ObtenerGrupo(string unidad)
        {
            if (string.IsNullOrEmpty(unidad))
                return "desconocido";

            if (_unidades.TryGetValue(unidad, out var info))
                return info.grupo;

            return "desconocido";
        }

        /// <summary>
        /// Formatea una cantidad con su unidad de forma legible (sin decimales innecesarios)
        /// </summary>
        public static string Formatear(decimal cantidad, string unidad)
        {
            if (string.IsNullOrEmpty(unidad))
                return FormatearNumero(cantidad);

            return $"{FormatearNumero(cantidad)} {unidad}";
        }

        /// <summary>
        /// Formatea un número sin decimales innecesarios
        /// Ejemplo: 1.000 → "1", 300.000 → "300", 2.5 → "2.5"
        /// </summary>
        public static string FormatearNumero(decimal cantidad)
        {
            // Si es un número entero, mostrarlo sin decimales
            if (cantidad == Math.Floor(cantidad))
            {
                return ((int)cantidad).ToString();
            }
            
            // Si tiene decimales, mostrar solo los necesarios (máximo 2)
            return cantidad.ToString("0.##");
        }

        /// <summary>
        /// Calcula cuánto descontar de mercadería cuando se vende una receta.
        /// </summary>
        public static decimal CalcularDescuento(decimal cantidadReceta, string unidadReceta, 
            string unidadMercaderia, int cantidadVendida)
        {
            decimal cantidadTotalNecesaria = cantidadReceta * cantidadVendida;
            decimal cantidadADescontar = Convertir(cantidadTotalNecesaria, unidadReceta, unidadMercaderia);
            return cantidadADescontar;
        }
    }
}
