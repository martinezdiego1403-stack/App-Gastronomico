import { useState, useEffect, useRef } from 'react';
import { pagosService } from '../services/api';
import { motion, AnimatePresence } from 'framer-motion';
import { FiCopy, FiCheck, FiUpload, FiClock, FiCheckCircle, FiXCircle, FiArrowLeft } from 'react-icons/fi';

interface PlanInfo {
  nombre: string;
  tipo: string;
  precioARS: number;
  precioUSDT: number;
  disponible: boolean;
}

interface PagoInfo {
  planActual: string;
  nombreNegocio: string;
  planes: PlanInfo[];
  metodosPago: {
    cvu: string;
    usdtBep20: string;
    usdtTrc20: string;
  };
}

interface Solicitud {
  solicitudPagoID: number;
  planSolicitado: string;
  metodoPago: string;
  montoDeclarado: number;
  monedaPago: string;
  estado: string;
  motivoRechazo: string | null;
  fechaSolicitud: string;
  fechaResolucion: string | null;
}

type MetodoPago = 'CVU_ARS' | 'USDT_BEP20' | 'USDT_TRC20';

export default function Upgrade() {
  const [info, setInfo] = useState<PagoInfo | null>(null);
  const [solicitudes, setSolicitudes] = useState<Solicitud[]>([]);
  const [loading, setLoading] = useState(true);
  const [planSeleccionado, setPlanSeleccionado] = useState<string | null>(null);
  const [metodoPago, setMetodoPago] = useState<MetodoPago>('CVU_ARS');
  const [referencia, setReferencia] = useState('');
  const [monto, setMonto] = useState('');
  const [archivo, setArchivo] = useState<File | null>(null);
  const [enviando, setEnviando] = useState(false);
  const [mensaje, setMensaje] = useState('');
  const [error, setError] = useState('');
  const [copiado, setCopiado] = useState('');
  const fileRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    cargar();
  }, []);

  const cargar = async () => {
    try {
      const [infoRes, solRes] = await Promise.all([
        pagosService.obtenerInfo(),
        pagosService.misSolicitudes()
      ]);
      setInfo(infoRes.data);
      setSolicitudes(solRes.data);

      // Auto-setear monto segun plan y metodo
    } catch {
      setError('Error al cargar informacion');
    } finally {
      setLoading(false);
    }
  };

  const copiar = (texto: string, label: string) => {
    navigator.clipboard.writeText(texto);
    setCopiado(label);
    setTimeout(() => setCopiado(''), 2000);
  };

  const getPrecio = () => {
    if (!info || !planSeleccionado) return 0;
    const plan = info.planes.find(p => p.nombre === planSeleccionado);
    if (!plan) return 0;
    return metodoPago === 'CVU_ARS' ? plan.precioARS : plan.precioUSDT;
  };

  const getMoneda = () => metodoPago === 'CVU_ARS' ? 'ARS' : 'USDT';

  const getDireccion = () => {
    if (!info) return '';
    if (metodoPago === 'CVU_ARS') return info.metodosPago.cvu;
    if (metodoPago === 'USDT_BEP20') return info.metodosPago.usdtBep20;
    return info.metodosPago.usdtTrc20;
  };

  const enviarSolicitud = async () => {
    if (!planSeleccionado || !archivo) {
      setError('Selecciona un plan y subi el comprobante');
      return;
    }

    setEnviando(true);
    setError('');
    setMensaje('');

    const formData = new FormData();
    formData.append('PlanSolicitado', planSeleccionado);
    formData.append('MetodoPago', metodoPago);
    formData.append('MontoDeclarado', monto || getPrecio().toString());
    formData.append('Comprobante', archivo);
    if (referencia) formData.append('ReferenciaTransferencia', referencia);

    try {
      const res = await pagosService.solicitar(formData);
      setMensaje(res.data.mensaje);
      setPlanSeleccionado(null);
      setArchivo(null);
      setReferencia('');
      setMonto('');
      cargar();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Error al enviar solicitud');
    } finally {
      setEnviando(false);
    }
  };

  const tienePendiente = solicitudes.some(s => s.estado === 'Pendiente');

  if (loading) return <div className="page-loading">Cargando...</div>;
  if (!info) return <div className="page-error">Error al cargar datos</div>;

  const planActualLabel = info.planActual === 'Pro+' ? 'Pro+ (Anual)' : info.planActual === 'Pro' ? 'Pro (Mensual)' : info.planActual;
  const yaEsMax = info.planActual === 'Pro+' || info.planActual === 'ProForever';

  return (
    <div className="upgrade-page">
      <div className="upgrade-header">
        <h1>Upgrade de Plan</h1>
        <p>Plan actual: <strong>{planActualLabel}</strong></p>
      </div>

      {mensaje && (
        <motion.div className="upgrade-success" initial={{ opacity: 0, y: -10 }} animate={{ opacity: 1, y: 0 }}>
          <FiCheckCircle /> {mensaje}
        </motion.div>
      )}

      {error && (
        <motion.div className="upgrade-error" initial={{ opacity: 0, y: -10 }} animate={{ opacity: 1, y: 0 }}>
          <FiXCircle /> {error}
        </motion.div>
      )}

      {yaEsMax ? (
        <div className="upgrade-max">Ya tenes el mejor plan web disponible.</div>
      ) : tienePendiente ? (
        <div className="upgrade-pendiente">
          <FiClock /> Tu solicitud de upgrade esta siendo revisada. Te notificaremos pronto.
        </div>
      ) : !planSeleccionado ? (
        /* Paso 1: Elegir plan */
        <div className="upgrade-planes">
          {info.planes.filter(p => p.disponible).map(plan => (
            <motion.div
              key={plan.nombre}
              className="upgrade-plan-card"
              whileHover={{ scale: 1.02 }}
              onClick={() => {
                setPlanSeleccionado(plan.nombre);
                setMonto(metodoPago === 'CVU_ARS' ? plan.precioARS.toString() : plan.precioUSDT.toString());
              }}
            >
              <h3>{plan.nombre}</h3>
              <span className="upgrade-plan-tipo">{plan.tipo}</span>
              <div className="upgrade-plan-precios">
                <div className="upgrade-precio">
                  <span className="upgrade-precio-valor">${plan.precioARS.toLocaleString('es-AR')}</span>
                  <span className="upgrade-precio-moneda">ARS</span>
                </div>
                <div className="upgrade-precio-sep">o</div>
                <div className="upgrade-precio">
                  <span className="upgrade-precio-valor">${plan.precioUSDT}</span>
                  <span className="upgrade-precio-moneda">USDT</span>
                </div>
              </div>
              <button className="btn btn-primary">Seleccionar</button>
            </motion.div>
          ))}
        </div>
      ) : (
        /* Paso 2: Pagar y subir comprobante */
        <motion.div className="upgrade-pago" initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
          <button className="upgrade-back" onClick={() => setPlanSeleccionado(null)}>
            <FiArrowLeft /> Cambiar plan
          </button>

          <h2>Plan {planSeleccionado} - {getMoneda() === 'ARS' ? `$${getPrecio().toLocaleString('es-AR')} ARS` : `$${getPrecio()} USDT`}</h2>

          {/* Metodo de pago */}
          <div className="upgrade-metodos">
            <label className="upgrade-metodo-label">Metodo de pago:</label>
            <div className="upgrade-metodo-btns">
              {(['CVU_ARS', 'USDT_BEP20', 'USDT_TRC20'] as MetodoPago[]).map(m => (
                <button
                  key={m}
                  className={`upgrade-metodo-btn ${metodoPago === m ? 'active' : ''}`}
                  onClick={() => {
                    setMetodoPago(m);
                    const plan = info.planes.find(p => p.nombre === planSeleccionado);
                    if (plan) setMonto(m === 'CVU_ARS' ? plan.precioARS.toString() : plan.precioUSDT.toString());
                  }}
                >
                  {m === 'CVU_ARS' ? 'Transferencia ARS' : m === 'USDT_BEP20' ? 'USDT (BEP20)' : 'USDT (TRC20)'}
                </button>
              ))}
            </div>
          </div>

          {/* Datos de pago */}
          <div className="upgrade-datos-pago">
            <label>{metodoPago === 'CVU_ARS' ? 'CVU:' : `Direccion ${metodoPago === 'USDT_BEP20' ? 'BEP20' : 'TRC20'}:`}</label>
            <div className="upgrade-direccion">
              <code>{getDireccion()}</code>
              <button className="upgrade-copiar" onClick={() => copiar(getDireccion(), metodoPago)}>
                {copiado === metodoPago ? <FiCheck /> : <FiCopy />}
              </button>
            </div>
            <p className="upgrade-monto-label">
              Monto a transferir: <strong>{getMoneda() === 'ARS' ? `$${getPrecio().toLocaleString('es-AR')}` : `$${getPrecio()}`} {getMoneda()}</strong>
            </p>
          </div>

          {/* Formulario */}
          <div className="upgrade-form">
            <div className="upgrade-form-group">
              <label>Monto transferido</label>
              <input
                type="number"
                value={monto}
                onChange={e => setMonto(e.target.value)}
                placeholder={`Monto en ${getMoneda()}`}
              />
            </div>

            <div className="upgrade-form-group">
              <label>Referencia / Hash (opcional)</label>
              <input
                type="text"
                value={referencia}
                onChange={e => setReferencia(e.target.value)}
                placeholder="Nro de operacion o TX hash"
              />
            </div>

            <div className="upgrade-form-group">
              <label>Comprobante de pago *</label>
              <div
                className={`upgrade-upload ${archivo ? 'has-file' : ''}`}
                onClick={() => fileRef.current?.click()}
              >
                <input
                  ref={fileRef}
                  type="file"
                  accept="image/png,image/jpeg,image/jpg"
                  onChange={e => setArchivo(e.target.files?.[0] || null)}
                  style={{ display: 'none' }}
                />
                {archivo ? (
                  <span><FiCheck /> {archivo.name}</span>
                ) : (
                  <span><FiUpload /> Click para subir captura (JPG/PNG, max 5MB)</span>
                )}
              </div>
            </div>

            <button className="btn btn-primary upgrade-enviar" onClick={enviarSolicitud} disabled={enviando || !archivo}>
              {enviando ? 'Enviando...' : 'Enviar solicitud de upgrade'}
            </button>
          </div>
        </motion.div>
      )}

      {/* Historial de solicitudes */}
      {solicitudes.length > 0 && (
        <div className="upgrade-historial">
          <h3>Historial de solicitudes</h3>
          <table className="upgrade-table">
            <thead>
              <tr>
                <th>Plan</th>
                <th>Metodo</th>
                <th>Monto</th>
                <th>Estado</th>
                <th>Fecha</th>
              </tr>
            </thead>
            <tbody>
              {solicitudes.map(s => (
                <tr key={s.solicitudPagoID}>
                  <td>{s.planSolicitado}</td>
                  <td>{s.metodoPago === 'CVU_ARS' ? 'ARS' : s.metodoPago.replace('USDT_', '')}</td>
                  <td>{s.monedaPago === 'ARS' ? `$${s.montoDeclarado.toLocaleString('es-AR')}` : `$${s.montoDeclarado} USDT`}</td>
                  <td>
                    <span className={`upgrade-estado ${s.estado.toLowerCase()}`}>
                      {s.estado === 'Pendiente' && <FiClock />}
                      {s.estado === 'Aprobada' && <FiCheckCircle />}
                      {s.estado === 'Rechazada' && <FiXCircle />}
                      {s.estado}
                    </span>
                    {s.motivoRechazo && <p className="upgrade-rechazo">{s.motivoRechazo}</p>}
                  </td>
                  <td>{new Date(s.fechaSolicitud).toLocaleDateString('es-AR')}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
