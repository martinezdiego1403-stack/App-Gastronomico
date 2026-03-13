import { useState, useEffect } from 'react';
import { superAdminService } from '../services/api';
import { FiUsers, FiActivity, FiClock, FiTrendingUp, FiToggleLeft, FiToggleRight, FiEye, FiDollarSign, FiCheckCircle, FiXCircle, FiImage } from 'react-icons/fi';

interface TenantRow {
  tenantID: number;
  tenantId: string;
  nombreNegocio: string;
  plan: string;
  activo: boolean;
  fechaCreacion: string;
  fechaExpiracionTrial: string | null;
  emailContacto: string | null;
  cantidadUsuarios: number;
  cantidadVentas: number;
  cantidadProductos: number;
}

interface DashboardData {
  totalTenants: number;
  tenantsActivos: number;
  tenantsInactivos: number;
  tenantsTrial: number;
  tenantsTrialExpirado: number;
  tenantsPro: number;
  tenantsProPlus: number;
  tenantsProForever: number;
  totalUsuarios: number;
  registrosHoy: number;
  registrosEstaSemana: number;
  solicitudesPendientes: number;
  tenants: TenantRow[];
}

interface SolicitudRow {
  solicitudPagoID: number;
  tenantId: string;
  nombreNegocio: string;
  planSolicitado: string;
  metodoPago: string;
  montoDeclarado: number;
  monedaPago: string;
  referenciaTransferencia: string | null;
  estado: string;
  motivoRechazo: string | null;
  fechaSolicitud: string;
  fechaResolucion: string | null;
}

