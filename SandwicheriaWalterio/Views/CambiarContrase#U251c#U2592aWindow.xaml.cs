using System;
using System.Windows;
using SandwicheriaWalterio.Data;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Views
{
    public partial class CambiarContraseñaWindow : Window
    {
        private readonly UsuarioRepository _usuarioRepository;
        private readonly Usuario _usuario;

        public CambiarContraseñaWindow(Usuario usuario)
        {
            InitializeComponent();
            _usuarioRepository = new UsuarioRepository();
            _usuario = usuario;

            txtUsuario.Text = $"Usuario: {usuario.NombreCompleto} ({usuario.NombreUsuario})";
        }

        private void BtnCambiar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(txtNuevaContraseña.Password))
                {
                    MessageBox.Show("Ingrese la nueva contraseña", "Validación",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNuevaContraseña.Focus();
                    return;
                }

                if (txtNuevaContraseña.Password.Length < 4)
                {
                    MessageBox.Show("La contraseña debe tener al menos 4 caracteres", "Validación",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNuevaContraseña.Focus();
                    return;
                }

                if (txtNuevaContraseña.Password != txtConfirmarContraseña.Password)
                {
                    MessageBox.Show("Las contraseñas no coinciden", "Validación",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtConfirmarContraseña.Focus();
                    return;
                }

                // Cambiar contraseña
                bool resultado = _usuarioRepository.CambiarContraseña(_usuario.UsuarioID, txtNuevaContraseña.Password);

                if (resultado)
                {
                    MessageBox.Show("Contraseña cambiada exitosamente", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("No se pudo cambiar la contraseña", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
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
