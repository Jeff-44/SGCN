import {
  BadgeCheck,
  Building2,
  ClipboardList,
  FileCheck,
  FileText,
  HeartPulse,
  Home,
  Hospital,
  Landmark,
  PlusCircle,
  Users
} from 'lucide-react';
import { NavLink } from 'react-router-dom';
import type { AuthUser } from '../../types/auth';
import { hasAnyRole, Roles } from '../../utils/roles';

type SidebarProps = {
  isOpen: boolean;
  onClose: () => void;
  user: AuthUser | null;
};

const items = [
  { label: 'Dashboard', path: '/', icon: Home, roles: [Roles.Administrateur] },
  { label: 'Users', path: '/users', icon: Users, roles: [Roles.Administrateur] },
  { label: 'Departments', path: '/referentials/departments', icon: Landmark, roles: [Roles.Administrateur] },
  { label: 'Communes', path: '/referentials/communes', icon: Building2, roles: [Roles.Administrateur] },
  { label: 'Hospitals', path: '/referentials/hospitals', icon: Hospital, roles: [Roles.Administrateur] },
  { label: 'Birth Records', path: '/birth-records', icon: HeartPulse, roles: [Roles.Administrateur, Roles.Medecin] },
  { label: 'Create Birth Record', path: '/birth-records/create', icon: PlusCircle, roles: [Roles.Medecin] },
  {
    label: 'Pending Certificate',
    path: '/birth-records/pending-certificate',
    icon: ClipboardList,
    roles: [Roles.AgentEtatCivil]
  },
  {
    label: 'Certificate Requests',
    path: '/certificate-requests',
    icon: FileText,
    roles: [Roles.Administrateur, Roles.AgentEtatCivil]
  },
  { label: 'My Requests', path: '/certificate-requests', icon: FileText, roles: [Roles.Citoyen] },
  {
    label: 'Certificates',
    path: '/certificates',
    icon: BadgeCheck,
    roles: [Roles.Administrateur, Roles.AgentEtatCivil]
  },
  { label: 'My Certificates', path: '/certificates', icon: FileCheck, roles: [Roles.Citoyen] }
];

export default function Sidebar({ isOpen, onClose, user }: SidebarProps) {
  const visibleItems = items.filter((item) => hasAnyRole(user, item.roles));
  const navigation = visibleItems.length > 0 ? visibleItems : items.slice(0, 1);
  // TODO: If backend role claims are unavailable, show basic navigation after login.

  return (
    <>
      <div
        className={`fixed inset-0 z-30 bg-slate-950/30 transition md:hidden ${isOpen ? 'block' : 'hidden'}`}
        onClick={onClose}
      />
      <aside
        className={`fixed inset-y-0 left-0 z-40 w-72 border-r border-slate-200 bg-white transition-transform md:static md:z-auto md:translate-x-0 ${
          isOpen ? 'translate-x-0' : '-translate-x-full'
        }`}
      >
        <div className="flex h-16 items-center border-b border-slate-200 px-5">
          <div>
            <p className="text-lg font-bold text-slate-950">SGCN</p>
            <p className="text-xs text-slate-500">Certificats de naissance</p>
          </div>
        </div>
        <nav className="grid gap-1 px-3 py-4">
          {navigation.map((item) => {
            const Icon = item.icon;
            return (
              <NavLink
                className={({ isActive }) =>
                  `flex min-h-10 items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition ${
                    isActive ? 'bg-slate-900 text-white' : 'text-slate-700 hover:bg-slate-100'
                  }`
                }
                key={`${item.label}-${item.path}`}
                onClick={onClose}
                to={item.path}
              >
                <Icon size={18} />
                <span>{item.label}</span>
              </NavLink>
            );
          })}
        </nav>
      </aside>
    </>
  );
}
