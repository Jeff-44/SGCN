import { LogOut, Menu } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { authStore, useAuth } from '../../store/authStore';
import { primaryRole } from '../../utils/roles';
import Button from '../ui/Button';

type HeaderProps = {
  onMenuClick: () => void;
};

export default function Header({ onMenuClick }: HeaderProps) {
  const { user } = useAuth();
  const navigate = useNavigate();

  function logout() {
    authStore.logout();
    navigate('/login', { replace: true });
  }

  return (
    <header className="flex min-h-16 items-center justify-between border-b border-slate-200 bg-white px-4 md:px-6">
      <div className="flex items-center gap-3">
        <Button aria-label="Menu" className="h-10 w-10 px-0 md:hidden" icon={<Menu size={20} />} onClick={onMenuClick} variant="ghost" />
        <div>
          <p className="text-sm font-semibold text-slate-900">Système de Gestion des Certificats de Naissance</p>
          <p className="text-xs text-slate-500">SGCN</p>
        </div>
      </div>
      <div className="flex items-center gap-3">
        <div className="hidden text-right sm:block">
          <p className="text-sm font-medium text-slate-900">{user?.fullName ?? 'Utilisateur'}</p>
          <p className="text-xs text-slate-500">{primaryRole(user)}</p>
        </div>
        <Button aria-label="Déconnexion" className="h-10 w-10 px-0" icon={<LogOut size={18} />} onClick={logout} variant="secondary" />
      </div>
    </header>
  );
}
