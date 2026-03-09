import { useState, useEffect } from 'react';
import { productosService, categoriasService } from '../services/api';
import { motion } from 'framer-motion';
import { FiGrid, FiPlus, FiEdit2, FiTrash2, FiSearch, FiRefreshCw } from 'react-icons/fi';

interface Producto {
  productoID: number;
  nombre: string;
  descripcion: string;
  precio: number;
  stockActual: number;
  stockMinimo: number;
  categoriaNombre: string;
  categoriaID: number;
  codigoBarras: string;
}

interface Categoria {
  categoriaID: number;
  nombre: string;
}

export default function Menu() {
  const [productos, setProductos] = useState<Producto[]>([]);
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [busqueda, setBusqueda] = useState('');
  const [categoriaFiltro, setCategoriaFiltro] = useState(0);
  const [loading, setLoading] = useState(true);

  // Form state
  const [showForm, setShowForm] = useState(false);
  const [editando, setEditando] = useState<Producto | null>(null);
  const [form, setForm] = useState({ nombre: '', descripcion: '', precio: '', stockActual: '', stockMinimo: '', categoriaID: '', codigoBarras: '' });

  useEffect(() => { cargar(); }, []);

  const cargar = async () => {
    setLoading(true);
    try {
      const [prodRes, catRes] = await Promise.all([
        productosService.obtenerMenu(),
        categoriasService.obtenerMenu(),
      ]);
      setProductos(prodRes.data || []);
      setCategorias(catRes.data || []);
    } catch {} finally { setLoading(false); }
  };

  const productosFiltrados = productos.filter(p => {
    const matchBusqueda = p.nombre.toLowerCase().includes(busqueda.toLowerCase());
    const matchCategoria = categoriaFiltro === 0 || p.categoriaID === categoriaFiltro;
    return matchBusqueda && matchCategoria;
  });

  const abrirForm = (prod?: Producto) => {
    if (prod) {
      setEditando(prod);
      setForm({
        nombre: prod.nombre,
        descripcion: prod.descripcion || '',
        precio: String(prod.precio),
        stockActual: String(prod.stockActual),
        stockMinimo: String(prod.stockMinimo),
        categoriaID: String(prod.categoriaID),
        codigoBarras: prod.codigoBarras || '',
      });
    } else {
      setEditando(null);
      setForm({ nombre: '', descripcion: '', precio: '', stockActual: '', stockMinimo: '5', categoriaID: categorias[0]?.categoriaID?.toString() || '', codigoBarras: '' });
    }
    setShowForm(true);
  };

  const guardar = async () => {
    const data = {
      nombre: form.nombre,
      descripcion: form.descripcion,
      precio: parseFloat(form.precio),
      stockActual: parseInt(form.stockActual),
      stockMinimo: parseInt(form.stockMinimo),
      categoriaID: parseInt(form.categoriaID),
      codigoBarras: form.codigoBarras,
      tipoProducto: 'Menu',
      unidadMedida: 'Unidad',
    };
    try {
      if (editando) {
        await productosService.actualizar(editando.productoID, { ...data, productoID: editando.productoID });
      } else {
        await productosService.crear(data);
      }
      setShowForm(false);
      cargar();
    } catch (err: any) {
      alert(err.response?.data?.error || 'Error al guardar');
    }
  };

  const eliminar = async (id: number) => {
    if (!window.confirm('Eliminar este producto?')) return;
    try {
      await productosService.eliminar(id);
      cargar();
    } catch (err: any) {
      alert(err.response?.data?.error || 'Error al eliminar');
    }
  };

  const formatMoney = (n: number) => `$${(n || 0).toLocaleString('es-AR')}`;

  return (
    <div className="page-menu">
      <h2 className="page-title"><FiGrid /> Gestion de Menu</h2>

      {/* Toolbar */}
      <div className="toolbar">
        <button className="btn btn-primary" onClick={() => abrirForm()}>
          <FiPlus /> Nuevo Producto
        </button>
        <div className="toolbar-right">
          <div className="search-bar">
            <FiSearch />
            <input placeholder="Buscar..." value={busqueda} onChange={e => setBusqueda(e.target.value)} />
          </div>
          <select value={categoriaFiltro} onChange={e => setCategoriaFiltro(Number(e.target.value))}>
            <option value={0}>Todas las categorias</option>
            {categorias.map(c => (
              <option key={c.categoriaID} value={c.categoriaID}>{c.nombre}</option>
            ))}
          </select>
          <button className="btn btn-ghost" onClick={cargar}><FiRefreshCw /></button>
        </div>
      </div>

      {/* Tabla */}
      <div className="section-card">
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Categoria</th>
                <th>Precio</th>
                <th>Stock</th>
                <th>Min</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr><td colSpan={6} className="text-center">Cargando...</td></tr>
              ) : productosFiltrados.map(p => (
                <motion.tr key={p.productoID} initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
                  <td>
                    <div className="cell-name">
                      <strong>{p.nombre}</strong>
                      {p.descripcion && <small>{p.descripcion}</small>}
                    </div>
                  </td>
                  <td>{p.categoriaNombre}</td>
                  <td className="text-success font-bold">{formatMoney(p.precio)}</td>
                  <td>
                    <span className={`stock-badge ${p.stockActual <= p.stockMinimo ? 'low' : 'ok'}`}>
                      {p.stockActual}
                    </span>
                  </td>
                  <td><span className="stock-badge min">{p.stockMinimo}</span></td>
                  <td>
                    <div className="actions">
                      <button className="btn-icon edit" onClick={() => abrirForm(p)}><FiEdit2 /></button>
                      <button className="btn-icon delete" onClick={() => eliminar(p.productoID)}><FiTrash2 /></button>
                    </div>
                  </td>
                </motion.tr>
              ))}
              {!loading && productosFiltrados.length === 0 && (
                <tr><td colSpan={6} className="text-center text-muted">No hay productos</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Modal Form */}
      {showForm && (
        <div className="modal-overlay" onClick={() => setShowForm(false)}>
          <motion.div
            className="modal-content"
            onClick={e => e.stopPropagation()}
            initial={{ opacity: 0, scale: 0.9 }}
            animate={{ opacity: 1, scale: 1 }}
          >
            <h3>{editando ? 'Editar Producto' : 'Nuevo Producto'}</h3>
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
                <label>Stock Actual</label>
                <input type="number" value={form.stockActual} onChange={e => setForm({ ...form, stockActual: e.target.value })} />
              </div>
              <div className="form-group">
                <label>Stock Minimo</label>
                <input type="number" value={form.stockMinimo} onChange={e => setForm({ ...form, stockMinimo: e.target.value })} />
              </div>
              <div className="form-group">
                <label>Codigo de Barras</label>
                <input value={form.codigoBarras} onChange={e => setForm({ ...form, codigoBarras: e.target.value })} />
              </div>
            </div>
            <div className="modal-actions">
              <button className="btn btn-ghost" onClick={() => setShowForm(false)}>Cancelar</button>
              <button className="btn btn-primary" onClick={guardar}>
                {editando ? 'Actualizar' : 'Crear'}
              </button>
            </div>
          </motion.div>
        </div>
      )}
    </div>
  );
}
