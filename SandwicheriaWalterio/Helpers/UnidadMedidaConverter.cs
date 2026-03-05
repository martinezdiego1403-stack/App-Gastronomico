namespace SandwicheriaWalterio.Helpers
{
    /// <summary>
    /// Helper para convertir entre diferentes unidades de medida
    /// Ejemplo: 300 gramos → 0.3 Kg
    /// </summary>
    public static class UnidadMedidaConverter
    {
        /// <summary>
        /// Convierte una cantidad de una unidad a otra
        /// </summary>
        /// <param name="cantidad">Cantidad en la unidad origen</param>
        /// <param name="unidadOrigen">Unidad de la receta (ej: "Gramo")</param>
        /// <param name="unidadDestino">Unidad de la mercadería (ej: "Kg")</param>
        /// <returns>Cantidad convertida a la unidad destino</returns>
        public static decimal Convertir(decimal cantidad, string unidadOrigen, string unidadDestino)
        {
            // Si son iguales, no convertir
            if (NormalizarUnidad(unidadOrigen) == NormalizarUnidad(unidadDestino))
                return cantidad;

            // Convertir a unidad base y luego a destino
            decimal cantidadBase = ConvertirAUnidadBase(cantidad, unidadOrigen);
            return ConvertirDesdeUnidadBase(cantidadBase, unidadDestino);
        }

        /// <summary>
        /// Normaliza el nombre de la unidad (minúsculas, sin tildes)
        /// </summary>
        private static string NormalizarUnidad(string unidad)
        {
            if (string.IsNullOrEmpty(unidad)) return "unidad";
            
            return unidad.ToLower().Trim() switch
            {
                // Peso
                "kg" or "kilo" or "kilogramo" or "kilogramos" => "kg",
                "g" or "gr" or "gramo" or "gramos" => "gramo",
                "mg" or "miligramo" or "miligramos" => "miligramo",
                
                // Volumen
                "l" or "lt" or "litro" or "litros" => "litro",
                "ml" or "mililitro" or "mililitros" => "mililitro",
                
                // Otros
                "unidad" or "unidades" or "u" or "un" => "unidad",
                "caja" or "cajas" => "caja",
                "metro" or "metros" or "m" => "metro",
                
                _ => unidad.ToLower()
            };
        }

        /// <summary>
        /// Convierte cualquier unidad a su unidad base
        /// Peso: gramos, Volumen: mililitros, Otros: unidad
        /// </summary>
        private static decimal ConvertirAUnidadBase(decimal cantidad, string unidad)
        {
            string unidadNorm = NormalizarUnidad(unidad);
            
            return unidadNorm switch
            {
                // Peso → base en gramos
                "kg" => cantidad * 1000m,           // 1 kg = 1000 g
                "gramo" => cantidad,                 // ya está en gramos
                "miligramo" => cantidad / 1000m,    // 1000 mg = 1 g
                
                // Volumen → base en mililitros
                "litro" => cantidad * 1000m,        // 1 L = 1000 ml
                "mililitro" => cantidad,             // ya está en ml
                
                // Otros (no convertibles)
                _ => cantidad
            };
        }

        /// <summary>
        /// Convierte desde unidad base a la unidad destino
        /// </summary>
        private static decimal ConvertirDesdeUnidadBase(decimal cantidadBase, string unidadDestino)
        {
            string unidadNorm = NormalizarUnidad(unidadDestino);
            
            return unidadNorm switch
            {
                // De gramos a...
                "kg" => cantidadBase / 1000m,       // g → kg
                "gramo" => cantidadBase,             // ya está en gramos
                "miligramo" => cantidadBase * 1000m, // g → mg
                
                // De mililitros a...
                "litro" => cantidadBase / 1000m,    // ml → L
                "mililitro" => cantidadBase,         // ya está en ml
                
                // Otros
                _ => cantidadBase
            };
        }

        /// <summary>
        /// Verifica si dos unidades son compatibles (se pueden convertir)
        /// </summary>
        public static bool SonCompatibles(string unidad1, string unidad2)
        {
            string u1 = NormalizarUnidad(unidad1);
            string u2 = NormalizarUnidad(unidad2);

            // Mismo tipo
            if (u1 == u2) return true;

            // Peso
            var unidadesPeso = new[] { "kg", "gramo", "miligramo" };
            if (unidadesPeso.Contains(u1) && unidadesPeso.Contains(u2)) return true;

            // Volumen
            var unidadesVolumen = new[] { "litro", "mililitro" };
            if (unidadesVolumen.Contains(u1) && unidadesVolumen.Contains(u2)) return true;

            return false;
        }

        /// <summary>
        /// Obtiene el grupo de la unidad (Peso, Volumen, Otro)
        /// </summary>
        public static string ObtenerGrupoUnidad(string unidad)
        {
            string u = NormalizarUnidad(unidad);
            
            if (new[] { "kg", "gramo", "miligramo" }.Contains(u))
                return "Peso";
            
            if (new[] { "litro", "mililitro" }.Contains(u))
                return "Volumen";
            
            return "Otro";
        }
    }
}
