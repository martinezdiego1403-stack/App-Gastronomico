import { useState, useEffect, useCallback } from 'react';
import { productosService, recetasService, ventasService, cajasService } from '../services/api';
import { motion, AnimatePresence } from 'framer-motion';
import { FiShoppingCart, FiSearch, FiTrash2, FiPlus, FiMinus, FiCheck } from 'react-icons/fi';

interface ItemVenta {
  id: number;
  nombre: string;
  precio: number;
  cantidad: number;
  tipo: 'producto' | 'receta';
}

export default function Ventas() {
  const [productos, setProductos] = useState<any[]>([]);
  const [recetas, setRecetas] = useState<any[]>([]);
  const [busqueda, setBusqueda] = useState('');
  const [carrito, setCarrito] = useState<ItemVenta[]>([]);
  const [metodoPago, setMetodoPago] = useState('Efectivo');
  const [montoPagado, setMontoPagado] = useState('');
  const [cajaAbiertaId, setCajaAbiertaId] = useState<number | null>(null);
  const [mensaje, setMensaje] = useState('');

  useEffect(() => {
    cargarDatos();
  }, []);

  const cargarDatos = async () => {
    try {
      const [prodRes, recRes, cajaRes] = await Promise.all([
        productosService.obtenerMenu(),
        recetasService.obtenerTodas(),
        cajasService.obtenerAbierta().catch(() => null),
      ]);
      setProductos(prodRes.data || []);
      setRecetas(recRes.data || []);
      setCajaAbiertaId(cajaRes?.data?.hayCajaAbierta ? cajaRes.data.caja.cajaID : null);
    } catch {}
  };

  const itemsDisponibles = useCallback(() => {
    const term = busqueda.toLowerCase();
    const prods = productos
      .filter(p => p.nombre.toLowerCase().includes(term))
      .map(p => ({ ...p, tipo: 'producto' as const }));
    const recs = recetas
      .filter(r => r.nombre.toLowerCase().includes(term))
      .map(r => ({ ...r, tipo: 'receta' as const, id: r.recetaID || r.id }));
    return [...prods, ...recs];
  }, [productos, recetas, busqueda]);

  const agregarAlCarrito = (item: any) => {
    const id = item.productoID || item.recetaID || item.id;
    setCarrito(prev => {
      const existe = prev.find(c => c.id === id && c.tipo === item.tipo);
      if (existe) {
        return prev.map(c =>
          c.id === id && c.tipo === item.tipo
            ? { ...c, cantidad: c.cantidad + 1 }
            : c
        );
      }
      return [...prev, { id, nombre: item.nombre, precio: item.precio, cantidad: 1, tipo: item.tipo }];
    });
  };

  const cambiarCantidad = (id: number, tipo: string, delta: number) => {
    setCarrito(prev =>
      prev
        .map(c => c.id === id && c.tipo === tipo ? { ...c, cantidad: c.cantidad + delta } : c)
        .filter(c => c.cantidad > 0)
    );
  };

  const eliminarDelCarrito = (id: number, tipo: string) => {
    setCarrito(prev => prev.filter(c => !(c.id === id && c.tipo === tipo)));
  };

  const total = carrito.reduce((sum, c) => sum + c.precio * c.cantidad, 0);
  const vuelto = parseFloat(montoPagado) - total;

  const registrarVenta = async () => {
    if (carrito.length === 0) { setMensaje('Agrega productos al carrito'); return; }
    if (!cajaAbiertaId) { setMensaje('Debes abrir la caja primero'); return; }

    try {
      const detalles = carrito.map(c => ({
        productoID: c.tipo === 'producto' ? c.id : null,
        recetaID: c.tipo === 'receta' ? c.id : null,
        nombreReceta: c.tipo === 'receta' ? c.nombre : null,
        cantidad: c.cantidad,
        precioUnitario: c.precio,
      }));
      await ventasService.registrar({
        cajaID: cajaAbiertaId,
        metodoPago,
        total,
        montoPagado: parseFloat(montoPagado) || total,
        detalles,
      });
      setCarrito([]);
      setMontoPagado('');
      setMensaje('Venta registrada con exito!');
      setTimeout(() => setMensaje(''), 3000);
      cargarDatos();
    } catch (err: any) {
      setMensaje(err.response?.data?.error || 'Error al registrar venta');
    }
  };

  const formatMoney = (n: number) => `$${n.toLocaleString('es-AR')}`;

  return (
    <div className="page-ventas">
      <h2 className="page-title"><FiShoppingCart /> Punto de Venta</h2>

      {!cajaAbiertaId && (
        <div className="alert alert-warning">
          No hay caja abierta. Abre la caja antes de registrar ventas.
        </div>
      )}

      <div className="ventas-layout">
        {/* Catalogo */}
        <div className="ventas-catalogo">
          <div className="search-bar">
            <FiSearch />
            <input
              type="text"
              placeholder="Buscar producto o receta..."
              value={busqueda}
              onChange={e => setBusqueda(e.target.value)}
            />
          </div>
          <div className="catalogo-grid">
            {itemsDisponibles().map(item => {
              const id = item.productoID || item.recetaID || item.id;
              return (
                <motion.div
                  key={`${item.tipo}-${id}`}
                  className={`catalogo-item ${item.tipo}`}
                  whileHover={{ scale: 1.02 }}
                  whileTap={{ scale: 0.98 }}
                  onClick={() => agregarAlCarrito(item)}
                >
                  <div className="catalogo-item-type">
                    {item.tipo === 'receta' ? 'R' : 'P'}
                  </div>
                  <div className="catalogo-item-info">
                    <span className="catalogo-item-name">{item.nombre}</span>
                    <span className="catalogo-item-price">{formatMoney(item.precio)}</span>
                  </div>
                </motion.div>
              );
            })}
          </div>
        </div>

        {/* Carrito */}
        <div className="ventas-carrito">
          <h3><FiShoppingCart /> Carrito ({carrito.length})</h3>

          <div className="carrito-items">
            <AnimatePresence>
              {carrito.map(item => (
                <motion.div
                  key={`${item.tipo}-${item.id}`}
                  className="carrito-item"
                  initial={{ opacity: 0, x: 20 }}
                  animate={{ opacity: 1, x: 0 }}
                  exit={{ opacity: 0, x: -20 }}
                >
                  <div className="carrito-item-info">
                    <span className="carrito-item-name">{item.nombre}</span>
                    <span className="carrito-item-subtotal">{formatMoney(item.precio * item.cantidad)}</span>
                  </div>
                  <div className="carrito-item-controls">
                    <button onClick={() => cambiarCantidad(item.id, item.tipo, -1)}><FiMinus /></button>
                    <span>{item.cantidad}</span>
                    <button onClick={() => cambiarCantidad(item.id, item.tipo, 1)}><FiPlus /></button>
                    <button className="btn-remove" onClick={() => eliminarDelCarrito(item.id, item.tipo)}>
                      <FiTrash2 />
                    </button>
                  </div>
                </motion.div>
              ))}
            </AnimatePresence>
            {carrito.length === 0 && (
              <p className="text-muted text-center">Carrito vacio</p>
            )}
          </div>

          <div className="carrito-footer">
            <div className="carrito-total">
              <span>Total</span>
              <strong>{formatMoney(total)}</strong>
            </div>

            <div className="carrito-pago">
              <select value={metodoPago} onChange={e => setMetodoPago(e.target.value)}>
                <option>Efectivo</option>
                <option>Tarjeta</option>
                <option>Transferencia</option>
              </select>
              {metodoPago === 'Efectivo' && (
                <input
                  type="number"
                  placeholder="Monto pagado"
                  value={montoPagado}
                  onChange={e => setMontoPagado(e.target.value)}
                />
              )}
              {metodoPago === 'Efectivo' && montoPagado && vuelto >= 0 && (
                <div className="carrito-vuelto">Vuelto: {formatMoney(vuelto)}</div>
              )}
            </div>

            {mensaje && <div className="carrito-mensaje">{mensaje}</div>}

            <button className="btn btn-success btn-block" onClick={registrarVenta} disabled={!cajaAbiertaId}>
              <FiCheck /> Registrar Venta
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
