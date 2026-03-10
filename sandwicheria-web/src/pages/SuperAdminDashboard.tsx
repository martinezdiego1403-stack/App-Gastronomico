import { useState, useEffect } from 'react';
import { superAdminService } from '../services/api';
import { FiUsers, FiActivity, FiClock, FiTrendingUp, FiToggleLeft, FiToggleRight, FiEye } from 'react-icons/fi';

interface DashboardStats {
  totalTenants: number;
  tenantsActivos: number;
  tenantsInactivos: number;
  tenantsTrial: number;
  tenantsTrialExpirado: number;
  tenantsPago: number;
  totalUsuarios: number;
  registrosHoy: number;
  registrosEstaSemana: number;
}

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

export default function SuperAdminDashboard() {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [tenants, setTenants] = useState<TenantRow[]>([]);
  const [loading, setLoading] = useState(true);
  const [detalle, setDetalle] = useState<any>(null);

  useEffect(() => {
    cargarDatos();
  }, []);

  const cargarDatos = async () => {
    try {
      const [dashRes, tenantsRes] = await Promise.all([
        superAdminService.dashboard(),
        superAdminService.obtenerTenants(),
      ]);
      setStats(dashRes.data);
      setTenants(tenantsRes.data);
    } catch (err) {
      console.error('Error cargando dashboard SuperAdmin', err);
    } finally {
      setLoading(false);
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

  return (
    <div className="page-container">
      <div className="page-header">
        <h2><FiActivity /> Panel SuperAdmin</h2>
      </div>

      {/* Stats Cards */}
      {stats && (
        <div className="stats-grid">
          <div className="stat-card">
            <div className="stat-icon"><FiUsers /></div>
            <div className="stat-value">{stats.totalTenants}</div>
            <div className="stat-label">Negocios totales</div>
          </div>
          <div className="stat-card stat-green">
            <div className="stat-icon"><FiToggleRight /></div>
            <div className="stat-value">{stats.tenantsActivos}</div>
            <div className="stat-label">Activos</div>
          </div>
          <div className="stat-card stat-orange">
            <div className="stat-icon"><FiClock /></div>
            <div className="stat-value">{stats.tenantsTrial}</div>
            <div className="stat-label">En trial</div>
          </div>
          <div className="stat-card stat-blue">
            <div className="stat-icon"><FiTrendingUp /></div>
            <div className="stat-value">{stats.tenantsPago}</div>
            <div className="stat-label">Plan pago</div>
          </div>
          <div className="stat-card">
            <div className="stat-icon"><FiUsers /></div>
            <div className="stat-value">{stats.totalUsuarios}</div>
            <div className="stat-label">Usuarios totales</div>
          </div>
          <div className="stat-card stat-green">
            <div className="stat-icon"><FiTrendingUp /></div>
            <div className="stat-value">{stats.registrosEstaSemana}</div>
            <div className="stat-label">Registros esta semana</div>
          </div>
        </div>
      )}

      {/* Tenants Table */}
      <div className="table-card">
        <h3>Negocios registrados</h3>
        <div className="table-responsive">
          <table className="data-table">
            <thead>
              <tr>
                <th>Negocio</th>
                <th>Plan</th>
                <th>Estado</th>
                <th>Usuarios</th>
                <th>Ventas</th>
                <th>Productos</th>
                <th>Registro</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {tenants.map(t => (
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
                      <option value="Basico">Basico</option>
                      <option value="Pro">Pro</option>
                    </select>
                  </td>
                  <td>
                    <span className={`badge ${t.activo ? 'badge-green' : 'badge-red'}`}>
                      {t.activo ? 'Activo' : 'Inactivo'}
                    </span>
                  </td>
                  <td>{t.cantidadUsuarios}</td>
                  <td>{t.cantidadVentas}</td>
                  <td>{t.cantidadProductos}</td>
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
            </tbody>
          </table>
        </div>
      </div>

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
