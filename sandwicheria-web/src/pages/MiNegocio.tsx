import { useState, useEffect } from 'react';
import { tenantSettingsService } from '../services/api';
import { useAuth } from '../context/AuthContext';
import { FiSave, FiHome, FiMail, FiPhone, FiClock, FiFileText } from 'react-icons/fi';

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
  const [condicionFiscal, setCondicionFiscal] = useState('ConsumidorFinal');
  const [cuit, setCuit] = useState('');
  const [direccionFiscal, setDireccionFiscal] = useState('');
  const [puntoVenta, setPuntoVenta] = useState(1);
  const [tipoFactura, setTipoFactura] = useState('X');
  const [guardando, setGuardando] = useState(false);
  const [guardandoFiscal, setGuardandoFiscal] = useState(false);
  const [mensaje, setMensaje] = useState('');
  const [mensajeFiscal, setMensajeFiscal] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    cargarDatos();
  }, []);

  const cargarDatos = async () => {
    try {
      const [negocioRes, planRes, fiscalRes] = await Promise.all([
        tenantSettingsService.obtenerMiNegocio(),
        tenantSettingsService.obtenerPlan(),
        tenantSettingsService.obtenerFiscal(),
      ]);
      const negocio = negocioRes.data;
      setNombreNegocio(negocio.nombreNegocio || '');
      setEmailContacto(negocio.emailContacto || '');
      setTelefono(negocio.telefono || '');
      setPlanInfo(planRes.data);

      const fiscal = fiscalRes.data;
      setCondicionFiscal(fiscal.condicionFiscal || 'ConsumidorFinal');
      setCuit(fiscal.cuit || '');
      setDireccionFiscal(fiscal.direccionFiscal || '');
      setPuntoVenta(fiscal.puntoVenta || 1);
      setTipoFactura(fiscal.tipoFactura || 'X');
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

  const guardarFiscal = async () => {
    setGuardandoFiscal(true);
    setMensajeFiscal('');
    try {
      const res = await tenantSettingsService.actualizarFiscal({
        condicionFiscal,
        cuit: cuit || undefined,
        direccionFiscal: direccionFiscal || undefined,
        puntoVenta: puntoVenta || undefined,
      });
      setTipoFactura(res.data.tipoFactura || 'X');
      setMensajeFiscal('Datos fiscales guardados correctamente');
      setTimeout(() => setMensajeFiscal(''), 3000);
    } catch (err) {
      setMensajeFiscal('Error al guardar datos fiscales');
    } finally {
      setGuardandoFiscal(false);
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

      {/* Fiscal Settings Form */}
      <div className="form-card">
        <h3><FiFileText /> Datos Fiscales</h3>
        <p className="form-description">
          Estos datos determinan el tipo de comprobante que se emite al imprimir tickets.
        </p>

        <div className="form-group">
          <label>Condicion Fiscal</label>
          <select value={condicionFiscal} onChange={e => setCondicionFiscal(e.target.value)}>
            <option value="ConsumidorFinal">Consumidor Final (Ticket comun)</option>
            <option value="Monotributista">Monotributista (Factura C)</option>
            <option value="ResponsableInscripto">Responsable Inscripto (Factura A/B)</option>
          </select>
        </div>

        {condicionFiscal !== 'ConsumidorFinal' && (
          <>
            <div className="form-group">
              <label>CUIT</label>
              <input
                type="text"
                value={cuit}
                onChange={e => setCuit(e.target.value)}
                placeholder="XX-XXXXXXXX-X"
              />
            </div>

            <div className="form-group">
              <label>Direccion Fiscal</label>
              <input
                type="text"
                value={direccionFiscal}
                onChange={e => setDireccionFiscal(e.target.value)}
                placeholder="Av. San Martin 1234, CABA"
              />
            </div>

            <div className="form-group">
              <label>Punto de Venta</label>
              <input
                type="number"
                min={1}
                max={9999}
                value={puntoVenta}
                onChange={e => setPuntoVenta(parseInt(e.target.value) || 1)}
              />
            </div>
          </>
        )}

        <div className="fiscal-info">
          <span className="fiscal-label">Tipo de comprobante:</span>
          <span className={`fiscal-badge badge-${tipoFactura === 'X' ? 'ticket' : 'factura'}`}>
            {tipoFactura === 'X' ? 'Ticket comun (X)' :
             tipoFactura === 'C' ? 'Factura C' :
             'Factura A/B'}
          </span>
          {planInfo?.plan === 'Mensual' && condicionFiscal !== 'ConsumidorFinal' && (
            <p className="fiscal-note">
              Con el plan Mensual solo se emiten tickets comunes. Actualiza al plan De por Vida para emitir facturas.
            </p>
          )}
        </div>

        {mensajeFiscal && (
          <div className={`form-message ${mensajeFiscal.includes('Error') ? 'error' : 'success'}`}>
            {mensajeFiscal}
          </div>
        )}

        <button className="btn-primary" onClick={guardarFiscal} disabled={guardandoFiscal}>
          <FiSave /> {guardandoFiscal ? 'Guardando...' : 'Guardar datos fiscales'}
        </button>
      </div>
    </div>
  );
}
