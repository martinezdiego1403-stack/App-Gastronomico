import axios from 'axios';

const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_URL,
  headers: { 'Content-Type': 'application/json' },
});

// Interceptor: agrega el token JWT a cada request automáticamente
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Interceptor: maneja errores de autenticación y trial expirado
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('usuario');
      localStorage.removeItem('tenant');
      window.location.href = '/login';
    }
    if (error.response?.status === 402) {
      // Trial expirado - redirigir a página de upgrade
      window.location.href = '/trial-expirado';
    }
    return Promise.reject(error);
  }
);

// ============================================
// AUTH
// ============================================
export const authService = {
  login: (nombreUsuario: string, contraseña: string) =>
    api.post('/auth/login', { NombreUsuario: nombreUsuario, 'Contraseña': contraseña }),

  register: (data: { nombreUsuario: string; nombreCompleto: string; email: string; contraseña: string }) =>
    api.post('/auth/register', { NombreUsuario: data.nombreUsuario, NombreCompleto: data.nombreCompleto, Email: data.email, 'Contraseña': data.contraseña }),

  registroNegocio: (data: {
    nombreUsuario: string; nombreCompleto: string; email: string;
    contraseña: string; nombreNegocio: string; telefono?: string;
  }) =>
    api.post('/auth/registro-negocio', {
      NombreUsuario: data.nombreUsuario,
      NombreCompleto: data.nombreCompleto,
      Email: data.email,
      'Contraseña': data.contraseña,
      NombreNegocio: data.nombreNegocio,
      Telefono: data.telefono,
    }),
};

// ============================================
// CATEGORIAS
// ============================================
export const categoriasService = {
  obtenerTodas: () => api.get('/categorias'),
  obtenerMenu: () => api.get('/categorias/menu'),
  obtenerMercaderia: () => api.get('/categorias/mercaderia'),
  crear: (data: { nombre: string; tipoCategoria: string }) => api.post('/categorias', data),
  actualizar: (id: number, data: any) => api.put(`/categorias/${id}`, data),
  eliminar: (id: number) => api.delete(`/categorias/${id}`),
};

// ============================================
// PRODUCTOS
// ============================================
export const productosService = {
  obtenerMenu: () => api.get('/productos/menu'),
  obtenerMercaderia: () => api.get('/productos/mercaderia'),
  obtenerPorId: (id: number) => api.get(`/productos/${id}`),
  buscar: (termino: string) => api.get(`/productos/buscar?termino=${termino}`),
  obtenerPorCodigo: (codigo: string) => api.get(`/productos/codigo/${codigo}`),
  obtenerStockBajo: () => api.get('/productos/stock-bajo'),
  crear: (data: any) => api.post('/productos', data),
  actualizar: (id: number, data: any) => api.put(`/productos/${id}`, data),
  eliminar: (id: number) => api.delete(`/productos/${id}`),
};

// ============================================
// RECETAS
// ============================================
export const recetasService = {
  obtenerTodas: () => api.get('/recetas'),
  obtenerPorId: (id: number) => api.get(`/recetas/${id}`),
  obtenerIngredientes: (id: number) => api.get(`/recetas/${id}/ingredientes`),
  crear: (data: any) => api.post('/recetas', data),
  actualizar: (id: number, data: any) => api.put(`/recetas/${id}`, data),
  eliminar: (id: number) => api.delete(`/recetas/${id}`),
};

// ============================================
// VENTAS
// ============================================
export const ventasService = {
  registrar: (data: any) => api.post('/ventas', data),
  obtenerDelDia: () => api.get('/ventas/del-dia'),
  obtenerPorCaja: (cajaId: number) => api.get(`/ventas/por-caja/${cajaId}`),
  obtenerPorRango: (fechaInicio: string, fechaFin: string) =>
    api.get(`/ventas/por-rango?fechaInicio=${fechaInicio}&fechaFin=${fechaFin}`),
  obtenerTicket: (ventaId: number) => api.get(`/ventas/${ventaId}/ticket`),
};

