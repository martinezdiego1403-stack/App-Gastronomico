using Microsoft.Extensions.Configuration;

namespace SandwicheriaWalterio.Services
{
    /// <summary>
    /// Clase centralizada para leer la configuración de appsettings.json
    ///
    /// COMO FUNCIONA:
    /// 1. Busca el archivo appsettings.json junto al .exe
    /// 2. Lee las connection strings y las guarda en memoria
    /// 3. Cualquier parte de la app puede pedirle las credenciales
    ///
    /// PATRON SINGLETON:
    /// Solo existe UNA instancia de esta clase en toda la app.
    /// Esto evita leer el archivo JSON multiples veces.
    /// </summary>
    public class AppConfiguration
    {
        private static AppConfiguration? _instance;
        private static readonly object _lock = new object();

        private readonly IConfiguration _configuration;

        private AppConfiguration()
        {
            // Buscar appsettings.json en la carpeta donde esta el .exe
            var basePath = AppDomain.CurrentDomain.BaseDirectory;

            _configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        public static AppConfiguration Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new AppConfiguration();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Connection string para PostgreSQL LOCAL (la PC del cliente)
        /// </summary>
        public string LocalConnectionString =>
            _configuration.GetConnectionString("Local") ?? "";

        /// <summary>
        /// Connection string para Supabase (nube)
        /// </summary>
        public string SupabaseConnectionString =>
            _configuration.GetConnectionString("Supabase") ?? "";
    }
}
