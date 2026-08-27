import React, { createContext, useContext, useEffect, useState } from 'react';
import type { User, UserRole } from '../types';
import { api } from '../services/api';

export interface DemoPersona {
  key: string;
  name: string;
  role: UserRole;
  roleLabel: string;
  email: string;
  departmentName?: string;
  avatarColor: string;
}

export const DEMO_PERSONAS: DemoPersona[] = [
  {
    key: 'admin',
    name: 'Sonipat Municipal Admin',
    role: 'Admin',
    roleLabel: 'City Administrator',
    email: 'admin@sonipat.civicfix.gov.in',
    departmentName: 'All Departments',
    avatarColor: 'from-amber-500 to-orange-600',
  },
  {
    key: 'water_officer',
    name: 'Er. Rajesh Malik',
    role: 'DepartmentOfficer',
    roleLabel: 'Water Supply Officer',
    email: 'water.officer@sonipat.civicfix.gov.in',
    departmentName: 'Water & Sewerage',
    avatarColor: 'from-blue-500 to-cyan-600',
  },
  {
    key: 'roads_officer',
    name: 'Er. Sunil Hooda',
    role: 'DepartmentOfficer',
    roleLabel: 'Road Works Officer',
    email: 'roads.officer@sonipat.civicfix.gov.in',
    departmentName: 'Roads & PWD',
    avatarColor: 'from-amber-600 to-yellow-600',
  },
  {
    key: 'sanitation_officer',
    name: 'Dr. Manju Sharma',
    role: 'DepartmentOfficer',
    roleLabel: 'Sanitation Officer',
    email: 'sanitation.officer@sonipat.civicfix.gov.in',
    departmentName: 'Solid Waste Management',
    avatarColor: 'from-emerald-500 to-teal-600',
  },
  {
    key: 'worker_ramesh',
    name: 'Ramesh Kumar',
    role: 'FieldWorker',
    roleLabel: 'Water Field Specialist',
    email: 'ramesh.kumar@worker.civicfix.gov.in',
    departmentName: 'Water & Sewerage',
    avatarColor: 'from-purple-500 to-indigo-600',
  },
  {
    key: 'citizen_vikram',
    name: 'Vikram Singh',
    role: 'Citizen',
    roleLabel: 'Active Resident (Sector 14)',
    email: 'vikram.singh@gmail.com',
    avatarColor: 'from-rose-500 to-pink-600',
  },
];

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  switchPersona: (personaKey: string) => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);

  useEffect(() => {
    const initAuth = async () => {
      const storedUser = localStorage.getItem('civicfix_user');
      const token = localStorage.getItem('civicfix_token');

      if (token && storedUser) {
        try {
          setUser(JSON.parse(storedUser));
          // Optionally verify with server
          const me = await api.getMe();
          setUser(me);
        } catch {
          // If token expired, clear
          api.clearToken();
          setUser(null);
        }
      } else {
        // Auto-login default demo persona (Admin) for seamless preview experience
        try {
          await switchPersona('admin');
        } catch (e) {
          console.warn('Could not auto-login demo admin:', e);
        }
      }
      setIsLoading(false);
    };

    initAuth();
  }, []);

  const login = async (email: string, password: string) => {
    setIsLoading(true);
    try {
      const response = await api.login(email, password);
      setUser(response.user);
    } finally {
      setIsLoading(false);
    }
  };

  const switchPersona = async (personaKey: string) => {
    const persona = DEMO_PERSONAS.find((p) => p.key === personaKey);
    if (!persona) return;

    setIsLoading(true);
    try {
      const response = await api.login(persona.email, 'Password123!');
      setUser(response.user);
    } catch (err) {
      console.error('Failed to switch persona:', err);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const logout = () => {
    api.clearToken();
    setUser(null);
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        isLoading,
        login,
        logout,
        switchPersona,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
