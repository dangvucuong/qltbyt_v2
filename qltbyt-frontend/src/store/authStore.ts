import { create } from 'zustand';
import type { UserInfo } from '@/types/api';

const TOKEN_KEY = 'qltbyt_token';
const USER_KEY = 'qltbyt_user';

interface AuthState {
  token: string | null;
  user: UserInfo | null;
  setAuth: (token: string, user: UserInfo) => void;
  logout: () => void;
  hydrate: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  token: null,
  user: null,
  setAuth: (token, user) => {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    set({ token, user });
  },
  logout: () => {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    set({ token: null, user: null });
  },
  hydrate: () => {
    const token = localStorage.getItem(TOKEN_KEY);
    const raw = localStorage.getItem(USER_KEY);
    if (token && raw) {
      try {
        set({ token, user: JSON.parse(raw) as UserInfo });
      } catch {
        localStorage.removeItem(USER_KEY);
      }
    }
  },
}));
