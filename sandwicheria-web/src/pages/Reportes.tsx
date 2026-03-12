import { useState, useEffect } from 'react';
import { reportesService } from '../services/api';
import { motion } from 'framer-motion';
import { FiBarChart2, FiCalendar, FiTrendingUp, FiPieChart, FiDollarSign } from 'react-icons/fi';

export default function Reportes() {
  const hoy = new Date().toISOString().split('T')[0];
  const hace30 = new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0];

  const [fechaInicio, setFechaInicio] = useState(hace30);
  const [fechaFin, setFechaFin] = useState(hoy);
  const [resumen, setResumen] = useState<any>(null);
  const [ventasPorDia, setVentasPorDia] = useState<any[]>([]);
  const [masVendidos, setMasVendidos] = useState<any[]>([]);
  const [porMetodoPago, setPorMetodoPago] = useState<any[]>([]);
  const [porCategoria, setPorCategoria] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [tabActiva, setTabActiva] = useState('resumen');

  useEffect(() => { cargar(); }, []);

  const cargar = async () => {
    setLoading(true);
    try {
      const [resRes, vdRes, mvRes, mpRes, vcRes] = await Promise.all([
        reportesService.resumen(fechaInicio, fechaFin),
        reportesService.ventasPorDia(fechaInicio, fechaFin),
        reportesService.productosMasVendidos(fechaInicio, fechaFin),
        reportesService.ventasPorMetodoPago(fechaInicio, fechaFin),
        reportesService.ventasPorCategoria(fechaInicio, fechaFin),
      ]);
      setResumen(resRes.data);
      setVentasPorDia(vdRes.data || []);
      setMasVendidos(mvRes.data || []);
      setPorMetodoPago(mpRes.data || []);
      setPorCategoria(vcRes.data || []);
    } catch (err: any) {
      alert('Error cargando reportes');
    } finally { setLoading(false); }
  };

  const formatMoney = (n: number) => `$${(n || 0).toLocaleString('es-AR')}`;
  const formatDate = (d: string) => new Date(d).toLocaleDateString('es-AR');

  const tabs = [
    { id: 'resumen', label: 'Resumen', icon: <FiDollarSign /> },
    { id: 'ventas-dia', label: 'Ventas por Dia', icon: <FiCalendar /> },
    { id: 'mas-vendidos', label: 'Mas Vendidos', icon: <FiTrendingUp /> },
    { id: 'metodo-pago', label: 'Metodo de Pago', icon: <FiPieChart /> },
    { id: 'por-categoria', label: 'Por Categoria', icon: <FiBarChart2 /> },
  ];

  return (
    <div className="page-reportes">
      <h2 className="page-title"><FiBarChart2 /> Reportes</h2>

      {/* Rango de fechas */}
      <div className="toolbar">
        <div className="date-range">
          <label>Desde:</label>
          <input type="date" value={fechaInicio} onChange={e => setFechaInicio(e.target.value)} />
          <label>Hasta:</label>
          <input type="date" value={fechaFin} onChange={e => setFechaFin(e.target.value)} />
          <button className="btn btn-primary" onClick={cargar} disabled={loading}>
            {loading ? 'Cargando...' : 'Generar Reportes'}
          </button>
        </div>
      </div>

      {/* Tabs */}
      <div className="report-tabs">
        {tabs.map(tab => (
          <button
            key={tab.id}
            className={`tab-btn ${tabActiva === tab.id ? 'active' : ''}`}
            onClick={() => setTabActiva(tab.id)}
          >
            {tab.icon} {tab.label}
          </button>
        ))}
      </div>

      {/* Contenido */}
      {resumen === null && !loading ? (
        <div className="section-card text-center text-muted" style={{ padding: '3rem' }}>
          Selecciona un rango de fechas y presiona "Generar Reportes"
        </div>
      ) : (
        <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
          {/* Resumen */}
          {tabActiva === 'resumen' && resumen && (
            <div className="resumen-grid">
              <div className="stat-card">
                <span className="stat-label">Total Ventas</span>
                <span className="stat-value text-success">{formatMoney(resumen.totalVentas)}</span>
              </div>
              <div className="stat-card">
                <span className="stat-label">Cantidad de Ventas</span>
                <span className="stat-value">{resumen.cantidadVentas}</span>
              </div>
              <div className="stat-card">
                <span className="stat-label">Promedio por Venta</span>
                <span className="stat-value">{formatMoney(resumen.ticketPromedio || resumen.promedioVenta)}</span>
              </div>
              <div className="stat-card">
                <span className="stat-label">Productos Vendidos</span>
                <span className="stat-value">{resumen.cantidadProductosVendidos || resumen.productosVendidos || 0}</span>
              </div>
            </div>
          )}

          {/* Ventas por dia */}
          {tabActiva === 'ventas-dia' && (
            <div className="section-card">
              <div className="bar-chart">
                {ventasPorDia.map((v, i) => {
                  const max = Math.max(...ventasPorDia.map(x => x.total || x.totalVentas || 0), 1);
                  const valor = v.total || v.totalVentas || 0;
                  return (
                    <div key={i} className="bar-row">
                      <span className="bar-label">{formatDate(v.fecha)}</span>
                      <div className="bar-track">
                        <motion.div
                          className="bar-fill"
                          initial={{ width: 0 }}
                          animate={{ width: `${(valor / max) * 100}%` }}
                          transition={{ delay: i * 0.05 }}
                        />
                      </div>
                      <span className="bar-value">{formatMoney(valor)}</span>
                    </div>
                  );
                })}
                {ventasPorDia.length === 0 && <p className="text-muted text-center">Sin datos</p>}
              </div>
            </div>
          )}

          {/* Mas vendidos */}
          {tabActiva === 'mas-vendidos' && (
            <div className="section-card">
              <table className="data-table">
                <thead><tr><th>#</th><th>Producto</th><th>Cantidad</th><th>Total</th></tr></thead>
                <tbody>
                  {masVendidos.map((p, i) => (
                    <tr key={i}>
                      <td><span className="rank-badge">{i + 1}</span></td>
                      <td><strong>{p.nombreProducto || p.nombre || p.producto}</strong></td>
                      <td>{p.cantidadVendida || p.cantidad}</td>
                      <td className="text-success">{formatMoney(p.totalVentas || p.total)}</td>
                    </tr>
                  ))}
                  {masVendidos.length === 0 && (
                    <tr><td colSpan={4} className="text-center text-muted">Sin datos</td></tr>
                  )}
                </tbody>
              </table>
            </div>
          )}

          {/* Por metodo de pago */}
          {tabActiva === 'metodo-pago' && (
            <div className="section-card">
              <div className="pago-cards">
                {porMetodoPago.map((mp, i) => (
                  <div key={i} className={`pago-card ${(mp.metodoPago || mp.metodo || '').toLowerCase()}`}>
                    <span className="pago-metodo">{mp.metodoPago || mp.metodo}</span>
                    <span className="pago-total">{formatMoney(mp.total || mp.totalVentas)}</span>
                    <span className="pago-cantidad">{mp.cantidad || mp.cantidadVentas} ventas</span>
                  </div>
                ))}
                {porMetodoPago.length === 0 && <p className="text-muted">Sin datos</p>}
              </div>
            </div>
          )}

          {/* Por categoria */}
          {tabActiva === 'por-categoria' && (
            <div className="section-card">
              <table className="data-table">
                <thead><tr><th>Categoria</th><th>Cantidad</th><th>Total</th></tr></thead>
                <tbody>
                  {porCategoria.map((c, i) => (
                    <tr key={i}>
                      <td><strong>{c.categoria || c.nombre}</strong></td>
                      <td>{c.cantidad || c.cantidadVendida}</td>
                      <td className="text-success">{formatMoney(c.total || c.totalVentas)}</td>
                    </tr>
                  ))}
                  {porCategoria.length === 0 && (
                    <tr><td colSpan={3} className="text-center text-muted">Sin datos</td></tr>
                  )}
                </tbody>
              </table>
            </div>
          )}
        </motion.div>
      )}
    </div>
  );
}
