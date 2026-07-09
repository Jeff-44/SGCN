import type { AuthResponse, AuthUser } from '../types/auth';

const TOKEN_KEY = 'sgcn.accessToken';
const USER_KEY = 'sgcn.user';

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

export function getStoredUser(): AuthUser | null {
  const value = localStorage.getItem(USER_KEY);
  if (!value) {
    return null;
  }

  try {
    return JSON.parse(value) as AuthUser;
  } catch {
    return null;
  }
}

export function storeAuth(response: AuthResponse): AuthUser {
  const user: AuthUser = {
    userId: response.userId,
    email: response.email,
    fullName: response.fullName,
    forcePasswordChange: response.forcePasswordChange,
    roles: response.roles ?? []
  };

  localStorage.setItem(TOKEN_KEY, response.accessToken);
  localStorage.setItem(USER_KEY, JSON.stringify(user));
  return user;
}

export function clearAuth(): void {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
}