// ============================================
// CAJAS
// ============================================
export const cajasService = {
  obtenerAbierta: () => api.get('/cajas/abierta'),
  abrir: (montoInicial: number) => api.post('/cajas/abrir', { montoInicial }),
  cerrar: (cajaID: number, montoCierre: number, observaciones?: string) =>
    api.post('/cajas/cerrar', { cajaID, montoCierre, observaciones }),
  historial: () => api.get('/cajas/historial'),
  resumenPagos: (id: number) => api.get(`/cajas/${id}/resumen-pagos`),
};

// ============================================
// REPORTES
// ============================================
export const reportesService = {
  ventasPorDia: (fechaInicio: string, fechaFin: string) =>
    api.get(`/reportes/ventas-por-dia?fechaInicio=${fechaInicio}&fechaFin=${fechaFin}`),
  productosMasVendidos: (fechaInicio: string, fechaFin: string) =>
    api.get(`/reportes/productos-mas-vendidos?fechaInicio=${fechaInicio}&fechaFin=${fechaFin}`),
  ventasPorMetodoPago: (fechaInicio: string, fechaFin: string) =>
    api.get(`/reportes/ventas-por-metodo-pago?fechaInicio=${fechaInicio}&fechaFin=${fechaFin}`),
  resumen: (fechaInicio: string, fechaFin: string) =>
    api.get(`/reportes/resumen?fechaInicio=${fechaInicio}&fechaFin=${fechaFin}`),
  ventasPorCategoria: (fechaInicio: string, fechaFin: string) =>
    api.get(`/reportes/ventas-por-categoria?fechaInicio=${fechaInicio}&fechaFin=${fechaFin}`),
};

// ============================================
// USUARIOS
// ============================================
export const usuariosService = {
  obtenerTodos: () => api.get('/usuarios'),
  obtenerPorId: (id: number) => api.get(`/usuarios/${id}`),
  crear: (data: any) => api.post('/usuarios', data),
  actualizar: (id: number, data: any) => api.put(`/usuarios/${id}`, data),
  cambiarEstado: (id: number, activo: boolean) => api.put(`/usuarios/${id}/cambiar-estado?activo=${activo}`),
  desbloquear: (id: number) => api.put(`/usuarios/${id}/desbloquear`),
};

// ============================================
// CONFIGURACION
// ============================================
export const configuracionService = {
  obtenerWhatsApp: () => api.get('/configuracion/whatsapp'),
  guardarWhatsApp: (data: { whatsAppNumero: string; whatsAppHabilitado: boolean; nombreNegocio: string }) =>
    api.post('/configuracion/whatsapp', data),
  testWhatsApp: () => api.get('/configuracion/whatsapp/test'),
  alertaStockBajo: () => api.get('/configuracion/whatsapp/stock-bajo'),
  resumenCierreCaja: (cajaId: number) => api.get(`/configuracion/whatsapp/cierre-caja/${cajaId}`),
};

// ============================================
// TENANT SETTINGS (Mi Negocio)
// ============================================
export const tenantSettingsService = {
  obtenerMiNegocio: () => api.get('/tenantsettings/mi-negocio'),
  actualizarMiNegocio: (data: { nombreNegocio: string; emailContacto?: string; telefono?: string }) =>
    api.put('/tenantsettings/mi-negocio', data),
  obtenerPlan: () => api.get('/tenantsettings/plan'),
  obtenerFiscal: () => api.get('/tenantsettings/fiscal'),
  actualizarFiscal: (data: { condicionFiscal: string; cuit?: string; direccionFiscal?: string; puntoVenta?: number }) =>
    api.put('/tenantsettings/fiscal', data),
};

// ============================================
// SUPER ADMIN
// ============================================
export const superAdminService = {
  dashboard: () => api.get('/superadmin/dashboard'),
  obtenerTenants: () => api.get('/superadmin/tenants'),
  obtenerTenant: (tenantId: string) => api.get(`/superadmin/tenants/${tenantId}`),
  cambiarEstadoTenant: (tenantId: string, activo: boolean) =>
    api.put(`/superadmin/tenants/${tenantId}/activar?activo=${activo}`),
  cambiarPlan: (tenantId: string, plan: string) =>
    api.put(`/superadmin/tenants/${tenantId}/plan`, { plan }),
};

export default api;
