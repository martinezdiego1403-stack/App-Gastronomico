import { useState, useEffect } from 'react';
import { cajasService, configuracionService } from '../services/api';
import { motion } from 'framer-motion';
import { FiDollarSign, FiLock, FiClock, FiCreditCard } from 'react-icons/fi';

interface CajaData {
  cajaID: number;
  montoInicial: number;
  fechaApertura: string;
  estado: string;
}

interface CajaHistorial {
  cajaID: number;
  montoInicial: number;
  montoCierre: number;
  fechaApertura: string;
  fechaCierre: string;
  totalVentas: number;
  diferenciaEsperado: number;
  observaciones: string;
}

export default function Caja() {
  const [cajaAbierta, setCajaAbierta] = useState<CajaData | null>(null);
  const [historial, setHistorial] = useState<CajaHistorial[]>([]);
  const [montoInicial, setMontoInicial] = useState('0');
  const [montoCierre, setMontoCierre] = useState('');
  const [observaciones, setObservaciones] = useState('');
  const [loading, setLoading] = useState(true);
  const [resumenPagos, setResumenPagos] = useState<any>(null);

  useEffect(() => { cargar(); }, []);

  const cargar = async () => {
    setLoading(true);
    try {
      const [cajaRes, histRes] = await Promise.all([
        cajasService.obtenerAbierta().catch(() => null),
        cajasService.historial().catch(() => ({ data: [] })),
      ]);
      if (cajaRes?.data?.hayCajaAbierta && cajaRes.data.caja) {
        setCajaAbierta(cajaRes.data.caja);
        try {
          const pagosRes = await cajasService.resumenPagos(cajaRes.data.caja.cajaID);
          setResumenPagos(pagosRes.data);
        } catch {}
      } else {
        setCajaAbierta(null);
      }
      setHistorial(histRes?.data || []);
    } catch {} finally {
      setLoading(false);
    }
  };

  const abrirCaja = async () => {
    try {
      await cajasService.abrir(parseFloat(montoInicial) || 0);
      setMontoInicial('0');
      await cargar();
    } catch (err: any) {
      alert(err.response?.data?.error || 'Error al abrir caja');
    }
  };

  const cerrarCaja = async () => {
    if (!cajaAbierta) return;
    if (!montoCierre) { alert('Ingresa el monto de cierre'); return; }
    try {
      const cajaId = cajaAbierta.cajaID;
      await cajasService.cerrar(cajaId, parseFloat(montoCierre), observaciones);
      // Intentar enviar resumen por WhatsApp
      try {
        const whatsRes = await configuracionService.resumenCierreCaja(cajaId);
        if (whatsRes.data?.link) {
          window.open(whatsRes.data.link, '_blank');
        }
      } catch {} // Si WhatsApp no esta configurado, no pasa nada
      setMontoCierre('');
      setObservaciones('');
      setResumenPagos(null);
      await cargar();
    } catch (err: any) {
      alert(err.response?.data?.error || 'Error al cerrar caja');
    }
  };

  const formatMoney = (n: number) => `$${(n || 0).toLocaleString('es-AR')}`;
  const formatDate = (d: string) => d ? new Date(d).toLocaleString('es-AR') : '-';

  if (loading) return <div className="page-loading">Cargando caja...</div>;

  return (
    <div className="page-caja">
      <h2 className="page-title"><FiDollarSign /> Caja</h2>

      {/* Estado actual */}
      <motion.div
        className={`caja-status ${cajaAbierta ? 'open' : 'closed'}`}
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
      >
        {cajaAbierta ? (
          <div className="caja-open-info">
            <div className="caja-badge open">CAJA ABIERTA</div>
            <div className="caja-details">
              <p><FiClock /> Abierta: {formatDate(cajaAbierta.fechaApertura)}</p>
              <p><FiDollarSign /> Monto inicial: {formatMoney(cajaAbierta.montoInicial)}</p>
            </div>

            {resumenPagos && (
              <div className="caja-resumen-pagos">
                <h4><FiCreditCard /> Resumen de pagos</h4>
                <div className="pagos-grid">
                  {Object.entries(resumenPagos).map(([metodo, monto]: any) => (
                    <div key={metodo} className="pago-item">
                      <span>{metodo}</span>
                      <strong>{formatMoney(monto)}</strong>
                    </div>
                  ))}
                </div>
              </div>
            )}

            <div className="caja-cerrar-form">
              <h4><FiLock /> Cerrar Caja</h4>
              <div className="form-row">
                <input
                  type="number"
                  placeholder="Monto de cierre"
                  value={montoCierre}
                  onChange={e => setMontoCierre(e.target.value)}
                />
                <input
                  type="text"
                  placeholder="Observaciones (opcional)"
                  value={observaciones}
                  onChange={e => setObservaciones(e.target.value)}
                />
                <button className="btn btn-danger" onClick={cerrarCaja}>
                  <FiLock /> Cerrar Caja
                </button>
              </div>
            </div>
          </div>
        ) : (
          <div className="caja-closed-info">
            <div className="caja-badge closed">CAJA CERRADA</div>
            <div className="caja-abrir-form">
              <input
                type="number"
                placeholder="Monto inicial (opcional)"
                value={montoInicial}
                onChange={e => setMontoInicial(e.target.value)}
              />
              <button className="btn btn-success" onClick={abrirCaja}>
                <FiDollarSign /> Abrir Caja
              </button>
            </div>
          </div>
        )}
      </motion.div>

      {/* Historial */}
      <div className="section-card">
        <h3><FiClock /> Historial de Cajas</h3>
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Apertura</th>
                <th>Cierre</th>
                <th>Monto Inicial</th>
                <th>Monto Cierre</th>
                <th>Total Ventas</th>
                <th>Diferencia</th>
                <th>Observaciones</th>
              </tr>
            </thead>
            <tbody>
              {historial.map(c => (
                <tr key={c.cajaID}>
                  <td>{formatDate(c.fechaApertura)}</td>
                  <td>{formatDate(c.fechaCierre)}</td>
                  <td>{formatMoney(c.montoInicial)}</td>
                  <td>{formatMoney(c.montoCierre)}</td>
                  <td className="text-success">{formatMoney(c.totalVentas)}</td>
                  <td className={c.diferenciaEsperado >= 0 ? 'text-success' : 'text-danger'}>
                    {formatMoney(c.diferenciaEsperado)}
                  </td>
                  <td>{c.observaciones || '-'}</td>
                </tr>
              ))}
              {historial.length === 0 && (
                <tr><td colSpan={7} className="text-center text-muted">Sin historial</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
