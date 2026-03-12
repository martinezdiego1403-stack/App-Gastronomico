import { useState, useEffect } from 'react';
import { productosService, categoriasService } from '../services/api';
import { motion } from 'framer-motion';
import { FiPackage, FiPlus, FiEdit2, FiTrash2, FiSearch, FiRefreshCw, FiX, FiCheck } from 'react-icons/fi';

interface Producto {
  productoID: number;
  nombre: string;
  descripcion: string;
  precio: number;
  stockActual: number;
  stockMinimo: number;
  categoriaNombre: string;
  categoriaID: number;
  unidadMedida: string;
}

interface Categoria {
  categoriaID: number;
  nombre: string;
}

export default function Mercaderia() {
  const [productos, setProductos] = useState<Producto[]>([]);
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [busqueda, setBusqueda] = useState('');
  const [categoriaFiltro, setCategoriaFiltro] = useState(0);
  const [loading, setLoading] = useState(true);

  const [showForm, setShowForm] = useState(false);
  const [editando, setEditando] = useState<Producto | null>(null);
  const [form, setForm] = useState({ nombre: '', descripcion: '', precio: '', stockActual: '', stockMinimo: '', categoriaID: '', unidadMedida: 'Unidad' });
  const [creandoCategoria, setCreandoCategoria] = useState(false);
  const [nuevaCategoriaNombre, setNuevaCategoriaNombre] = useState('');

  useEffect(() => { cargar(); }, []);

  const cargar = async () => {
    setLoading(true);
    try {
      const [prodRes, catRes] = await Promise.all([
        productosService.obtenerMercaderia(),
        categoriasService.obtenerMercaderia(),
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
        unidadMedida: prod.unidadMedida || 'Unidad',
      });
    } else {
      setEditando(null);
      setForm({ nombre: '', descripcion: '', precio: '', stockActual: '', stockMinimo: '5', categoriaID: categorias[0]?.categoriaID?.toString() || '', unidadMedida: 'Unidad' });
    }
    setShowForm(true);
  };

  const guardar = async () => {
    const data = {
      nombre: form.nombre,
      descripcion: form.descripcion,
      precio: parseFloat(form.precio),
      stockActual: parseFloat(form.stockActual),
      stockMinimo: parseFloat(form.stockMinimo),
      categoriaID: parseInt(form.categoriaID),
      tipoProducto: 'Mercaderia',
      unidadMedida: form.unidadMedida,
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
    if (!window.confirm('Eliminar este insumo?')) return;
    try {
      await productosService.eliminar(id);
      cargar();
    } catch (err: any) {
      alert(err.response?.data?.error || 'Error al eliminar');
    }
  };

  const crearCategoria = async () => {
    if (!nuevaCategoriaNombre.trim()) return;
    try {
      await categoriasService.crear({ nombre: nuevaCategoriaNombre.trim(), tipoCategoria: 'Mercaderia' });
      setNuevaCategoriaNombre('');
      setCreandoCategoria(false);
      const catRes = await categoriasService.obtenerMercaderia();
      const cats = catRes.data || [];
      setCategorias(cats);
      const nueva = cats.find((c: Categoria) => c.nombre === nuevaCategoriaNombre.trim());
      if (nueva) setForm(prev => ({ ...prev, categoriaID: String(nueva.categoriaID) }));
    } catch (err: any) {
      alert(err.response?.data?.error || 'Error al crear categoria');
    }
  };

  const eliminarCategoria = async (id: number) => {
    if (!window.confirm('Eliminar esta categoria? Los insumos asociados podrian verse afectados.')) return;
    try {
      await categoriasService.eliminar(id);
      const catRes = await categoriasService.obtenerMercaderia();
      setCategorias(catRes.data || []);
      cargar();
    } catch (err: any) {
      alert(err.response?.data?.error || 'Error al eliminar categoria');
    }
  };

  const formatMoney = (n: number) => `$${(n || 0).toLocaleString('es-AR')}`;
  const formatStock = (p: Producto) => {
    const u = p.unidadMedida || 'U';
    const abrev = u === 'Kilogramo' ? 'Kg' : u === 'Gramo' ? 'g' : u === 'Litro' ? 'L' : u === 'Mililitro' ? 'ml' : 'U';
    return `${p.stockActual}${abrev}`;
  };

  return (
    <div className="page-mercaderia">
      <h2 className="page-title"><FiPackage /> Gestion de Mercaderia</h2>

      <div className="toolbar">
        <button className="btn btn-success" onClick={() => abrirForm()}>
          <FiPlus /> Nueva Mercaderia
        </button>
        <div className="toolbar-right">
          <div className="search-bar">
            <FiSearch />
            <input placeholder="Buscar..." value={busqueda} onChange={e => setBusqueda(e.target.value)} />
          </div>
          <div className="categoria-filter-group">
            <select value={categoriaFiltro} onChange={e => setCategoriaFiltro(Number(e.target.value))}>
              <option value={0}>Todas las categorias</option>
              {categorias.map(c => (
                <option key={c.categoriaID} value={c.categoriaID}>{c.nombre}</option>
              ))}
            </select>
            {categoriaFiltro !== 0 && (
              <button className="btn-icon delete" title="Eliminar categoria" onClick={() => eliminarCategoria(categoriaFiltro)}>
                <FiX />
              </button>
            )}
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
                <th>Minimo</th>
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
                      {formatStock(p)}
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
                <tr><td colSpan={6} className="text-center text-muted">No hay mercaderia</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {showForm && (
        <div className="modal-overlay" onClick={() => setShowForm(false)}>
          <motion.div
            className="modal-content"
            onClick={e => e.stopPropagation()}
            initial={{ opacity: 0, scale: 0.9 }}
            animate={{ opacity: 1, scale: 1 }}
          >
            <h3>{editando ? 'Editar Mercaderia' : 'Nueva Mercaderia'}</h3>
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
                <div className="categoria-input-group">
                  <select value={form.categoriaID} onChange={e => setForm({ ...form, categoriaID: e.target.value })}>
                    {categorias.map(c => (
                      <option key={c.categoriaID} value={c.categoriaID}>{c.nombre}</option>
                    ))}
                  </select>
                  <button type="button" className="btn-crear-cat" onClick={() => setCreandoCategoria(!creandoCategoria)} title="Crear nueva categoria">
                    <FiPlus />
                  </button>
                </div>
                {creandoCategoria && (
                  <div className="nueva-categoria-inline">
                    <input
                      placeholder="Nombre de la categoria"
                      value={nuevaCategoriaNombre}
                      onChange={e => setNuevaCategoriaNombre(e.target.value)}
                      onKeyDown={e => e.key === 'Enter' && crearCategoria()}
                      autoFocus
                    />
                    <button className="btn-icon success" onClick={crearCategoria}><FiCheck /></button>
                    <button className="btn-icon" onClick={() => { setCreandoCategoria(false); setNuevaCategoriaNombre(''); }}><FiX /></button>
                  </div>
                )}
              </div>
              <div className="form-group">
                <label>Unidad de Medida</label>
                <select value={form.unidadMedida} onChange={e => setForm({ ...form, unidadMedida: e.target.value })}>
                  <option value="Unidad">Unidad</option>
                  <option value="Kilogramo">Kilogramo (Kg)</option>
                  <option value="Gramo">Gramo (g)</option>
                  <option value="Litro">Litro (L)</option>
                  <option value="Mililitro">Mililitro (ml)</option>
                </select>
              </div>
              <div className="form-group">
                <label>Precio</label>
                <input type="number" value={form.precio} onChange={e => setForm({ ...form, precio: e.target.value })} />
              </div>
              <div className="form-group">
                <label>Stock Actual</label>
                <input type="number" step="0.1" value={form.stockActual} onChange={e => setForm({ ...form, stockActual: e.target.value })} />
              </div>
              <div className="form-group">
                <label>Stock Minimo</label>
                <input type="number" step="0.1" value={form.stockMinimo} onChange={e => setForm({ ...form, stockMinimo: e.target.value })} />
              </div>
            </div>
            <div className="modal-actions">
              <button className="btn btn-ghost" onClick={() => setShowForm(false)}>Cancelar</button>
              <button className="btn btn-success" onClick={guardar}>
                {editando ? 'Actualizar' : 'Crear'}
              </button>
            </div>
          </motion.div>
        </div>
      )}
    </div>
  );
}
