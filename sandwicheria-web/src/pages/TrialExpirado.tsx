import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { FiAlertTriangle, FiLogOut } from 'react-icons/fi';

export default function TrialExpirado() {
  const { logout, usuario } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/');
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
        <div className="login-logo">
          <div className="logo-icon" style={{ background: 'rgba(255,107,53,0.15)' }}>
            <FiAlertTriangle size={36} color="#ff6b35" />
          </div>
          <h1 className="logo-title">Periodo de prueba finalizado</h1>
          <p className="logo-subtitle" style={{ maxWidth: 320, margin: '0.5rem auto 0' }}>
            Hola {usuario?.nombreCompleto || 'usuario'}, tu prueba gratis de 7 dias ha terminado.
          </p>
        </div>

        <div style={{ padding: '1.5rem 2rem', textAlign: 'center' }}>
          <p style={{ color: '#b0b0b0', marginBottom: '1.5rem', lineHeight: 1.6 }}>
            Para seguir usando el sistema, contactanos para activar un plan pago.
            Tus datos estan seguros y se mantendran cuando actives tu cuenta.
          </p>

          <div style={{ background: 'rgba(255,107,53,0.08)', borderRadius: 12, padding: '1.2rem', marginBottom: '1.5rem' }}>
            <p style={{ color: '#ff6b35', fontWeight: 600, marginBottom: '0.3rem' }}>Contacto para activacion</p>
            <p style={{ color: '#ccc' }}>admin@lasandwicheria.com</p>
          </div>

          <button className="btn-login" onClick={handleLogout} style={{ background: '#333' }}>
            <FiLogOut />
            <span>Volver al inicio</span>
          </button>
        </div>
      </motion.div>
    </div>
  );
}
