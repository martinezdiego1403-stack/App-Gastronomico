import { useState, useEffect } from 'react';
import { configuracionService, productosService } from '../services/api';
import { motion } from 'framer-motion';
import { FiSettings, FiSave, FiSend, FiAlertTriangle, FiCheckCircle, FiSmartphone } from 'react-icons/fi';

export default function Configuracion() {
  const [numero, setNumero] = useState('');
  const [habilitado, setHabilitado] = useState(false);
  const [nombreNegocio, setNombreNegocio] = useState('La Sandwicheria');
  const [loading, setLoading] = useState(true);
  const [mensaje, setMensaje] = useState('');
  const [tipoMensaje, setTipoMensaje] = useState<'success' | 'error' | ''>('');
  const [stockBajoCount, setStockBajoCount] = useState(0);

  useEffect(() => { cargar(); }, []);

  const cargar = async () => {
    setLoading(true);
    try {
      const [configRes, stockRes] = await Promise.all([
        configuracionService.obtenerWhatsApp(),
        productosService.obtenerStockBajo().catch(() => ({ data: [] })),
      ]);
      setNumero(configRes.data.whatsAppNumero || '');
      setHabilitado(configRes.data.whatsAppHabilitado || false);
      setNombreNegocio(configRes.data.nombreNegocio || 'La Sandwicheria');
      setStockBajoCount(Array.isArray(stockRes.data) ? stockRes.data.length : 0);
    } catch {} finally { setLoading(false); }
  };

  const mostrarMsg = (text: string, tipo: 'success' | 'error') => {
    setMensaje(text);
    setTipoMensaje(tipo);
    setTimeout(() => { setMensaje(''); setTipoMensaje(''); }, 4000);
  };

  const guardar = async () => {
    try {
      await configuracionService.guardarWhatsApp({
        whatsAppNumero: numero,
        whatsAppHabilitado: habilitado,
        nombreNegocio,
      });
      mostrarMsg('Configuracion guardada correctamente', 'success');
    } catch (err: any) {
      mostrarMsg(err.response?.data?.error || 'Error al guardar', 'error');
    }
  };

  const enviarTest = async () => {
    try {
      const res = await configuracionService.testWhatsApp();
      window.open(res.data.link, '_blank');
      mostrarMsg('Se abrio WhatsApp Web con el mensaje de prueba', 'success');
    } catch (err: any) {
      mostrarMsg(err.response?.data?.error || 'Configura el numero primero', 'error');
    }
  };

  const enviarAlertaStock = async () => {
    try {
      const res = await configuracionService.alertaStockBajo();
      if (res.data.links && res.data.links.length > 0) {
        for (const item of res.data.links) {
          window.open(item.link, '_blank');
        }
        mostrarMsg(`Se abrieron ${res.data.links.length} alerta(s) de stock bajo`, 'success');
      } else {
        mostrarMsg('No hay productos con stock bajo', 'success');
      }
    } catch (err: any) {
      mostrarMsg(err.response?.data?.error || 'Error', 'error');
    }
  };

  if (loading) return <div className="page-loading">Cargando configuracion...</div>;

  return (
    <div className="page-configuracion">
      <h2 className="page-title"><FiSettings /> Configuracion</h2>

      {mensaje && (
        <motion.div
          className={`alert ${tipoMensaje === 'success' ? 'alert-success' : 'alert-danger'}`}
          initial={{ opacity: 0, y: -10 }}
          animate={{ opacity: 1, y: 0 }}
        >
          {tipoMensaje === 'success' ? <FiCheckCircle /> : <FiAlertTriangle />}
          <span>{mensaje}</span>
        </motion.div>
      )}

      {/* WhatsApp Config */}
      <div className="section-card">
        <h3><FiSmartphone /> Notificaciones WhatsApp</h3>
        <p className="config-description">
          Recibe alertas automaticas por WhatsApp cuando el stock este bajo
          y un resumen completo al cerrar la caja del dia.
        </p>

        <div className="config-form">
          <div className="config-toggle">
            <label className="toggle-label">
              <input
                type="checkbox"
                checked={habilitado}
                onChange={e => setHabilitado(e.target.checked)}
              />
              <span className="toggle-slider" />
              <span className="toggle-text">
                {habilitado ? 'Notificaciones activadas' : 'Notificaciones desactivadas'}
              </span>
            </label>
          </div>

          <div className="form-grid" style={{ marginTop: '1.5rem' }}>
            <div className="form-group">
              <label>Nombre del negocio</label>
              <input
                value={nombreNegocio}
                onChange={e => setNombreNegocio(e.target.value)}
                placeholder="Ej: La Sandwicheria"
              />
            </div>
            <div className="form-group">
              <label>Numero de WhatsApp (con codigo de pais)</label>
              <input
                value={numero}
                onChange={e => setNumero(e.target.value)}
                placeholder="Ej: 5493811234567"
              />
              <small className="config-hint">
                Argentina: 549 + codigo de area + numero (sin 0 ni 15)
              </small>
            </div>
          </div>

          <div className="config-actions">
            <button className="btn btn-orange" onClick={guardar}>
              <FiSave /> Guardar Configuracion
            </button>
            <button className="btn btn-success" onClick={enviarTest} disabled={!numero}>
              <FiSend /> Enviar Mensaje de Prueba
            </button>
          </div>
        </div>
      </div>

      {/* Acciones rapidas */}
      <div className="section-card">
        <h3><FiAlertTriangle /> Acciones Rapidas</h3>
        <div className="quick-actions">
          <div className="quick-action-card">
            <div className="qa-icon warning">
              <FiAlertTriangle />
            </div>
            <div className="qa-info">
              <h4>Alerta de Stock Bajo</h4>
              <p>
                {stockBajoCount > 0
                  ? `Hay ${stockBajoCount} producto(s) con stock bajo`
                  : 'Todo el stock esta bien'}
              </p>
            </div>
            <button
              className="btn btn-ghost"
              onClick={enviarAlertaStock}
              disabled={!habilitado || !numero}
            >
              <FiSend /> Enviar Alerta
            </button>
          </div>
        </div>

        <div className="config-info-panel">
          <h4>Como funciona?</h4>
          <ul>
            <li><strong>Stock bajo:</strong> Cuando un producto tiene stock igual o menor al minimo, se genera una alerta que podes enviar por WhatsApp.</li>
            <li><strong>Cierre de caja:</strong> Al cerrar la caja, se genera automaticamente un resumen con todas las ventas del dia, metodos de pago y alertas de stock. Se abre WhatsApp Web para que confirmes el envio.</li>
            <li><strong>Formato:</strong> Los mensajes usan formato WhatsApp (negritas, emojis) para que sean faciles de leer.</li>
          </ul>
        </div>
      </div>
    </div>
  );
}
