import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { ThemeProvider } from './context/ThemeContext';
import Layout from './components/Layout';
import Landing from './pages/Landing';
import Login from './pages/Login';
import Registro from './pages/Registro';
import TrialExpirado from './pages/TrialExpirado';
import Caja from './pages/Caja';
import Ventas from './pages/Ventas';
import Menu from './pages/Menu';
import Mercaderia from './pages/Mercaderia';
import Recetas from './pages/Recetas';
import Reportes from './pages/Reportes';
import Usuarios from './pages/Usuarios';
import Configuracion from './pages/Configuracion';
import MiNegocio from './pages/MiNegocio';
import SuperAdminDashboard from './pages/SuperAdminDashboard';
import './App.css';

function PrivateRoute({ children }: { children: React.ReactElement }) {
  const { isAuthenticated } = useAuth();
  return isAuthenticated ? children : <Navigate to="/login" />;
}

function SuperAdminRoute({ children }: { children: React.ReactElement }) {
  const { isAuthenticated, isSuperAdmin } = useAuth();
  if (!isAuthenticated) return <Navigate to="/login" />;
  if (!isSuperAdmin) return <Navigate to="/" />;
  return children;
}

function AppRoutes() {
  const { isAuthenticated, isSuperAdmin } = useAuth();

  return (
    <Routes>
      {/* Ruta principal: landing si no esta logueado, dashboard si lo esta */}
      <Route path="/" element={isAuthenticated ? <Navigate to="/app" /> : <Landing />} />
      <Route path="/landing" element={isAuthenticated ? <Navigate to="/app" /> : <Landing />} />
      <Route path="/login" element={isAuthenticated ? <Navigate to="/app" /> : <Login />} />
      <Route path="/registro" element={isAuthenticated ? <Navigate to="/app" /> : <Registro />} />
      <Route path="/trial-expirado" element={<TrialExpirado />} />

      {/* Rutas protegidas - App POS */}
      <Route path="/app" element={<PrivateRoute><Layout /></PrivateRoute>}>
        <Route index element={<Navigate to={isSuperAdmin ? '/app/admin/dashboard' : '/app/caja'} />} />
        <Route path="caja" element={<Caja />} />
        <Route path="ventas" element={<Ventas />} />
        <Route path="menu" element={<Menu />} />
        <Route path="mercaderia" element={<Mercaderia />} />
        <Route path="recetas" element={<Recetas />} />
        <Route path="reportes" element={<Reportes />} />
        <Route path="usuarios" element={<Usuarios />} />
        <Route path="configuracion" element={<Configuracion />} />
        <Route path="mi-negocio" element={<MiNegocio />} />

        {/* Rutas SuperAdmin */}
        <Route path="admin/dashboard" element={
          <SuperAdminRoute><SuperAdminDashboard /></SuperAdminRoute>
        } />
        <Route path="admin/tenants" element={
          <SuperAdminRoute><SuperAdminDashboard /></SuperAdminRoute>
        } />
      </Route>

      {/* Fallback */}
      <Route path="*" element={<Navigate to={isAuthenticated ? '/app' : '/'} />} />
    </Routes>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <ThemeProvider>
        <AuthProvider>
          <AppRoutes />
        </AuthProvider>
      </ThemeProvider>
    </BrowserRouter>
  );
}
