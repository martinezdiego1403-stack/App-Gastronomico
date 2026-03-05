//------------------------------------------------------------------------------
// Este código se genera automáticamente para mantener compatibilidad con Settings
//------------------------------------------------------------------------------

namespace SandwicheriaWalterio.Properties
{
    /// <summary>
    /// Configuraciones de la aplicación.
    /// Permite guardar preferencias del usuario como el tema seleccionado.
    /// </summary>
    internal sealed partial class Settings : System.Configuration.ApplicationSettingsBase
    {
        private static Settings defaultInstance = new Settings();

        public static Settings Default
        {
            get { return defaultInstance; }
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Configuration.DefaultSettingValueAttribute("False")]
        public bool DarkTheme
        {
            get { return (bool)this["DarkTheme"]; }
            set { this["DarkTheme"] = value; }
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Configuration.DefaultSettingValueAttribute("")]
        public string LastUser
        {
            get { return (string)this["LastUser"]; }
            set { this["LastUser"] = value; }
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Configuration.DefaultSettingValueAttribute("")]
        public string WhatsAppNumero
        {
            get { return (string)this["WhatsAppNumero"]; }
            set { this["WhatsAppNumero"] = value; }
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Configuration.DefaultSettingValueAttribute("False")]
        public bool WhatsAppHabilitado
        {
            get { return (bool)this["WhatsAppHabilitado"]; }
            set { this["WhatsAppHabilitado"] = value; }
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Configuration.DefaultSettingValueAttribute("admin123")]
        public string ContrasenaMercaderia
        {
            get { return (string)this["ContrasenaMercaderia"]; }
            set { this["ContrasenaMercaderia"] = value; }
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Configuration.DefaultSettingValueAttribute("coquena")]
        public string ContrasenaMenu
        {
            get { return (string)this["ContrasenaMenu"]; }
            set { this["ContrasenaMenu"] = value; }
        }
    }
}
