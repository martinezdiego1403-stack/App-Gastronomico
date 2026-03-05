using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SandwicheriaWalterio.Data;
using SandwicheriaWalterio.Helpers;
using SandwicheriaWalterio.Models;
using SandwicheriaWalterio.Services;

namespace SandwicheriaWalterio.ViewModels
{
    /// <summary>
    /// ViewModel para la gestión de usuarios.
    /// 
    /// 🎓 EXPLICACIÓN:
    /// Este ViewModel maneja toda la lógica de la pantalla de usuarios:
    /// - Lista de usuarios
    /// - Crear/editar usuarios
    /// - Activar/desactivar
    /// - Cambiar contraseñas
    /// - Ver historial de accesos
    /// - Eliminar ventas por período
    /// </summary>
    public class UsuariosViewModel : ViewModelBase
    {
        private readonly UsuarioRepository _usuarioRepository;
        private readonly VentaRepository _ventaRepository;
        private readonly CajaRepository _cajaRepository;

        // Campos privados
        private ObservableCollection<Usuario> _usuarios;
        private ObservableCollection<HistorialAcceso> _historialAccesos;
        private Usuario _usuarioSeleccionado;
        private string _filtro;
        private bool _mostrarSoloActivos = true;

        // Propiedades para binding
        public ObservableCollection<Usuario> Usuarios
        {
            get => _usuarios;
            set => SetProperty(ref _usuarios, value);
        }

        public ObservableCollection<HistorialAcceso> HistorialAccesos
        {
            get => _historialAccesos;
            set => SetProperty(ref _historialAccesos, value);
        }

        public Usuario UsuarioSeleccionado
        {
            get => _usuarioSeleccionado;
            set
            {
                SetProperty(ref _usuarioSeleccionado, value);
                // Notificar que los comandos pueden cambiar su estado
                OnPropertyChanged(nameof(PuedeEditar));
                OnPropertyChanged(nameof(PuedeCambiarEstado));
            }
        }

        public string Filtro
        {
            get => _filtro;
            set
            {
                SetProperty(ref _filtro, value);
                AplicarFiltro();
            }
        }

        public bool MostrarSoloActivos
        {
            get => _mostrarSoloActivos;
            set
            {
                SetProperty(ref _mostrarSoloActivos, value);
                CargarUsuarios();
            }
        }

        // Lista de todos los usuarios (sin filtrar)
        private List<Usuario> _todosLosUsuarios;

        // Propiedades calculadas
        public bool PuedeEditar => UsuarioSeleccionado != null;
        public bool PuedeCambiarEstado => UsuarioSeleccionado != null && 
            UsuarioSeleccionado.UsuarioID != SessionService.Instance.UsuarioActual?.UsuarioID;

        // Estadísticas
        public int TotalUsuarios => _todosLosUsuarios?.Count ?? 0;
        public int UsuariosActivos => _todosLosUsuarios?.Count(u => u.Activo) ?? 0;
        public int UsuariosInactivos => _todosLosUsuarios?.Count(u => !u.Activo) ?? 0;

        // Comandos de usuarios
        public ICommand NuevoUsuarioCommand { get; }
        public ICommand EditarUsuarioCommand { get; }
        public ICommand EliminarUsuarioCommand { get; }
        public ICommand CambiarEstadoCommand { get; }
        public ICommand CambiarContraseñaCommand { get; }
        public ICommand DesbloquearCommand { get; }
        public ICommand ActualizarCommand { get; }

        // Comandos de eliminar ventas
        public ICommand EliminarVentasDiaCommand { get; }
        public ICommand EliminarVentasSemanaCommand { get; }
        public ICommand EliminarVentasMesCommand { get; }
        public ICommand EliminarTodasVentasCommand { get; }

        // Constructor
        public UsuariosViewModel()
        {
            _usuarioRepository = new UsuarioRepository();
            _ventaRepository = new VentaRepository();
            _cajaRepository = new CajaRepository();
            Usuarios = new ObservableCollection<Usuario>();
            HistorialAccesos = new ObservableCollection<HistorialAcceso>();

            // Inicializar comandos de usuarios
            NuevoUsuarioCommand = new RelayCommand(_ => NuevoUsuario());
            EditarUsuarioCommand = new RelayCommand(param => EditarUsuario(param));
            EliminarUsuarioCommand = new RelayCommand(param => EliminarUsuario(param));
            CambiarEstadoCommand = new RelayCommand(_ => CambiarEstado(), _ => PuedeCambiarEstado);
            CambiarContraseñaCommand = new RelayCommand(_ => CambiarContraseña(), _ => PuedeEditar);
            DesbloquearCommand = new RelayCommand(_ => DesbloquearUsuario(), _ => PuedeEditar);
            ActualizarCommand = new RelayCommand(_ => Actualizar());

            // Inicializar comandos de eliminar ventas
            EliminarVentasDiaCommand = new RelayCommand(_ => EliminarVentasDelDia());
            EliminarVentasSemanaCommand = new RelayCommand(_ => EliminarVentasDeLaSemana());
            EliminarVentasMesCommand = new RelayCommand(_ => EliminarVentasDelMes());
            EliminarTodasVentasCommand = new RelayCommand(_ => EliminarTodasLasVentas());

            // Cargar datos
            CargarUsuarios();
            CargarHistorial();
        }

