import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { FiAlertTriangle, FiLogOut, FiCheck } from 'react-icons/fi';

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
        style={{ maxWidth: 480 }}
      >
        <div className="login-logo">
          <div className="logo-icon" style={{ background: 'rgba(255,107,53,0.15)' }}>
            <FiAlertTriangle size={36} color="#ff6b35" />
          </div>
          <h1 className="logo-title">Periodo de prueba finalizado</h1>
          <p className="logo-subtitle" style={{ maxWidth: 340, margin: '0.5rem auto 0' }}>
            Hola {usuario?.nombreCompleto || 'usuario'}, tu prueba gratis de 7 dias ha terminado.
          </p>
        </div>

        <div style={{ padding: '1rem 2rem 2rem', textAlign: 'center' }}>
          <p style={{ color: '#b0b0b0', marginBottom: '1.5rem', lineHeight: 1.6 }}>
            Tus datos estan seguros. Elegi un plan para seguir usando GastronomiApp:
          </p>

          {/* Plan Mensual */}
          <div style={{ background: 'rgba(255,107,53,0.08)', borderRadius: 12, padding: '1.2rem', marginBottom: '1rem', textAlign: 'left' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.5rem' }}>
              <span style={{ color: '#ff6b35', fontWeight: 700, fontSize: '1.1rem' }}>Mensual (Web)</span>
              <span style={{ color: '#eee', fontWeight: 700 }}>$35 USD/mes</span>
            </div>
            <p style={{ color: '#999', fontSize: '0.85rem', marginBottom: '0.3rem' }}>o 29 USDT/mes</p>
            <div style={{ color: '#aaa', fontSize: '0.85rem' }}>
              <FiCheck color="#4caf50" style={{ marginRight: 4 }} /> Acceso completo via web
            </div>
          </div>

          {/* Plan De por vida */}
          <div style={{ background: 'rgba(0,200,83,0.08)', borderRadius: 12, padding: '1.2rem', marginBottom: '1.5rem', textAlign: 'left', border: '1px solid rgba(0,200,83,0.2)' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.5rem' }}>
              <span style={{ color: '#4caf50', fontWeight: 700, fontSize: '1.1rem' }}>De por vida (App)</span>
              <span style={{ color: '#eee', fontWeight: 700 }}>$380 USD</span>
            </div>
            <p style={{ color: '#999', fontSize: '0.85rem', marginBottom: '0.3rem' }}>o 380 USDT - pago unico</p>
            <div style={{ color: '#aaa', fontSize: '0.85rem' }}>
              <FiCheck color="#4caf50" style={{ marginRight: 4 }} /> App de escritorio para siempre
            </div>
          </div>

          <div style={{ background: 'rgba(255,255,255,0.05)', borderRadius: 12, padding: '1rem', marginBottom: '1.5rem' }}>
            <p style={{ color: '#ff6b35', fontWeight: 600, marginBottom: '0.3rem' }}>Contacto para activacion</p>
            <p style={{ color: '#ccc' }}>admin@gastronomiapp.com</p>
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
