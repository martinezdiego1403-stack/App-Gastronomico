using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Services
{
    public class SessionService
    {
        private static SessionService _instance;
        private static readonly object _lock = new object();

        public Usuario UsuarioActual { get; private set; }
        public Caja CajaActual { get; private set; }

        private SessionService() { }

        public static SessionService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new SessionService();
                        }
                    }
                }
                return _instance;
            }
        }

        public void IniciarSesion(Usuario usuario)
        {
            UsuarioActual = usuario;
        }

        public void EstablecerCajaActual(Caja caja)
        {
            CajaActual = caja;
        }

        public void CerrarSesion()
        {
            UsuarioActual = null;
            CajaActual = null;
        }

        public bool HaySesionActiva => UsuarioActual != null;
        public bool HayCajaAbierta => CajaActual != null && CajaActual.EstaAbierta;
    }
}
