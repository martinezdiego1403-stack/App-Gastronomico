import { useState, FormEvent } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { authService } from '../services/api';
import { motion, AnimatePresence } from 'framer-motion';
import { FiUser, FiLock, FiLogIn, FiSun, FiMoon, FiHome, FiArrowLeft } from 'react-icons/fi';
import { useTheme } from '../context/ThemeContext';

type TipoUsuario = null | 'dueno' | 'empleado';

export default function Login() {
  const [tipoUsuario, setTipoUsuario] = useState<TipoUsuario>(null);
  const [nombreNegocio, setNombreNegocio] = useState('');
  const [nombreUsuario, setNombreUsuario] = useState('');
  const [contrasena, setContrasena] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { login, loginWithToken } = useAuth();
  const navigate = useNavigate();
  const { theme, toggleTheme } = useTheme();

  const handleLoginDueno = async (e: FormEvent) => {
    e.preventDefault();
    if (!nombreUsuario.trim()) {
      setError('Ingresa tu nombre de usuario');
      return;
    }
    setError('');
    setLoading(true);
    try {
      await login(nombreUsuario, contrasena);
      navigate('/app');
    } catch (err: any) {
      setError(err.message || err.response?.data?.error || 'Error al iniciar sesion');
    } finally {
      setLoading(false);
    }
  };

  const handleLoginEmpleado = async (e: FormEvent) => {
    e.preventDefault();
    if (!nombreNegocio.trim()) {
      setError('Ingresa el nombre del local');
      return;
    }
    if (!nombreUsuario.trim()) {
      setError('Ingresa tu nombre de usuario');
      return;
    }
    setError('');
    setLoading(true);
    try {
      const res = await authService.loginEmpleado(nombreUsuario, nombreNegocio);
      const data = res.data;
      if (data.exitoso) {
        loginWithToken(data.token, data.usuario, data.tenant);
        navigate('/app');
      } else {
        setError(data.mensaje || 'Error al iniciar sesion');
      }
    } catch (err: any) {
      setError(err.response?.data?.error || err.message || 'Error al iniciar sesion');
    } finally {
      setLoading(false);
    }
  };

  const volver = () => {
    setTipoUsuario(null);
    setError('');
    setNombreUsuario('');
    setContrasena('');
    setNombreNegocio('');
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
        className="login-card"
        initial={{ opacity: 0, y: 30 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5 }}
      >
        {/* Logo */}
        <div className="login-logo">
          <div className="logo-icon">
            <img src="/logo.png" alt="GastronomiApp" className="login-logo-img" />
          </div>
          <h1 className="logo-title">GastronomiApp</h1>
          <p className="logo-subtitle">Sistema de Gestion</p>
        </div>

        <AnimatePresence mode="wait">
          {/* Paso 1: Elegir tipo de usuario */}
          {tipoUsuario === null && (
            <motion.div
              key="selector"
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -20 }}
              className="login-selector"
            >
              <p className="login-selector-title">Como queres ingresar?</p>
              <div className="login-role-buttons">
                <button
                  className="login-role-btn dueno"
                  onClick={() => setTipoUsuario('dueno')}
                >
                  <FiUser className="login-role-icon" />
                  <span className="login-role-name">Soy Dueño / Admin</span>
                  <span className="login-role-desc">Acceso completo con contraseña</span>
                </button>
                <button
                  className="login-role-btn empleado"
                  onClick={() => setTipoUsuario('empleado')}
                >
                  <FiHome className="login-role-icon" />
                  <span className="login-role-name">Soy Empleado</span>
                  <span className="login-role-desc">Acceso rapido sin contraseña</span>
                </button>
              </div>
            </motion.div>
          )}

          {/* Paso 2a: Login Dueño/Admin */}
          {tipoUsuario === 'dueno' && (
            <motion.div
              key="dueno"
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -20 }}
            >
              <button className="login-back-btn" onClick={volver}>
                <FiArrowLeft /> Volver
              </button>
              <form onSubmit={handleLoginDueno} className="login-form">
                <div className="input-group">
                  <FiUser className="input-icon" />
                  <input
                    type="text"
                    placeholder="Nombre de usuario"
                    value={nombreUsuario}
                    onChange={e => setNombreUsuario(e.target.value)}
                    autoFocus
                  />
                </div>

                <div className="input-group">
                  <FiLock className="input-icon" />
                  <input
                    type="password"
                    placeholder="Contrasena"
                    value={contrasena}
                    onChange={e => setContrasena(e.target.value)}
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
                  <FiLogIn />
                  <span>{loading ? 'Ingresando...' : 'Ingresar'}</span>
                </button>
              </form>
            </motion.div>
          )}

          {/* Paso 2b: Login Empleado */}
          {tipoUsuario === 'empleado' && (
            <motion.div
              key="empleado"
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -20 }}
            >
              <button className="login-back-btn" onClick={volver}>
                <FiArrowLeft /> Volver
              </button>
              <form onSubmit={handleLoginEmpleado} className="login-form">
                <div className="input-group">
                  <FiHome className="input-icon" />
                  <input
                    type="text"
                    placeholder="Nombre del local"
                    value={nombreNegocio}
                    onChange={e => setNombreNegocio(e.target.value)}
                    autoFocus
                  />
                </div>

                <div className="input-group">
                  <FiUser className="input-icon" />
                  <input
                    type="text"
                    placeholder="Tu nombre de usuario"
                    value={nombreUsuario}
                    onChange={e => setNombreUsuario(e.target.value)}
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
                  <FiLogIn />
                  <span>{loading ? 'Ingresando...' : 'Ingresar'}</span>
                </button>
              </form>
            </motion.div>
          )}
        </AnimatePresence>

        <p className="login-footer">
          No tenes cuenta? <Link to="/registro" className="link-accent">Registra tu negocio</Link>
        </p>
      </motion.div>
    </div>
  );
}
