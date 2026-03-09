import { ReactNode } from 'react';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import {
  FiShoppingCart, FiGrid, FiPackage, FiBookOpen,
  FiBarChart2, FiUsers, FiLogOut, FiDollarSign, FiSettings,
} from 'react-icons/fi';

interface NavItem {
  to: string;
  icon: ReactNode;
  label: string;
  rolesPermitidos?: string[];
}

const navItems: NavItem[] = [
  { to: '/caja', icon: <FiDollarSign />, label: 'Caja' },
  { to: '/ventas', icon: <FiShoppingCart />, label: 'Punto de Venta' },
  { to: '/menu', icon: <FiGrid />, label: 'Menu' },
  { to: '/mercaderia', icon: <FiPackage />, label: 'Mercaderia' },
  { to: '/recetas', icon: <FiBookOpen />, label: 'Recetas' },
  { to: '/reportes', icon: <FiBarChart2 />, label: 'Reportes' },
  { to: '/usuarios', icon: <FiUsers />, label: 'Usuarios', rolesPermitidos: ['Dueno'] },
  { to: '/configuracion', icon: <FiSettings />, label: 'Configuracion' },
];

export default function Layout() {
  const { usuario, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="app-layout">
      {/* Header */}
      <header className="app-header">
        <div className="header-left">
          <span className="header-logo" role="img" aria-label="logo">&#x1F96A;</span>
          <div>
            <h1 className="header-title">La Sandwicheria</h1>
            <span className="header-sub">Sistema de Gestion</span>
          </div>
        </div>
        <div className="header-right">
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
            {navItems
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
            <span>La Sandwicheria</span>
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
