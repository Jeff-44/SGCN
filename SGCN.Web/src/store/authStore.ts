import { useSyncExternalStore } from 'react';
import type { AuthResponse, AuthUser } from '../types/auth';
import { clearAuth, getStoredUser, getToken, storeAuth } from '../utils/token';

type AuthSnapshot = {
  token: string | null;
  user: AuthUser | null;
  isAuthenticated: boolean;
};

const listeners = new Set<() => void>();

let currentSnapshot: AuthSnapshot = createSnapshot();

function createSnapshot(): AuthSnapshot {
  const token = getToken();
  const user = getStoredUser();

  return {
    token,
    user,
    isAuthenticated: Boolean(token && user)
  };
}

function emitChange(): void {
  currentSnapshot = createSnapshot();
  listeners.forEach((listener) => listener());
}

export const authStore = {
  getSnapshot() {
    return currentSnapshot;
  },

  subscribe(listener: () => void) {
    listeners.add(listener);
    return () => {
      listeners.delete(listener);
    };
  },

  setSession(response: AuthResponse) {
    storeAuth(response);
    emitChange();
  },

  logout() {
    clearAuth();
    emitChange();
  }
};

export function useAuth(): AuthSnapshot {
  return useSyncExternalStore(
    authStore.subscribe,
    authStore.getSnapshot,
    authStore.getSnapshot
  );
}