        // ========================================
        // MÉTODOS DE CARGA
        // ========================================

        private void CargarUsuarios()
        {
            try
            {
                _todosLosUsuarios = _usuarioRepository.ObtenerTodos();

                // Aplicar filtro de activos
                var usuariosFiltrados = MostrarSoloActivos
                    ? _todosLosUsuarios.Where(u => u.Activo).ToList()
                    : _todosLosUsuarios;

                Usuarios = new ObservableCollection<Usuario>(usuariosFiltrados);

                // Actualizar estadísticas
                OnPropertyChanged(nameof(TotalUsuarios));
                OnPropertyChanged(nameof(UsuariosActivos));
                OnPropertyChanged(nameof(UsuariosInactivos));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar usuarios: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarHistorial()
        {
            try
            {
                var historial = _usuarioRepository.ObtenerHistorialAccesos(100);
                HistorialAccesos = new ObservableCollection<HistorialAcceso>(historial);
            }
            catch (Exception ex)
            {
                // Si falla cargar historial, no es crítico
                System.Diagnostics.Debug.WriteLine($"Error al cargar historial: {ex.Message}");
            }
        }

        private void AplicarFiltro()
        {
            if (_todosLosUsuarios == null) return;

            var usuariosFiltrados = _todosLosUsuarios.AsEnumerable();

            // Filtrar por activos
            if (MostrarSoloActivos)
            {
                usuariosFiltrados = usuariosFiltrados.Where(u => u.Activo);
            }

            // Filtrar por texto
            if (!string.IsNullOrWhiteSpace(Filtro))
            {
                string filtroLower = Filtro.ToLower();
                usuariosFiltrados = usuariosFiltrados.Where(u =>
                    u.NombreUsuario.ToLower().Contains(filtroLower) ||
                    u.NombreCompleto.ToLower().Contains(filtroLower) ||
                    (u.Email?.ToLower().Contains(filtroLower) ?? false));
            }

            Usuarios = new ObservableCollection<Usuario>(usuariosFiltrados);
        }

        // ========================================
        // MÉTODOS DE COMANDOS - USUARIOS
        // ========================================

        private void NuevoUsuario()
        {
            var ventana = new Views.UsuarioFormWindow(null);
            if (ventana.ShowDialog() == true)
            {
                CargarUsuarios();
                MessageBox.Show("Usuario creado exitosamente", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EditarUsuario(object parameter)
        {
            var usuario = parameter as Usuario ?? UsuarioSeleccionado;
            if (usuario == null) return;

            var ventana = new Views.UsuarioFormWindow(usuario);
            if (ventana.ShowDialog() == true)
            {
                CargarUsuarios();
                MessageBox.Show("Usuario actualizado exitosamente", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EliminarUsuario(object parameter)
        {
            var usuario = parameter as Usuario ?? UsuarioSeleccionado;
            if (usuario == null) return;

            // No permitir eliminarse a sí mismo
            if (usuario.UsuarioID == SessionService.Instance.UsuarioActual?.UsuarioID)
            {
                MessageBox.Show("No puedes eliminar tu propio usuario",
                    "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var resultado = MessageBox.Show(
                $"¿Está seguro de ELIMINAR al usuario '{usuario.NombreCompleto}'?\n\n" +
                "⚠️ Esta acción desactivará permanentemente al usuario.",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    // Desactivar usuario (soft delete)
                    _usuarioRepository.CambiarEstado(usuario.UsuarioID, false);
                    CargarUsuarios();

                    MessageBox.Show("Usuario eliminado exitosamente",
                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar usuario: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CambiarEstado()
        {
            if (UsuarioSeleccionado == null) return;

            // No permitir desactivarse a sí mismo
            if (UsuarioSeleccionado.UsuarioID == SessionService.Instance.UsuarioActual?.UsuarioID)
            {
                MessageBox.Show("No puedes desactivar tu propio usuario",
                    "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string accion = UsuarioSeleccionado.Activo ? "desactivar" : "activar";
            var resultado = MessageBox.Show(
                $"¿Está seguro de {accion} al usuario '{UsuarioSeleccionado.NombreCompleto}'?",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    bool nuevoEstado = !UsuarioSeleccionado.Activo;
                    _usuarioRepository.CambiarEstado(UsuarioSeleccionado.UsuarioID, nuevoEstado);
                    CargarUsuarios();

                    MessageBox.Show($"Usuario {(nuevoEstado ? "activado" : "desactivado")} exitosamente",
                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cambiar estado: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CambiarContraseña()
        {
            if (UsuarioSeleccionado == null) return;

            var ventana = new Views.CambiarContraseñaWindow(UsuarioSeleccionado);
            ventana.ShowDialog();
        }

        private void DesbloquearUsuario()
        {
            if (UsuarioSeleccionado == null) return;

            if (!UsuarioSeleccionado.EstaBloqueado)
            {
                MessageBox.Show("Este usuario no está bloqueado",
                    "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                _usuarioRepository.DesbloquearUsuario(UsuarioSeleccionado.UsuarioID);
                CargarUsuarios();
                MessageBox.Show("Usuario desbloqueado exitosamente",
                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al desbloquear: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Actualizar()
        {
            CargarUsuarios();
            CargarHistorial();
        }

        // ========================================
        // MÉTODOS DE COMANDOS - ELIMINAR VENTAS
        // ========================================

        private void EliminarVentasDelDia()
        {
            var resultado = MessageBox.Show(
                "¿Está seguro de eliminar TODAS las ventas de HOY?\n\n" +
                "⚠️ Esta acción no se puede deshacer.",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    int cantidad = _ventaRepository.EliminarVentasDelDia();
                    MessageBox.Show($"✅ Se eliminaron {cantidad} ventas del día",
                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar ventas: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EliminarVentasDeLaSemana()
        {
            var resultado = MessageBox.Show(
                "¿Está seguro de eliminar TODAS las ventas de ESTA SEMANA?\n\n" +
                "⚠️ Esta acción no se puede deshacer.",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    int cantidad = _ventaRepository.EliminarVentasDeLaSemana();
                    MessageBox.Show($"✅ Se eliminaron {cantidad} ventas de la semana",
                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar ventas: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EliminarVentasDelMes()
        {
            var resultado = MessageBox.Show(
                "¿Está seguro de eliminar TODAS las ventas de ESTE MES?\n\n" +
                "⚠️ Esta acción no se puede deshacer.",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    int cantidad = _ventaRepository.EliminarVentasDelMes();
                    MessageBox.Show($"✅ Se eliminaron {cantidad} ventas del mes",
                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar ventas: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EliminarTodasLasVentas()
        {
            var resultado = MessageBox.Show(
                "⚠️ ¡PELIGRO!\n\n" +
                "Está a punto de eliminar TODAS las ventas del sistema.\n" +
                "Esta acción eliminará TODO el historial de ventas.\n\n" +
                "También se eliminarán TODAS las cajas registradas,\n" +
                "reiniciando el conteo desde Caja #1.\n\n" +
                "¿Está COMPLETAMENTE seguro?",
                "⚠️ CONFIRMAR ELIMINACIÓN TOTAL",
                MessageBoxButton.YesNo,
                MessageBoxImage.Stop);

            if (resultado == MessageBoxResult.Yes)
            {
                // Segunda confirmación
                var resultado2 = MessageBox.Show(
                    "ÚLTIMA CONFIRMACIÓN\n\n" +
                    "Se eliminarán:\n" +
                    "• Todas las ventas\n" +
                    "• Todas las cajas\n\n" +
                    "El conteo de cajas se reiniciará a #1.\n\n" +
                    "¿Proceder con la eliminación?",
                    "Confirmación Final",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Stop);

                if (resultado2 == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Primero eliminar ventas
                        int cantidadVentas = _ventaRepository.EliminarTodasLasVentas();
                        
                        // Luego eliminar cajas (esto reinicia el conteo)
                        int cantidadCajas = _cajaRepository.EliminarTodasLasCajas();
                        
                        // Limpiar la caja actual de la sesión
                        SessionService.Instance.EstablecerCajaActual(null);
                        
                        MessageBox.Show(
                            $"✅ Eliminación completada:\n\n" +
                            $"• {cantidadVentas} ventas eliminadas\n" +
                            $"• {cantidadCajas} cajas eliminadas\n\n" +
                            $"El conteo de cajas se ha reiniciado.\n" +
                            $"La próxima caja será Caja #1.",
                            "Eliminación Completada", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al eliminar: {ex.Message}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
