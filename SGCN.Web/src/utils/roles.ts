import type { AuthUser } from '../types/auth';

export const Roles = {
  Administrateur: 'Administrateur',
  AgentEtatCivil: 'AgentEtatCivil',
  Medecin: 'Medecin',
  Citoyen: 'Citoyen'
} as const;

export function hasRole(user: AuthUser | null, role: string): boolean {
  return user?.roles?.includes(role) ?? false;
}

export function hasAnyRole(user: AuthUser | null, roles: string[]): boolean {
  return roles.some((role) => hasRole(user, role));
}

export function primaryRole(user: AuthUser | null): string {
  return user?.roles?.[0] ?? 'Utilisateur';
}
