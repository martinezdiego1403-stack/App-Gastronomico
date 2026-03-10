import { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { authService } from '../services/api';

interface Usuario {
  usuarioID: number;
  nombreUsuario: string;
  nombreCompleto: string;
  rol: string;
  email: string;
}

interface TenantInfo {
  tenantId: string;
  nombreNegocio: string;
  plan: string;
  activo: boolean;
  diasRestantesTrial: number;
  trialExpirado: boolean;
}

interface AuthContextType {
  usuario: Usuario | null;
  tenant: TenantInfo | null;
  token: string | null;
  login: (nombreUsuario: string, contrasena: string) => Promise<void>;
  loginWithToken: (token: string, usuario: Usuario, tenant?: TenantInfo | null) => void;
  updateTenant: (tenant: TenantInfo) => void;
  logout: () => void;
  isAuthenticated: boolean;
  isSuperAdmin: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [usuario, setUsuario] = useState<Usuario | null>(null);
  const [tenant, setTenant] = useState<TenantInfo | null>(null);
  const [token, setToken] = useState<string | null>(null);

  useEffect(() => {
    const savedToken = localStorage.getItem('token');
    const savedUsuario = localStorage.getItem('usuario');
    const savedTenant = localStorage.getItem('tenant');
    if (savedToken && savedUsuario) {
      setToken(savedToken);
      setUsuario(JSON.parse(savedUsuario));
      if (savedTenant) setTenant(JSON.parse(savedTenant));
    }
  }, []);

  const login = async (nombreUsuario: string, contrasena: string) => {
    const res = await authService.login(nombreUsuario, contrasena);
    const data = res.data;

    if (!data.exitoso) {
      throw new Error(data.mensaje || 'Error al iniciar sesion');
    }

    localStorage.setItem('token', data.token);
    localStorage.setItem('usuario', JSON.stringify(data.usuario));
    if (data.tenant) {
      localStorage.setItem('tenant', JSON.stringify(data.tenant));
      setTenant(data.tenant);
    }
    setToken(data.token);
    setUsuario(data.usuario);
  };

  const loginWithToken = (newToken: string, newUsuario: Usuario, newTenant?: TenantInfo | null) => {
    localStorage.setItem('token', newToken);
    localStorage.setItem('usuario', JSON.stringify(newUsuario));
    if (newTenant) {
      localStorage.setItem('tenant', JSON.stringify(newTenant));
      setTenant(newTenant);
    }
    setToken(newToken);
    setUsuario(newUsuario);
  };

  const updateTenant = (updatedTenant: TenantInfo) => {
    localStorage.setItem('tenant', JSON.stringify(updatedTenant));
    setTenant(updatedTenant);
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('usuario');
    localStorage.removeItem('tenant');
    setToken(null);
    setUsuario(null);
    setTenant(null);
  };

  return (
    <AuthContext.Provider value={{
      usuario, tenant, token,
      login, loginWithToken, updateTenant, logout,
      isAuthenticated: !!token,
      isSuperAdmin: usuario?.rol === 'SuperAdmin',
    }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth debe usarse dentro de AuthProvider');
  return ctx;
}