export default function SuperAdminDashboard() {
  const [data, setData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [detalle, setDetalle] = useState<any>(null);
  const [solicitudes, setSolicitudes] = useState<SolicitudRow[]>([]);
  const [filtroSol, setFiltroSol] = useState('Pendiente');
  const [solDetalle, setSolDetalle] = useState<any>(null);
  const [motivoRechazo, setMotivoRechazo] = useState('');
  const [tab, setTab] = useState<'negocios' | 'solicitudes'>('negocios');

  useEffect(() => {
    cargarDatos();
    cargarSolicitudes();
  }, []);

  useEffect(() => {
    cargarSolicitudes();
  }, [filtroSol]);

  const cargarDatos = async () => {
    try {
      const res = await superAdminService.dashboard();
      setData(res.data);
      setError('');
    } catch (err: any) {
      console.error('Error cargando dashboard SuperAdmin', err);
      setError('Error al cargar datos: ' + (err.message || 'Error desconocido'));
    } finally {
      setLoading(false);
    }
  };

  const cargarSolicitudes = async () => {
    try {
      const res = await superAdminService.solicitudesPago(filtroSol || undefined);
      setSolicitudes(res.data);
    } catch (err) {
      console.error('Error cargando solicitudes', err);
    }
  };

  const verComprobante = async (id: number) => {
    try {
      const res = await superAdminService.obtenerSolicitud(id);
      setSolDetalle(res.data);
      setMotivoRechazo('');
    } catch (err) {
      console.error('Error cargando solicitud', err);
    }
  };

  const aprobar = async (id: number) => {
    if (!window.confirm('Aprobar esta solicitud? El plan se actualizara automaticamente.')) return;
    try {
      await superAdminService.aprobarSolicitud(id);
      setSolDetalle(null);
      cargarSolicitudes();
      cargarDatos();
    } catch (err) {
      console.error('Error aprobando', err);
    }
  };

  const rechazar = async (id: number) => {
    if (!motivoRechazo.trim()) {
      alert('Ingresa un motivo de rechazo');
      return;
    }
    try {
      await superAdminService.rechazarSolicitud(id, motivoRechazo);
      setSolDetalle(null);
      cargarSolicitudes();
      cargarDatos();
    } catch (err) {
      console.error('Error rechazando', err);
    }
  };

  const toggleTenant = async (tenantId: string, activo: boolean) => {
    try {
      await superAdminService.cambiarEstadoTenant(tenantId, !activo);
      cargarDatos();
    } catch (err) {
      console.error('Error cambiando estado', err);
    }
  };

  const cambiarPlan = async (tenantId: string, plan: string) => {
    try {
      await superAdminService.cambiarPlan(tenantId, plan);
      cargarDatos();
    } catch (err) {
      console.error('Error cambiando plan', err);
    }
  };

  const verDetalle = async (tenantId: string) => {
    try {
      const res = await superAdminService.obtenerTenant(tenantId);
      setDetalle(res.data);
    } catch (err) {
      console.error('Error cargando detalle', err);
    }
  };

  if (loading) return <div className="page-loading">Cargando...</div>;
  if (error) return <div className="page-loading" style={{ color: '#ff5252' }}>{error}</div>;

  return (
    <div className="page-container">
      <div className="page-header">
        <h2><FiActivity /> Panel SuperAdmin</h2>
      </div>

      {/* Stats Cards */}
      {data && (
        <div className="stats-grid">
          <div className="stat-card">
            <div className="stat-icon"><FiUsers /></div>
            <div className="stat-value">{data.totalTenants}</div>
            <div className="stat-label">Negocios totales</div>
          </div>
          <div className="stat-card stat-green">
            <div className="stat-icon"><FiToggleRight /></div>
            <div className="stat-value">{data.tenantsActivos}</div>
            <div className="stat-label">Activos</div>
          </div>
          <div className="stat-card stat-orange">
            <div className="stat-icon"><FiClock /></div>
            <div className="stat-value">{data.tenantsTrial}</div>
            <div className="stat-label">En trial</div>
          </div>
          <div className="stat-card stat-blue">
            <div className="stat-icon"><FiTrendingUp /></div>
            <div className="stat-value">{data.tenantsPro}</div>
            <div className="stat-label">Plan Pro</div>
          </div>
          <div className="stat-card stat-blue">
            <div className="stat-icon"><FiTrendingUp /></div>
            <div className="stat-value">{data.tenantsProPlus}</div>
            <div className="stat-label">Plan Pro+</div>
          </div>
          <div className="stat-card stat-blue">
            <div className="stat-icon"><FiTrendingUp /></div>
            <div className="stat-value">{data.tenantsProForever}</div>
            <div className="stat-label">Pro Forever</div>
          </div>
          <div className="stat-card">
            <div className="stat-icon"><FiUsers /></div>
            <div className="stat-value">{data.totalUsuarios}</div>
            <div className="stat-label">Usuarios totales</div>
          </div>
          <div className="stat-card stat-green">
            <div className="stat-icon"><FiTrendingUp /></div>
            <div className="stat-value">{data.registrosEstaSemana}</div>
            <div className="stat-label">Registros esta semana</div>
          </div>
        </div>
      )}

      {/* Tabs */}
      <div className="sa-tabs">
        <button className={`sa-tab ${tab === 'negocios' ? 'active' : ''}`} onClick={() => setTab('negocios')}>
          <FiUsers /> Negocios ({data?.tenants?.length || 0})
        </button>
        <button className={`sa-tab ${tab === 'solicitudes' ? 'active' : ''}`} onClick={() => setTab('solicitudes')}>
          <FiDollarSign /> Solicitudes de Pago
          {(data?.solicitudesPendientes || 0) > 0 && (
            <span className="sa-badge">{data?.solicitudesPendientes}</span>
          )}
        </button>
      </div>

      {/* Tenants Table */}
      {tab === 'negocios' && <div className="table-card">
        <h3>Negocios registrados ({data?.tenants?.length || 0})</h3>
        <div className="table-responsive">
          <table className="data-table">
            <thead>
              <tr>
                <th>Negocio</th>
                <th>Plan</th>
                <th>Estado</th>
                <th>Registro</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {(data?.tenants || []).map(t => (
                <tr key={t.tenantId}>
                  <td>
                    <div className="tenant-name">{t.nombreNegocio}</div>
                    <div className="tenant-email">{t.emailContacto || '-'}</div>
                  </td>
                  <td>
                    <select
                      className="plan-select"
                      value={t.plan}
                      onChange={e => cambiarPlan(t.tenantId, e.target.value)}
                    >
                      <option value="Trial">Trial</option>
                      <option value="Pro">Pro (Mensual)</option>
                      <option value="Pro+">Pro+ (Anual)</option>
                      <option value="ProForever">Pro Forever</option>
                    </select>
                  </td>
                  <td>
                    <span className={`badge ${t.activo ? 'badge-green' : 'badge-red'}`}>
                      {t.activo ? 'Activo' : 'Inactivo'}
                    </span>
                  </td>
                  <td>{new Date(t.fechaCreacion).toLocaleDateString('es-AR')}</td>
                  <td>
                    <div className="action-buttons">
                      <button
                        className="btn-icon"
                        onClick={() => toggleTenant(t.tenantId, t.activo)}
                        title={t.activo ? 'Desactivar' : 'Activar'}
                      >
                        {t.activo ? <FiToggleRight color="#4caf50" /> : <FiToggleLeft color="#999" />}
                      </button>
                      <button
                        className="btn-icon"
                        onClick={() => verDetalle(t.tenantId)}
                        title="Ver detalle"
                      >
                        <FiEye />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
              {(!data?.tenants || data.tenants.length === 0) && (
                <tr><td colSpan={5} style={{ textAlign: 'center', padding: '2rem', opacity: 0.5 }}>No hay negocios registrados</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>}

      {/* Solicitudes de Pago */}
      {tab === 'solicitudes' && (
        <div className="table-card">
          <div className="sa-sol-filters">
            {['Pendiente', 'Aprobada', 'Rechazada', ''].map(f => (
              <button
                key={f || 'todas'}
                className={`sa-filter-btn ${filtroSol === f ? 'active' : ''}`}
                onClick={() => setFiltroSol(f)}
              >
                {f || 'Todas'}
              </button>
            ))}
          </div>
          <div className="table-responsive">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Negocio</th>
                  <th>Plan</th>
                  <th>Monto</th>
                  <th>Metodo</th>
                  <th>Estado</th>
                  <th>Fecha</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {solicitudes.map(s => (
                  <tr key={s.solicitudPagoID}>
                    <td>{s.nombreNegocio}</td>
                    <td>{s.planSolicitado}</td>
                    <td>{s.monedaPago === 'ARS' ? `$${s.montoDeclarado.toLocaleString('es-AR')}` : `$${s.montoDeclarado} USDT`}</td>
                    <td>{s.metodoPago === 'CVU_ARS' ? 'CVU/ARS' : s.metodoPago.replace('USDT_', 'USDT ')}</td>
                    <td>
                      <span className={`badge ${s.estado === 'Pendiente' ? 'badge-orange' : s.estado === 'Aprobada' ? 'badge-green' : 'badge-red'}`}>
                        {s.estado}
                      </span>
                    </td>
                    <td>{new Date(s.fechaSolicitud).toLocaleDateString('es-AR')}</td>
                    <td>
                      <button className="btn-icon" onClick={() => verComprobante(s.solicitudPagoID)} title="Ver comprobante">
                        <FiImage />
                      </button>
                    </td>
                  </tr>
                ))}
                {solicitudes.length === 0 && (
                  <tr><td colSpan={7} style={{ textAlign: 'center', padding: '2rem', opacity: 0.5 }}>No hay solicitudes</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Solicitud Detail Modal */}
      {solDetalle && (
        <div className="modal-overlay" onClick={() => setSolDetalle(null)}>
          <div className="modal-content sa-sol-modal" onClick={e => e.stopPropagation()}>
            <h3>Solicitud de {solDetalle.nombreNegocio}</h3>
            <div className="detail-grid">
              <div><strong>Plan solicitado:</strong> {solDetalle.planSolicitado}</div>
              <div><strong>Metodo:</strong> {solDetalle.metodoPago === 'CVU_ARS' ? 'CVU/ARS' : solDetalle.metodoPago.replace('USDT_', 'USDT ')}</div>
              <div><strong>Monto:</strong> {solDetalle.monedaPago === 'ARS' ? `$${solDetalle.montoDeclarado.toLocaleString('es-AR')} ARS` : `$${solDetalle.montoDeclarado} USDT`}</div>
              <div><strong>Referencia:</strong> {solDetalle.referenciaTransferencia || '-'}</div>
              <div><strong>Estado:</strong> {solDetalle.estado}</div>
              <div><strong>Fecha:</strong> {new Date(solDetalle.fechaSolicitud).toLocaleString('es-AR')}</div>
            </div>

            {solDetalle.comprobante && (
              <div className="sa-comprobante">
                <h4>Comprobante:</h4>
                <img src={solDetalle.comprobante} alt="Comprobante de pago" />
              </div>
            )}

            {solDetalle.estado === 'Pendiente' && (
              <div className="sa-sol-actions">
                <button className="btn btn-success" onClick={() => aprobar(solDetalle.solicitudPagoID)}>
                  <FiCheckCircle /> Aprobar
                </button>
                <div className="sa-rechazo">
                  <input
                    type="text"
                    placeholder="Motivo de rechazo..."
                    value={motivoRechazo}
                    onChange={e => setMotivoRechazo(e.target.value)}
                  />
                  <button className="btn btn-danger" onClick={() => rechazar(solDetalle.solicitudPagoID)}>
                    <FiXCircle /> Rechazar
                  </button>
                </div>
              </div>
            )}

            <button className="btn-primary" onClick={() => setSolDetalle(null)} style={{ marginTop: '1rem' }}>
              Cerrar
            </button>
          </div>
        </div>
      )}

      {/* Detail Modal */}
      {detalle && (
        <div className="modal-overlay" onClick={() => setDetalle(null)}>
          <div className="modal-content" onClick={e => e.stopPropagation()}>
            <h3>Detalle: {detalle.tenant.nombreNegocio}</h3>
            <div className="detail-grid">
              <div><strong>TenantId:</strong> {detalle.tenant.tenantId}</div>
              <div><strong>Plan:</strong> {detalle.tenant.plan}</div>
              <div><strong>Estado:</strong> {detalle.tenant.activo ? 'Activo' : 'Inactivo'}</div>
              <div><strong>Dias trial:</strong> {detalle.tenant.diasRestantesTrial}</div>
              <div><strong>Email:</strong> {detalle.tenant.emailContacto || '-'}</div>
              <div><strong>Telefono:</strong> {detalle.tenant.telefono || '-'}</div>
            </div>

            <h4>Usuarios ({detalle.usuarios.length})</h4>
            <table className="data-table">
              <thead>
                <tr><th>Usuario</th><th>Nombre</th><th>Rol</th><th>Activo</th></tr>
              </thead>
              <tbody>
                {detalle.usuarios.map((u: any) => (
                  <tr key={u.usuarioID}>
                    <td>{u.nombreUsuario}</td>
                    <td>{u.nombreCompleto}</td>
                    <td>{u.rol}</td>
                    <td>{u.activo ? 'Si' : 'No'}</td>
                  </tr>
                ))}
              </tbody>
            </table>

            <h4>Estadisticas</h4>
            <div className="detail-grid">
              <div><strong>Ventas:</strong> {detalle.estadisticas.totalVentas}</div>
              <div><strong>Productos:</strong> {detalle.estadisticas.totalProductos}</div>
              <div><strong>Recetas:</strong> {detalle.estadisticas.totalRecetas}</div>
            </div>

            <button className="btn-primary" onClick={() => setDetalle(null)} style={{ marginTop: '1rem' }}>
              Cerrar
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
