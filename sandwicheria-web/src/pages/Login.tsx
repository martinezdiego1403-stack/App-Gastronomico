import { useState, FormEvent } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { motion } from 'framer-motion';
import { FiUser, FiLock, FiLogIn } from 'react-icons/fi';

export default function Login() {
  const [nombreUsuario, setNombreUsuario] = useState('');
  const [contrasena, setContrasena] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!nombreUsuario.trim()) {
      setError('Ingresa tu nombre de usuario');
      return;
    }
    setError('');
    setLoading(true);
    try {
      await login(nombreUsuario, contrasena);
      navigate('/');
    } catch (err: any) {
      setError(err.message || err.response?.data?.error || 'Error al iniciar sesion');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-page">
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
            <span role="img" aria-label="sandwich">&#x1F96A;</span>
          </div>
          <h1 className="logo-title">La Sandwicheria</h1>
          <p className="logo-subtitle">Sistema de Gestion</p>
        </div>

        <form onSubmit={handleSubmit} className="login-form">
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

        <p className="login-footer">
          No tenes cuenta? <Link to="/registro" className="link-accent">Registra tu negocio</Link>
        </p>
      </motion.div>
    </div>
  );
}
