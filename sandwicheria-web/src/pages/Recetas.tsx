import { useState, useEffect } from 'react';
import { recetasService, productosService, categoriasService } from '../services/api';
import { motion } from 'framer-motion';
import { FiBookOpen, FiPlus, FiEdit2, FiTrash2, FiSearch, FiRefreshCw, FiList } from 'react-icons/fi';

interface Receta {
  recetaID: number;
  nombre: string;
  descripcion: string;
  precio: number;
  stockActual: number;
  categoriaID: number;
  categoriaNombre: string;
  ingredientes?: Ingrediente[];
}

interface Ingrediente {
  ingredienteRecetaID?: number;
  productoMercaderiaID: number;
  productoNombre?: string;
  cantidad: number;
  unidadMedida: string;
}

export default function Recetas() {
  const [recetas, setRecetas] = useState<Receta[]>([]);
  const [categorias, setCategorias] = useState<any[]>([]);
  const [mercaderia, setMercaderia] = useState<any[]>([]);
  const [busqueda, setBusqueda] = useState('');
  const [loading, setLoading] = useState(true);
  const [detalleReceta, setDetalleReceta] = useState<Receta | null>(null);

  const [showForm, setShowForm] = useState(false);
  const [editando, setEditando] = useState<Receta | null>(null);
  const [form, setForm] = useState({ nombre: '', descripcion: '', precio: '', stockActual: '0', categoriaID: '' });
  const [ingredientes, setIngredientes] = useState<Ingrediente[]>([]);

  useEffect(() => { cargar(); }, []);

  const cargar = async () => {
    setLoading(true);
    try {
      const [recRes, catRes, mercRes] = await Promise.all([
        recetasService.obtenerTodas(),
        categoriasService.obtenerMenu(),
        productosService.obtenerMercaderia(),
      ]);
      setRecetas(recRes.data || []);
      setCategorias(catRes.data || []);
      setMercaderia(mercRes.data || []);
    } catch {} finally { setLoading(false); }
  };

  const recetasFiltradas = recetas.filter(r =>
    r.nombre.toLowerCase().includes(busqueda.toLowerCase())
  );

  const verDetalle = async (receta: Receta) => {
    try {
      const res = await recetasService.obtenerIngredientes(receta.recetaID);
      setDetalleReceta({ ...receta, ingredientes: res.data || [] });
    } catch {
      setDetalleReceta(receta);
    }
  };

  const abrirForm = (receta?: Receta) => {
    if (receta) {
      setEditando(receta);
      setForm({
        nombre: receta.nombre,
        descripcion: receta.descripcion || '',
        precio: String(receta.precio),
        stockActual: String(receta.stockActual),
        categoriaID: String(receta.categoriaID),
      });
      // Cargar ingredientes
      recetasService.obtenerIngredientes(receta.recetaID)
        .then(res => setIngredientes(res.data || []))
        .catch(() => setIngredientes([]));
    } else {
      setEditando(null);
      setForm({ nombre: '', descripcion: '', precio: '', stockActual: '0', categoriaID: categorias[0]?.categoriaID?.toString() || '' });
      setIngredientes([]);
    }
    setShowForm(true);
  };

  const agregarIngrediente = () => {
    if (mercaderia.length === 0) return;
    setIngredientes([...ingredientes, {
      productoMercaderiaID: mercaderia[0].productoID,
      cantidad: 1,
      unidadMedida: mercaderia[0].unidadMedida || 'Unidad',
    }]);
  };

  const actualizarIngrediente = (index: number, campo: string, valor: any) => {
    const nuevos = [...ingredientes];
    (nuevos[index] as any)[campo] = valor;
    if (campo === 'productoMercaderiaID') {
      const prod = mercaderia.find(m => m.productoID === Number(valor));
      if (prod) nuevos[index].unidadMedida = prod.unidadMedida || 'Unidad';
    }
    setIngredientes(nuevos);
  };

  const eliminarIngrediente = (index: number) => {
    setIngredientes(ingredientes.filter((_, i) => i !== index));
  };

  const guardar = async () => {
    const data = {
      nombre: form.nombre,
      descripcion: form.descripcion,
      precio: parseFloat(form.precio),
      stockActual: parseInt(form.stockActual),
      categoriaID: parseInt(form.categoriaID),
      ingredientes: ingredientes.map(i => ({
        productoMercaderiaID: Number(i.productoMercaderiaID),
        cantidad: Number(i.cantidad),
        unidadMedida: i.unidadMedida,
      })),
    };
    try {
      if (editando) {
        await recetasService.actualizar(editando.recetaID, { ...data, recetaID: editando.recetaID });
      } else {
        await recetasService.crear(data);
      }
      setShowForm(false);
      cargar();
    } catch (err: any) {
      alert(err.response?.data?.error || 'Error al guardar');
    }
  };

  const eliminar = async (id: number) => {
    if (!window.confirm('Eliminar esta receta?')) return;
    try {
      await recetasService.eliminar(id);
      cargar();
    } catch (err: any) {
      alert(err.response?.data?.error || 'Error al eliminar');
    }
  };

  const formatMoney = (n: number) => `$${(n || 0).toLocaleString('es-AR')}`;

  return (
    <div className="page-recetas">
      <h2 className="page-title"><FiBookOpen /> Gestion de Recetas</h2>

      <div className="toolbar">
        <button className="btn btn-orange" onClick={() => abrirForm()}>
          <FiPlus /> Nueva Receta
        </button>
        <div className="toolbar-right">
          <div className="search-bar">
            <FiSearch />
            <input placeholder="Buscar receta..." value={busqueda} onChange={e => setBusqueda(e.target.value)} />
          </div>
          <button className="btn btn-ghost" onClick={cargar}><FiRefreshCw /></button>
        </div>
      </div>

      <div className="section-card">
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Categoria</th>
                <th>Precio</th>
                <th>Stock</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr><td colSpan={5} className="text-center">Cargando...</td></tr>
              ) : recetasFiltradas.map(r => (
                <motion.tr key={r.recetaID} initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
                  <td>
                    <div className="cell-name">
                      <strong>{r.nombre}</strong>
                      {r.descripcion && <small>{r.descripcion}</small>}
                    </div>
                  </td>
                  <td>{r.categoriaNombre}</td>
                  <td className="text-success font-bold">{formatMoney(r.precio)}</td>
                  <td><span className="stock-badge ok">{r.stockActual}</span></td>
                  <td>
                    <div className="actions">
                      <button className="btn-icon info" onClick={() => verDetalle(r)} title="Ver ingredientes"><FiList /></button>
                      <button className="btn-icon edit" onClick={() => abrirForm(r)}><FiEdit2 /></button>
                      <button className="btn-icon delete" onClick={() => eliminar(r.recetaID)}><FiTrash2 /></button>
                    </div>
                  </td>
                </motion.tr>
              ))}
              {!loading && recetasFiltradas.length === 0 && (
                <tr><td colSpan={5} className="text-center text-muted">No hay recetas</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Modal Detalle Ingredientes */}
      {detalleReceta && (
        <div className="modal-overlay" onClick={() => setDetalleReceta(null)}>
          <motion.div className="modal-content" onClick={e => e.stopPropagation()}
            initial={{ opacity: 0, scale: 0.9 }} animate={{ opacity: 1, scale: 1 }}>
            <h3>Ingredientes de: {detalleReceta.nombre}</h3>
            <table className="data-table">
              <thead>
                <tr><th>Insumo</th><th>Cantidad</th><th>Unidad</th></tr>
              </thead>
              <tbody>
                {(detalleReceta.ingredientes || []).map((ing, i) => (
                  <tr key={i}>
                    <td>{ing.productoNombre || `Producto #${ing.productoMercaderiaID}`}</td>
                    <td>{ing.cantidad}</td>
                    <td>{ing.unidadMedida}</td>
                  </tr>
                ))}
                {(!detalleReceta.ingredientes || detalleReceta.ingredientes.length === 0) && (
                  <tr><td colSpan={3} className="text-center text-muted">Sin ingredientes</td></tr>
                )}
              </tbody>
            </table>
            <div className="modal-actions">
              <button className="btn btn-ghost" onClick={() => setDetalleReceta(null)}>Cerrar</button>
            </div>
          </motion.div>
        </div>
      )}

      {/* Modal Form */}
      {showForm && (
        <div className="modal-overlay" onClick={() => setShowForm(false)}>
          <motion.div className="modal-content modal-large" onClick={e => e.stopPropagation()}
            initial={{ opacity: 0, scale: 0.9 }} animate={{ opacity: 1, scale: 1 }}>
            <h3>{editando ? 'Editar Receta' : 'Nueva Receta'}</h3>
            <div className="form-grid">
              <div className="form-group">
                <label>Nombre</label>
                <input value={form.nombre} onChange={e => setForm({ ...form, nombre: e.target.value })} />
              </div>
              <div className="form-group">
                <label>Descripcion</label>
                <input value={form.descripcion} onChange={e => setForm({ ...form, descripcion: e.target.value })} />
              </div>
              <div className="form-group">
                <label>Categoria</label>
                <select value={form.categoriaID} onChange={e => setForm({ ...form, categoriaID: e.target.value })}>
                  {categorias.map(c => (
                    <option key={c.categoriaID} value={c.categoriaID}>{c.nombre}</option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <label>Precio</label>
                <input type="number" value={form.precio} onChange={e => setForm({ ...form, precio: e.target.value })} />
              </div>
              <div className="form-group">
                <label>Stock Preparado</label>
                <input type="number" value={form.stockActual} onChange={e => setForm({ ...form, stockActual: e.target.value })} />
              </div>
            </div>

            <h4 style={{ marginTop: '1.5rem' }}>Ingredientes</h4>
            <table className="data-table compact">
              <thead>
                <tr><th>Insumo</th><th>Cantidad</th><th>Unidad</th><th></th></tr>
              </thead>
              <tbody>
                {ingredientes.map((ing, i) => (
                  <tr key={i}>
                    <td>
                      <select value={ing.productoMercaderiaID} onChange={e => actualizarIngrediente(i, 'productoMercaderiaID', e.target.value)}>
                        {mercaderia.map(m => (
                          <option key={m.productoID} value={m.productoID}>{m.nombre}</option>
                        ))}
                      </select>
                    </td>
                    <td>
                      <input type="number" step="0.1" value={ing.cantidad}
                        onChange={e => actualizarIngrediente(i, 'cantidad', e.target.value)} style={{ width: '80px' }} />
                    </td>
                    <td>{ing.unidadMedida}</td>
                    <td>
                      <button className="btn-icon delete" onClick={() => eliminarIngrediente(i)}><FiTrash2 /></button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            <button className="btn btn-ghost" onClick={agregarIngrediente} style={{ marginTop: '0.5rem' }}>
              <FiPlus /> Agregar Ingrediente
            </button>

            <div className="modal-actions">
              <button className="btn btn-ghost" onClick={() => setShowForm(false)}>Cancelar</button>
              <button className="btn btn-orange" onClick={guardar}>
                {editando ? 'Actualizar' : 'Crear'}
              </button>
            </div>
          </motion.div>
        </div>
      )}
    </div>
  );
}
