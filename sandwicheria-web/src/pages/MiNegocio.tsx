import { useState, useEffect } from 'react';
import { tenantSettingsService } from '../services/api';
import { useAuth } from '../context/AuthContext';
import { FiSave, FiHome, FiMail, FiPhone, FiClock } from 'react-icons/fi';

interface TenantInfo {
  tenantId: string;
  nombreNegocio: string;
  plan: string;
  activo: boolean;
  diasRestantesTrial: number;
  trialExpirado: boolean;
}

interface PlanInfo {
  plan: string;
  esTrial: boolean;
  trialExpirado: boolean;
  diasRestantesTrial: number;
  fechaExpiracionTrial: string | null;
  fechaCreacion: string;
}

export default function MiNegocio() {
  const { tenant, updateTenant } = useAuth();
  const [nombreNegocio, setNombreNegocio] = useState('');
  const [emailContacto, setEmailContacto] = useState('');
  const [telefono, setTelefono] = useState('');
  const [planInfo, setPlanInfo] = useState<PlanInfo | null>(null);
  const [guardando, setGuardando] = useState(false);
  const [mensaje, setMensaje] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    cargarDatos();
  }, []);

  const cargarDatos = async () => {
    try {
      const [negocioRes, planRes] = await Promise.all([
        tenantSettingsService.obtenerMiNegocio(),
        tenantSettingsService.obtenerPlan(),
      ]);
      const negocio = negocioRes.data;
      setNombreNegocio(negocio.nombreNegocio || '');
      setPlanInfo(planRes.data);
    } catch (err) {
      console.error('Error cargando datos del negocio', err);
    } finally {
      setLoading(false);
    }
  };

  const guardar = async () => {
    if (!nombreNegocio.trim()) return;
    setGuardando(true);
    setMensaje('');
    try {
      await tenantSettingsService.actualizarMiNegocio({
        nombreNegocio,
        emailContacto: emailContacto || undefined,
        telefono: telefono || undefined,
      });
      // Actualizar el nombre en el contexto global
      if (updateTenant) {
        updateTenant({ ...tenant!, nombreNegocio });
      }
      setMensaje('Datos guardados correctamente');
      setTimeout(() => setMensaje(''), 3000);
    } catch (err) {
      setMensaje('Error al guardar');
    } finally {
      setGuardando(false);
    }
  };

  if (loading) return <div className="page-loading">Cargando...</div>;

  return (
    <div className="page-container">
      <div className="page-header">
        <h2><FiHome /> Mi Negocio</h2>
      </div>

      {/* Plan Info Card */}
      {planInfo && (
        <div className={`plan-card ${planInfo.esTrial ? 'plan-trial' : 'plan-pro'}`}>
          <div className="plan-card-header">
            <span className="plan-badge">{planInfo.plan}</span>
            {planInfo.esTrial && (
              <span className="plan-days">
                <FiClock /> {planInfo.diasRestantesTrial} dias restantes
              </span>
            )}
          </div>
          <p className="plan-card-text">
            {planInfo.esTrial
              ? `Tu prueba gratis vence el ${planInfo.fechaExpiracionTrial ? new Date(planInfo.fechaExpiracionTrial).toLocaleDateString('es-AR') : ''}.`
              : planInfo.plan === 'DePorVida'
              ? 'Tenes acceso de por vida a la app de escritorio.'
              : 'Tenes acceso completo al sistema via web.'
            }
          </p>
          {planInfo.esTrial && (
            <p className="plan-card-text" style={{ marginTop: '0.5rem', fontSize: '0.85rem' }}>
              Para activar un plan, contactanos a <strong style={{ color: 'var(--accent-primary)' }}>admin@gastronomiapp.com</strong>
            </p>
          )}
        </div>
      )}

      {/* Business Settings Form */}
      <div className="form-card">
        <h3>Datos del negocio</h3>

        <div className="form-group">
          <label><FiHome /> Nombre del negocio</label>
          <input
            type="text"
            value={nombreNegocio}
            onChange={e => setNombreNegocio(e.target.value)}
            placeholder="Ej: Sandwicheria Don Pepe"
          />
        </div>

        <div className="form-group">
          <label><FiMail /> Email de contacto</label>
          <input
            type="email"
            value={emailContacto}
            onChange={e => setEmailContacto(e.target.value)}
            placeholder="contacto@minegocio.com"
          />
        </div>

        <div className="form-group">
          <label><FiPhone /> Telefono</label>
          <input
            type="tel"
            value={telefono}
            onChange={e => setTelefono(e.target.value)}
            placeholder="+54 11 1234-5678"
          />
        </div>

        {mensaje && (
          <div className={`form-message ${mensaje.includes('Error') ? 'error' : 'success'}`}>
            {mensaje}
          </div>
        )}

        <button className="btn-primary" onClick={guardar} disabled={guardando}>
          <FiSave /> {guardando ? 'Guardando...' : 'Guardar cambios'}
        </button>
      </div>
    </div>
  );
}
