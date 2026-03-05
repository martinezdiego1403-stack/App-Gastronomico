using System;
using System.Net;
using System.Net.Mail;
using System.IO;
using System.Windows;

namespace SandwicheriaWalterio.Services
{
    /// <summary>
    /// Servicio para enviar emails con reportes adjuntos.
    /// 
    /// 🎓 EXPLICACIÓN PARA PRINCIPIANTES:
    /// 
    /// Este servicio usa SMTP (Simple Mail Transfer Protocol) para enviar emails.
    /// SMTP es como el "cartero" de internet para los correos electrónicos.
    /// 
    /// Necesitas configurar:
    /// 1. Un servidor SMTP (Gmail, Outlook, etc.)
    /// 2. Credenciales (email y contraseña)
    /// 3. El destinatario y el archivo a enviar
    /// </summary>
    public class EmailService
    {
        // Configuración del servidor SMTP
        // Puedes cambiar estos valores según tu proveedor de email
        
        private string _smtpServer = "smtp.gmail.com";  // Servidor de Gmail
        private int _smtpPort = 587;                      // Puerto para TLS
        private string _emailRemitente = "";              // Tu email
        private string _passwordRemitente = "";           // Tu contraseña de aplicación
        private string _nombreRemitente = "Sandwichería Walterio";

        /// <summary>
        /// Configura las credenciales del email.
        /// </summary>
        public void ConfigurarCredenciales(string email, string password, string servidor = "smtp.gmail.com", int puerto = 587)
        {
            _emailRemitente = email;
            _passwordRemitente = password;
            _smtpServer = servidor;
            _smtpPort = puerto;
        }

        /// <summary>
        /// Envía un email con un archivo adjunto.
        /// </summary>
        /// <param name="destinatario">Email del destinatario</param>
        /// <param name="asunto">Asunto del email</param>
        /// <param name="cuerpo">Contenido del email (puede ser HTML)</param>
        /// <param name="rutaAdjunto">Ruta del archivo a adjuntar (opcional)</param>
        /// <returns>True si se envió correctamente</returns>
        public bool EnviarEmail(string destinatario, string asunto, string cuerpo, string rutaAdjunto = null)
        {
            try
            {
                // Validar que tengamos credenciales
                if (string.IsNullOrEmpty(_emailRemitente) || string.IsNullOrEmpty(_passwordRemitente))
                {
                    throw new Exception("No se han configurado las credenciales de email.\n\n" +
                        "Ve a Configuración para establecer tu email y contraseña.");
                }

                // Crear el mensaje
                var mensaje = new MailMessage
                {
                    From = new MailAddress(_emailRemitente, _nombreRemitente),
                    Subject = asunto,
                    Body = cuerpo,
                    IsBodyHtml = true  // Permite HTML en el cuerpo
                };

                // Agregar destinatario
                mensaje.To.Add(destinatario);

                // Agregar archivo adjunto si existe
                if (!string.IsNullOrEmpty(rutaAdjunto) && File.Exists(rutaAdjunto))
                {
                    var adjunto = new Attachment(rutaAdjunto);
                    mensaje.Attachments.Add(adjunto);
                }

                // Configurar el cliente SMTP
                using (var smtp = new SmtpClient(_smtpServer, _smtpPort))
                {
                    smtp.EnableSsl = true;  // Usar conexión segura
                    smtp.Credentials = new NetworkCredential(_emailRemitente, _passwordRemitente);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Timeout = 30000;  // 30 segundos de timeout

                    // Enviar el email
                    smtp.Send(mensaje);
                }

                return true;
            }
            catch (SmtpException smtpEx)
            {
                // Errores específicos de SMTP
                string mensajeError = smtpEx.StatusCode switch
                {
                    SmtpStatusCode.MailboxBusy => "El servidor está ocupado. Intenta de nuevo.",
                    SmtpStatusCode.MailboxUnavailable => "El email de destino no existe.",
                    SmtpStatusCode.ClientNotPermitted => "No tienes permiso para enviar emails. Verifica tu configuración.",
                    _ => $"Error SMTP: {smtpEx.Message}"
                };
                throw new Exception(mensajeError);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al enviar email: {ex.Message}");
            }
        }

        /// <summary>
        /// Genera el cuerpo HTML del email para el reporte.
        /// </summary>
        public string GenerarCuerpoReporte(DateTime fechaInicio, DateTime fechaFin, decimal totalVentas, int cantidadVentas)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #f5f5f5; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #2C3E50, #3498DB); color: white; padding: 30px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 24px; }}
        .header p {{ margin: 10px 0 0 0; opacity: 0.9; }}
        .content {{ padding: 30px; }}
        .kpi-grid {{ display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin: 20px 0; }}
        .kpi-card {{ background: #f8f9fa; border-radius: 8px; padding: 20px; text-align: center; border-left: 4px solid #3498DB; }}
        .kpi-card.green {{ border-left-color: #27AE60; }}
        .kpi-card h3 {{ margin: 0 0 10px 0; color: #7f8c8d; font-size: 12px; text-transform: uppercase; }}
        .kpi-card p {{ margin: 0; font-size: 28px; font-weight: bold; color: #2C3E50; }}
        .footer {{ background: #f8f9fa; padding: 20px; text-align: center; color: #7f8c8d; font-size: 12px; }}
        .btn {{ display: inline-block; background: #27AE60; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🥪 SANDWICHERÍA WALTERIO</h1>
            <p>Reporte de Ventas</p>
        </div>
        <div class='content'>
            <p>Hola,</p>
            <p>Adjunto encontrarás el reporte de ventas del período <strong>{fechaInicio:dd/MM/yyyy}</strong> al <strong>{fechaFin:dd/MM/yyyy}</strong>.</p>
            
            <div class='kpi-grid'>
                <div class='kpi-card green'>
                    <h3>💰 Total Ventas</h3>
                    <p>${totalVentas:N0}</p>
                </div>
                <div class='kpi-card'>
                    <h3>🧾 Cantidad</h3>
                    <p>{cantidadVentas}</p>
                </div>
            </div>
            
            <p>El archivo Excel adjunto contiene:</p>
            <ul>
                <li>📊 Resumen ejecutivo con KPIs</li>
                <li>📅 Ventas detalladas por día</li>
                <li>🏆 Top 10 productos más vendidos</li>
                <li>💳 Análisis por método de pago</li>
                <li>🕐 Horas pico de ventas</li>
            </ul>
            
            <p style='color: #7f8c8d; font-size: 12px; margin-top: 30px;'>
                Este reporte fue generado automáticamente el {DateTime.Now:dd/MM/yyyy} a las {DateTime.Now:HH:mm}.
            </p>
        </div>
        <div class='footer'>
            <p>Sistema de Gestión - Sandwichería Walterio</p>
            <p>© {DateTime.Now.Year} Todos los derechos reservados</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
