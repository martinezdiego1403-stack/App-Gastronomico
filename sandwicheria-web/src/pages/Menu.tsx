import { useState, useEffect } from 'react';
import { productosService, recetasService, categoriasService } from '../services/api';
import { motion } from 'framer-motion';
import { FiGrid, FiPlus, FiEdit2, FiTrash2, FiSearch, FiRefreshCw, FiBookOpen, FiX, FiCheck } from 'react-icons/fi';

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

interface Receta {
  recetaID: number;
  nombre: string;
  descripcion: string;
  precio: number;
  stockActual: number;
  categoriaID: number;
  categoriaNombre: string;
}

interface Categoria {
  categoriaID: number;
  nombre: string;
}

// Tipo unificado para mostrar productos y recetas juntos
interface ItemMenu {
  id: number;
  nombre: string;
  descripcion: string;
  precio: number;
  stockActual: number;
  stockMinimo: number;
  categoriaNombre: string;
  categoriaID: number;
  tipo: 'producto' | 'receta';
  codigoBarras?: string;
}

export default function Menu() {
  const [productos, setProductos] = useState<Producto[]>([]);
  const [recetas, setRecetas] = useState<Receta[]>([]);
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [busqueda, setBusqueda] = useState('');
  const [categoriaFiltro, setCategoriaFiltro] = useState(0);
  const [loading, setLoading] = useState(true);

  // Form state
  const [showForm, setShowForm] = useState(false);
  const [editando, setEditando] = useState<Producto | null>(null);
  const [form, setForm] = useState({ nombre: '', descripcion: '', precio: '', stockActual: '', stockMinimo: '', categoriaID: '', codigoBarras: '' });
  const [creandoCategoria, setCreandoCategoria] = useState(false);
  const [nuevaCategoriaNombre, setNuevaCategoriaNombre] = useState('');

  useEffect(() => { cargar(); }, []);

  const cargar = async () => {
    setLoading(true);
    try {
      const [prodRes, recRes, catRes] = await Promise.all([
        productosService.obtenerMenu(),
        recetasService.obtenerTodas(),
        categoriasService.obtenerMenu(),
      ]);
      setProductos(prodRes.data || []);
      setRecetas(recRes.data || []);
      setCategorias(catRes.data || []);
    } catch {} finally { setLoading(false); }
  };

  // Unificar productos y recetas en una sola lista
  const itemsMenu: ItemMenu[] = [
    ...productos.map(p => ({
      id: p.productoID, nombre: p.nombre, descripcion: p.descripcion,
      precio: p.precio, stockActual: p.stockActual, stockMinimo: p.stockMinimo,
      categoriaNombre: p.categoriaNombre, categoriaID: p.categoriaID,
      tipo: 'producto' as const, codigoBarras: p.codigoBarras,
    })),
    ...recetas.map(r => ({
      id: r.recetaID, nombre: r.nombre, descripcion: r.descripcion || '',
      precio: r.precio, stockActual: r.stockActual, stockMinimo: 0,
      categoriaNombre: r.categoriaNombre, categoriaID: r.categoriaID,
      tipo: 'receta' as const,
    })),
  ];

  const productosFiltrados = itemsMenu.filter(p => {
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

  const crearCategoria = async () => {
    if (!nuevaCategoriaNombre.trim()) return;
    try {
      await categoriasService.crear({ nombre: nuevaCategoriaNombre.trim(), tipoCategoria: 'Menu' });
      setNuevaCategoriaNombre('');
      setCreandoCategoria(false);
      const catRes = await categoriasService.obtenerMenu();
      const cats = catRes.data || [];
      setCategorias(cats);
      const nueva = cats.find((c: Categoria) => c.nombre === nuevaCategoriaNombre.trim());
      if (nueva) setForm(prev => ({ ...prev, categoriaID: String(nueva.categoriaID) }));
    } catch (err: any) {
      alert(err.response?.data?.error || 'Error al crear categoria');
    }
  };

  const eliminarCategoria = async (id: number) => {
    if (!window.confirm('Eliminar esta categoria? Los productos asociados podrian verse afectados.')) return;
    try {
      await categoriasService.eliminar(id);
      const catRes = await categoriasService.obtenerMenu();
      setCategorias(catRes.data || []);
      cargar();
    } catch (err: any) {
      alert(err.response?.data?.error || 'Error al eliminar categoria');
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

      {/* Tabla */}
      <div className="section-card">
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Tipo</th>
                <th>Categoria</th>
                <th>Precio</th>
                <th>Stock</th>
                <th>Min</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr><td colSpan={7} className="text-center">Cargando...</td></tr>
              ) : productosFiltrados.map(p => (
                <motion.tr key={`${p.tipo}-${p.id}`} initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
                  <td>
                    <div className="cell-name">
                      <strong>{p.nombre}</strong>
                      {p.descripcion && <small>{p.descripcion}</small>}
                    </div>
                  </td>
                  <td>
                    <span className={`type-badge ${p.tipo}`}>
                      {p.tipo === 'receta' ? <><FiBookOpen /> Receta</> : 'Producto'}
                    </span>
                  </td>
                  <td>{p.categoriaNombre}</td>
                  <td className="text-success font-bold">{formatMoney(p.precio)}</td>
                  <td>
                    <span className={`stock-badge ${p.stockMinimo > 0 && p.stockActual <= p.stockMinimo ? 'low' : 'ok'}`}>
                      {p.stockActual}
                    </span>
                  </td>
                  <td>{p.tipo === 'producto' ? <span className="stock-badge min">{p.stockMinimo}</span> : '-'}</td>
                  <td>
                    {p.tipo === 'producto' ? (
                      <div className="actions">
                        <button className="btn-icon edit" onClick={() => abrirForm(productos.find(pr => pr.productoID === p.id))}><FiEdit2 /></button>
                        <button className="btn-icon delete" onClick={() => eliminar(p.id)}><FiTrash2 /></button>
                      </div>
                    ) : (
                      <span className="text-muted" style={{ fontSize: '0.75rem' }}>Editar en Recetas</span>
                    )}
                  </td>
                </motion.tr>
              ))}
              {!loading && productosFiltrados.length === 0 && (
                <tr><td colSpan={7} className="text-center text-muted">No hay productos ni recetas</td></tr>
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
