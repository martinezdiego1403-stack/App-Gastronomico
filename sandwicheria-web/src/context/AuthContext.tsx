import { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { authService } from '../services/api';

interface Usuario {
  usuarioID: number;
  nombreUsuario: string;
  nombreCompleto: string;
  rol: string;
  email: string;
}

interface AuthContextType {
  usuario: Usuario | null;
  token: string | null;
  login: (nombreUsuario: string, contrasena: string) => Promise<void>;
  logout: () => void;
  isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [usuario, setUsuario] = useState<Usuario | null>(null);
  const [token, setToken] = useState<string | null>(null);

  useEffect(() => {
    const savedToken = localStorage.getItem('token');
    const savedUsuario = localStorage.getItem('usuario');
    if (savedToken && savedUsuario) {
      setToken(savedToken);
      setUsuario(JSON.parse(savedUsuario));
    }
  }, []);

  const login = async (nombreUsuario: string, contrasena: string) => {
    const res = await authService.login(nombreUsuario, contrasena);
    const { token: jwt, usuario: usr } = res.data;
    localStorage.setItem('token', jwt);
    localStorage.setItem('usuario', JSON.stringify(usr));
    setToken(jwt);
    setUsuario(usr);
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('usuario');
    setToken(null);
    setUsuario(null);
  };

  return (
    <AuthContext.Provider value={{ usuario, token, login, logout, isAuthenticated: !!token }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth debe usarse dentro de AuthProvider');
  return ctx;
}
