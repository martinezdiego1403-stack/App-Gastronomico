import { useState, useEffect } from 'react';
import { usuariosService } from '../services/api';
import { motion } from 'framer-motion';
import { FiUsers, FiPlus, FiEdit2, FiLock, FiUnlock, FiRefreshCw, FiEye, FiEyeOff } from 'react-icons/fi';

interface Usuario {
  usuarioID: number;
  nombreUsuario: string;
  nombreCompleto: string;
  email: string;
  rol: string;
  activo: boolean;
  estaBloqueado: boolean;
}

export default function Usuarios() {
  const [usuarios, setUsuarios] = useState<Usuario[]>([]);
  const [loading, setLoading] = useState(true);

  const [showForm, setShowForm] = useState(false);
  const [editando, setEditando] = useState<Usuario | null>(null);
  const [form, setForm] = useState({ nombreUsuario: '', nombreCompleto: '', email: '', rol: 'Empleado', contrasena: '' });
  const [showPass, setShowPass] = useState(false);

  useEffect(() => { cargar(); }, []);

  const cargar = async () => {
    setLoading(true);
    try {
      const res = await usuariosService.obtenerTodos();
      setUsuarios(res.data || []);
    } catch {} finally { setLoading(false); }
  };

  const abrirForm = (usr?: Usuario) => {
    if (usr) {
      setEditando(usr);
      setForm({
        nombreUsuario: usr.nombreUsuario,
        nombreCompleto: usr.nombreCompleto,
        email: usr.email || '',
        rol: usr.rol,
        contrasena: '',
      });
    } else {
      setEditando(null);
      setForm({ nombreUsuario: '', nombreCompleto: '', email: '', rol: 'Empleado', contrasena: '' });
    }
    setShowForm(true);
  };

  const guardar = async () => {
    try {
      if (editando) {
        const data: any = {
          UsuarioID: editando.usuarioID,
          NombreUsuario: form.nombreUsuario,
          NombreCompleto: form.nombreCompleto,
          Email: form.email,
          Rol: form.rol,
        };
        if (form.contrasena) data.Contrasena = form.contrasena;
        await usuariosService.actualizar(editando.usuarioID, data);
      } else {
        await usuariosService.crear({
          NombreUsuario: form.nombreUsuario,
          NombreCompleto: form.nombreCompleto,
          Email: form.email,
          Rol: form.rol,
          Contrasena: form.contrasena,
        });
      }
      setShowForm(false);
      cargar();
    } catch (err: any) {
      alert(err.response?.data?.error || 'Error al guardar');
    }
  };

  const cambiarEstado = async (id: number, activo: boolean) => {
    try {
      await usuariosService.cambiarEstado(id, !activo);
      cargar();
    } catch (err: any) {
      alert(err.response?.data?.error || 'Error');
    }
  };

  const desbloquear = async (id: number) => {
    try {
      await usuariosService.desbloquear(id);
      cargar();
    } catch (err: any) {
      alert(err.response?.data?.error || 'Error');
    }
  };

  return (
    <div className="page-usuarios">
      <h2 className="page-title"><FiUsers /> Gestion de Usuarios</h2>

      <div className="toolbar">
        <button className="btn btn-primary" onClick={() => abrirForm()}>
          <FiPlus /> Nuevo Usuario
        </button>
        <div className="toolbar-right">
          <button className="btn btn-ghost" onClick={cargar}><FiRefreshCw /></button>
        </div>
      </div>

      <div className="section-card">
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Usuario</th>
                <th>Nombre Completo</th>
                <th>Email</th>
                <th>Rol</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr><td colSpan={6} className="text-center">Cargando...</td></tr>
              ) : usuarios.map(u => (
                <motion.tr key={u.usuarioID} initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
                  <td><strong>{u.nombreUsuario}</strong></td>
                  <td>{u.nombreCompleto}</td>
                  <td>{u.email || '-'}</td>
                  <td>
                    <span className={`role-badge ${u.rol.toLowerCase()}`}>{u.rol}</span>
                  </td>
                  <td>
                    {u.estaBloqueado ? (
                      <span className="status-badge blocked">Bloqueado</span>
                    ) : u.activo ? (
                      <span className="status-badge active">Activo</span>
                    ) : (
                      <span className="status-badge inactive">Inactivo</span>
                    )}
                  </td>
                  <td>
                    <div className="actions">
                      <button className="btn-icon edit" onClick={() => abrirForm(u)} title="Editar"><FiEdit2 /></button>
                      <button
                        className={`btn-icon ${u.activo ? 'delete' : 'success'}`}
                        onClick={() => cambiarEstado(u.usuarioID, u.activo)}
                        title={u.activo ? 'Desactivar' : 'Activar'}
                      >
                        {u.activo ? <FiLock /> : <FiUnlock />}
                      </button>
                      {u.estaBloqueado && (
                        <button className="btn-icon info" onClick={() => desbloquear(u.usuarioID)} title="Desbloquear">
                          <FiUnlock />
                        </button>
                      )}
                    </div>
                  </td>
                </motion.tr>
              ))}
              {!loading && usuarios.length === 0 && (
                <tr><td colSpan={6} className="text-center text-muted">No hay usuarios</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {showForm && (
        <div className="modal-overlay" onClick={() => setShowForm(false)}>
          <motion.div className="modal-content" onClick={e => e.stopPropagation()}
            initial={{ opacity: 0, scale: 0.9 }} animate={{ opacity: 1, scale: 1 }}>
            <h3>{editando ? 'Editar Usuario' : 'Nuevo Usuario'}</h3>
            <div className="form-grid">
              <div className="form-group">
                <label>Nombre de Usuario</label>
                <input value={form.nombreUsuario} onChange={e => setForm({ ...form, nombreUsuario: e.target.value })} />
              </div>
              <div className="form-group">
                <label>Nombre Completo</label>
                <input value={form.nombreCompleto} onChange={e => setForm({ ...form, nombreCompleto: e.target.value })} />
              </div>
              <div className="form-group">
                <label>Email</label>
                <input type="email" value={form.email} onChange={e => setForm({ ...form, email: e.target.value })} />
              </div>
              <div className="form-group">
                <label>Rol</label>
                <select value={form.rol} onChange={e => setForm({ ...form, rol: e.target.value })}>
                  <option value="Empleado">Empleado</option>
                  <option value="Administrador">Administrador</option>
                  <option value="Dueno">Dueno</option>
                </select>
              </div>
              <div className="form-group">
                <label>{editando ? 'Nueva Contrasena (dejar vacio para no cambiar)' : 'Contrasena'}</label>
                <div className="input-group">
                  <input type={showPass ? 'text' : 'password'} value={form.contrasena} onChange={e => setForm({ ...form, contrasena: e.target.value })} />
                  <button type="button" className="input-toggle-pass" onClick={() => setShowPass(!showPass)} tabIndex={-1}>
                    {showPass ? <FiEyeOff /> : <FiEye />}
                  </button>
                </div>
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
