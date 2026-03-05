using System;
using System.Windows;
using System.Windows.Controls;
using FontAwesome.Sharp;
using SandwicheriaWalterio.Data;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Views
{
    public partial class UsuarioFormWindow : Window
    {
        private readonly UsuarioRepository _usuarioRepository;
        private readonly Usuario _usuarioEditar;
        private readonly bool _esNuevo;

        public UsuarioFormWindow(Usuario usuario)
        {
            InitializeComponent();
            _usuarioRepository = new UsuarioRepository();
            _usuarioEditar = usuario;
            _esNuevo = usuario == null;

            ConfigurarVentana();
        }

        private void ConfigurarVentana()
        {
            if (_esNuevo)
            {
                // Modo creación
                txtTitulo.Text = "NUEVO USUARIO";
                iconTitulo.Icon = IconChar.UserPlus;
                this.Title = "Nuevo Usuario";
                cmbRol.SelectedIndex = 1; // Empleado por defecto
            }
            else
            {
                // Modo edición
                txtTitulo.Text = "EDITAR USUARIO";
                iconTitulo.Icon = IconChar.UserEdit;
                this.Title = "Editar Usuario";

                // Cargar datos del usuario
                txtNombreUsuario.Text = _usuarioEditar.NombreUsuario;
                chkActivo.IsChecked = _usuarioEditar.Activo;

                // Seleccionar rol
                foreach (ComboBoxItem item in cmbRol.Items)
                {
                    if (item.Tag.ToString() == _usuarioEditar.Rol)
                    {
                        cmbRol.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar campos obligatorios
                if (string.IsNullOrWhiteSpace(txtNombreUsuario.Text))
                {
                    MessageBox.Show("El nombre de usuario es obligatorio", "Validación",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNombreUsuario.Focus();
                    return;
                }

                if (cmbRol.SelectedItem == null)
                {
                    MessageBox.Show("Seleccione un rol", "Validación",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    cmbRol.Focus();
                    return;
                }

                // Verificar si el nombre de usuario ya existe
                int? exceptoId = _esNuevo ? null : _usuarioEditar.UsuarioID;
                if (_usuarioRepository.ExisteNombreUsuario(txtNombreUsuario.Text.Trim(), exceptoId))
                {
                    MessageBox.Show("El nombre de usuario ya existe.\nElija otro nombre.", "Validación",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNombreUsuario.Focus();
                    return;
                }

                // Crear o actualizar usuario
                var rol = ((ComboBoxItem)cmbRol.SelectedItem).Tag.ToString();

                if (_esNuevo)
                {
                    var nuevoUsuario = new Usuario
                    {
                        NombreUsuario = txtNombreUsuario.Text.Trim(),
                        NombreCompleto = txtNombreUsuario.Text.Trim(), // Usar nombre de usuario como nombre completo
                        Rol = rol,
                        Activo = chkActivo.IsChecked ?? true
                    };

                    // La contraseña por defecto es el nombre de usuario
                    _usuarioRepository.Crear(nuevoUsuario, txtNombreUsuario.Text.Trim());
                }
                else
                {
                    _usuarioEditar.NombreUsuario = txtNombreUsuario.Text.Trim();
                    _usuarioEditar.NombreCompleto = txtNombreUsuario.Text.Trim();
                    _usuarioEditar.Rol = rol;
                    _usuarioEditar.Activo = chkActivo.IsChecked ?? true;

                    _usuarioRepository.Actualizar(_usuarioEditar);
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
