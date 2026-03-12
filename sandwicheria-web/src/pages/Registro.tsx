import { useState, FormEvent } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { authService } from '../services/api';
import { motion } from 'framer-motion';
import { FiUser, FiLock, FiMail, FiPhone, FiHome, FiArrowRight, FiSun, FiMoon } from 'react-icons/fi';
import { useTheme } from '../context/ThemeContext';

export default function Registro() {
  const [form, setForm] = useState({
    nombreUsuario: '',
    nombreCompleto: '',
    email: '',
    contrasena: '',
    confirmarContrasena: '',
    nombreNegocio: '',
    telefono: '',
  });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { loginWithToken } = useAuth();
  const navigate = useNavigate();
  const { theme, toggleTheme } = useTheme();

  const handleChange = (field: string, value: string) => {
    setForm(prev => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');

    if (!form.nombreUsuario.trim() || !form.nombreCompleto.trim() || !form.email.trim() || !form.contrasena || !form.nombreNegocio.trim()) {
      setError('Completa todos los campos obligatorios');
      return;
    }
    if (form.contrasena !== form.confirmarContrasena) {
      setError('Las contraseñas no coinciden');
      return;
    }
    if (form.contrasena.length < 4) {
      setError('La contraseña debe tener al menos 4 caracteres');
      return;
    }

    setLoading(true);
    try {
      const res = await authService.registroNegocio({
        nombreUsuario: form.nombreUsuario,
        nombreCompleto: form.nombreCompleto,
        email: form.email,
        contrasena: form.contrasena,
        nombreNegocio: form.nombreNegocio,
        telefono: form.telefono || undefined,
      });

      const data = res.data;
      if (data.exitoso) {
        // Guardar token y datos, luego redirigir al dashboard
        loginWithToken(data.token, data.usuario, data.tenant);
        navigate('/app');
      } else {
        setError(data.mensaje || 'Error al registrar');
      }
    } catch (err: any) {
      const msg = err.response?.data?.error
        || err.response?.data?.mensaje
        || err.response?.data?.title
        || err.message
        || 'Error al registrar el negocio';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-page">
      <button className="login-theme-toggle" onClick={toggleTheme} title={theme === 'light' ? 'Modo oscuro' : 'Modo claro'}>
        {theme === 'light' ? <FiMoon /> : <FiSun />}
      </button>
      <div className="login-bg-shapes">
        <div className="shape shape-1" />
        <div className="shape shape-2" />
        <div className="shape shape-3" />
      </div>

      <motion.div
        className="login-card registro-card"
        initial={{ opacity: 0, y: 30 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5 }}
      >
        <div className="login-logo">
          <div className="logo-icon">
            <img src="/logo.png" alt="GastronomiApp" className="login-logo-img" />
          </div>
          <h1 className="logo-title">Registra tu negocio</h1>
          <p className="logo-subtitle">7 dias de prueba gratis</p>
        </div>

        <form onSubmit={handleSubmit} className="login-form registro-form">
          <h3 className="form-section-title">Datos del negocio</h3>

          <div className="input-group">
            <FiHome className="input-icon" />
            <input
              type="text"
              placeholder="Nombre del negocio *"
              value={form.nombreNegocio}
              onChange={e => handleChange('nombreNegocio', e.target.value)}
              autoFocus
            />
          </div>

          <div className="input-group">
            <FiPhone className="input-icon" />
            <input
              type="tel"
              placeholder="Telefono (opcional)"
              value={form.telefono}
              onChange={e => handleChange('telefono', e.target.value)}
            />
          </div>

          <h3 className="form-section-title">Tu cuenta</h3>

          <div className="input-group">
            <FiUser className="input-icon" />
            <input
              type="text"
              placeholder="Nombre de usuario *"
              value={form.nombreUsuario}
              onChange={e => handleChange('nombreUsuario', e.target.value)}
            />
          </div>

          <div className="input-group">
            <FiUser className="input-icon" />
            <input
              type="text"
              placeholder="Nombre completo *"
              value={form.nombreCompleto}
              onChange={e => handleChange('nombreCompleto', e.target.value)}
            />
          </div>

          <div className="input-group">
            <FiMail className="input-icon" />
            <input
              type="email"
              placeholder="Email *"
              value={form.email}
              onChange={e => handleChange('email', e.target.value)}
            />
          </div>

          <div className="input-group">
            <FiLock className="input-icon" />
            <input
              type="password"
              placeholder="Contraseña *"
              value={form.contrasena}
              onChange={e => handleChange('contrasena', e.target.value)}
            />
          </div>

          <div className="input-group">
            <FiLock className="input-icon" />
            <input
              type="password"
              placeholder="Confirmar contraseña *"
              value={form.confirmarContrasena}
              onChange={e => handleChange('confirmarContrasena', e.target.value)}
            />
          </div>

          {error && (
            <motion.div
              className="login-error"
              initial={{ opacity: 0, x: -10 }}
              animate={{ opacity: 1, x: 0 }}
            >
              {error}
            </motion.div>
          )}

          <button type="submit" className="btn-login" disabled={loading}>
            <span>{loading ? 'Registrando...' : 'Crear mi negocio'}</span>
            <FiArrowRight />
          </button>
        </form>

        <p className="login-footer">
          Ya tenes cuenta? <Link to="/login" className="link-accent">Inicia sesion</Link>
        </p>
      </motion.div>
    </div>
  );
}
