using System.Net.NetworkInformation;

namespace SandwicheriaWalterio.Services
{
    /// <summary>
    /// Servicio para detectar conectividad a internet y a Supabase
    /// </summary>
    public class ConnectivityService
    {
        private static ConnectivityService? _instance;
        private static readonly object _lock = new object();

        private bool _hayInternet = false;
        private bool _hayConexionSupabase = false;
        private DateTime _ultimaVerificacion = DateTime.MinValue;
        private readonly TimeSpan _intervaloVerificacion = TimeSpan.FromSeconds(30);

        // Eventos para notificar cambios de conectividad
        public event EventHandler<bool>? ConectividadCambiada;
        public event EventHandler? ConexionRestaurada;
        public event EventHandler? ConexionPerdida;

        private ConnectivityService() { }

        public static ConnectivityService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new ConnectivityService();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Indica si hay conexión a internet
        /// </summary>
        public bool HayInternet => _hayInternet;

        /// <summary>
        /// Indica si hay conexión a Supabase (PostgreSQL)
        /// </summary>
        public bool HayConexionSupabase => _hayConexionSupabase;

        /// <summary>
        /// Indica si se puede usar la base de datos remota
        /// </summary>
        public bool PuedeUsarRemoto => _hayInternet && _hayConexionSupabase;

        /// <summary>
        /// Verifica la conectividad (con caché para no verificar constantemente)
        /// </summary>
        public bool VerificarConectividad(bool forzar = false)
        {
            // Usar caché si no ha pasado el intervalo
            if (!forzar && DateTime.Now - _ultimaVerificacion < _intervaloVerificacion)
            {
                return _hayInternet;
            }

            bool estadoAnterior = _hayInternet;

            // Verificar conexión a internet
            _hayInternet = VerificarInternet();

            // Si hay internet, verificar Supabase
            if (_hayInternet)
            {
                _hayConexionSupabase = VerificarConexionSupabase();
            }
            else
            {
                _hayConexionSupabase = false;
            }

            _ultimaVerificacion = DateTime.Now;

            // Notificar cambios
            if (estadoAnterior != _hayInternet)
            {
                ConectividadCambiada?.Invoke(this, _hayInternet);

                if (_hayInternet)
                {
                    ConexionRestaurada?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ConexionPerdida?.Invoke(this, EventArgs.Empty);
                }
            }

            return _hayInternet;
        }

        /// <summary>
        /// Verifica si hay conexión a internet haciendo ping a varios servidores
        /// </summary>
        private bool VerificarInternet()
        {
            string[] hosts = { "8.8.8.8", "1.1.1.1", "208.67.222.222" }; // Google, Cloudflare, OpenDNS

            foreach (var host in hosts)
            {
                try
                {
                    using var ping = new Ping();
                    var respuesta = ping.Send(host, 2000); // Timeout de 2 segundos
                    if (respuesta.Status == IPStatus.Success)
                    {
                        return true;
                    }
                }
                catch
                {
                    // Continuar con el siguiente host
                }
            }

            return false;
        }

        /// <summary>
        /// Verifica si hay conexión a Supabase
        /// </summary>
        private bool VerificarConexionSupabase()
        {
            try
            {
                using var context = new Data.SandwicheriaDbContext();
                return context.Database.CanConnect();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Verifica la conectividad de forma asíncrona
        /// </summary>
        public async Task<bool> VerificarConectividadAsync(bool forzar = false)
        {
            return await Task.Run(() => VerificarConectividad(forzar));
        }

        /// <summary>
        /// Inicia monitoreo continuo de conectividad en segundo plano
        /// </summary>
        public void IniciarMonitoreo(int intervaloSegundos = 30)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(intervaloSegundos));
                    VerificarConectividad(forzar: true);
                }
            });
        }

        /// <summary>
        /// Fuerza una verificación inmediata
        /// </summary>
        public void ForzarVerificacion()
        {
            VerificarConectividad(forzar: true);
        }

        /// <summary>
        /// Obtiene el estado actual como texto
        /// </summary>
        public string ObtenerEstadoTexto()
        {
            if (!_hayInternet)
                return "🔴 Sin conexión a internet";
            
            if (!_hayConexionSupabase)
                return "🟡 Internet OK, sin conexión a servidor";
            
            return "🟢 Conectado";
        }
    }
}
