import { ReactNode } from 'react';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import {
  FiShoppingCart, FiGrid, FiPackage, FiBookOpen,
  FiBarChart2, FiUsers, FiLogOut, FiDollarSign, FiSettings,
  FiHome, FiActivity,
} from 'react-icons/fi';

interface NavItem {
  to: string;
  icon: ReactNode;
  label: string;
  rolesPermitidos?: string[];
}

const navItems: NavItem[] = [
  { to: '/app/caja', icon: <FiDollarSign />, label: 'Caja' },
  { to: '/app/ventas', icon: <FiShoppingCart />, label: 'Punto de Venta' },
  { to: '/app/menu', icon: <FiGrid />, label: 'Menu' },
  { to: '/app/mercaderia', icon: <FiPackage />, label: 'Mercaderia' },
  { to: '/app/recetas', icon: <FiBookOpen />, label: 'Recetas' },
  { to: '/app/reportes', icon: <FiBarChart2 />, label: 'Reportes' },
  { to: '/app/usuarios', icon: <FiUsers />, label: 'Usuarios', rolesPermitidos: ['Dueño', 'Dueno'] },
  { to: '/app/mi-negocio', icon: <FiHome />, label: 'Mi Negocio', rolesPermitidos: ['Dueño', 'Dueno'] },
  { to: '/app/configuracion', icon: <FiSettings />, label: 'Configuracion' },
];

const superAdminNavItems: NavItem[] = [
  { to: '/app/admin/dashboard', icon: <FiActivity />, label: 'Dashboard' },
  { to: '/app/admin/tenants', icon: <FiUsers />, label: 'Negocios' },
];

export default function Layout() {
  const { usuario, tenant, logout, isSuperAdmin } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  const businessName = tenant?.nombreNegocio || 'GastronomiApp';
  const items = isSuperAdmin ? superAdminNavItems : navItems;

  return (
    <div className="app-layout">
      {/* Header */}
      <header className="app-header">
        <div className="header-left">
          <img src="/logo.png" alt="GastronomiApp" className="header-logo-img" />
          <div>
            <h1 className="header-title">{businessName}</h1>
            <span className="header-sub">
              {isSuperAdmin ? 'Panel de Administracion' : 'Sistema de Gestion'}
            </span>
          </div>
        </div>
        <div className="header-right">
          {tenant?.esTrial !== false && tenant?.diasRestantesTrial != null && tenant.diasRestantesTrial <= 3 && !isSuperAdmin && (
            <div className="trial-warning">
              {tenant.diasRestantesTrial} dias restantes
            </div>
          )}
          <div className="header-user">
            <span className="user-name">{usuario?.nombreCompleto || usuario?.nombreUsuario}</span>
            <span className="user-role">{usuario?.rol}</span>
          </div>
          <button className="btn-logout" onClick={handleLogout} title="Cerrar sesion">
            <FiLogOut />
          </button>
        </div>
      </header>

      <div className="app-body">
        {/* Sidebar */}
        <nav className="app-sidebar">
          <ul className="nav-list">
            {items
              .filter(item => !item.rolesPermitidos || item.rolesPermitidos.includes(usuario?.rol || ''))
              .map(item => (
                <li key={item.to}>
                  <NavLink
                    to={item.to}
                    className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}
                  >
                    {item.icon}
                    <span>{item.label}</span>
                  </NavLink>
                </li>
              ))}
          </ul>

          <div className="sidebar-footer">
            <span>{businessName}</span>
            <span>v2.0 SaaS</span>
          </div>
        </nav>

        {/* Content */}
        <main className="app-content">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
