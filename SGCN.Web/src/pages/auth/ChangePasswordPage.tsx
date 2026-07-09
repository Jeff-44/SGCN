import { FormEvent, useState } from 'react';
import { KeyRound } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { changePassword } from '../../api/authApi';
import Button from '../../components/ui/Button';
import Card from '../../components/ui/Card';
import Input from '../../components/ui/Input';
import { authStore } from '../../store/authStore';

export default function ChangePasswordPage() {
  const navigate = useNavigate();
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  async function submit(event: FormEvent) {
    event.preventDefault();
    setError('');

    if (newPassword !== confirmPassword) {
      setError('Le nouveau mot de passe et la confirmation ne correspondent pas.');
      return;
    }

    setLoading(true);

    try {
      const result = await changePassword({
        currentPassword,
        newPassword,
        confirmPassword
      });

      if (!result.success || !result.data) {
        setError(result.message);
        return;
      }

      authStore.setSession(result.data);
      navigate('/', { replace: true });
    } catch {
      setError('Impossible de changer le mot de passe pour le moment.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="grid min-h-screen place-items-center bg-slate-50 px-4 py-10">
      <Card className="w-full max-w-md p-6">
        <div className="mb-6">
          <p className="text-2xl font-bold text-slate-950">Changer le mot de passe</p>
          <p className="mt-1 text-sm text-slate-500">SGCN</p>
        </div>
        <form className="grid gap-4" onSubmit={submit}>
          <Input
            autoComplete="current-password"
            label="Mot de passe actuel"
            onChange={(event) => setCurrentPassword(event.target.value)}
            required
            type="password"
            value={currentPassword}
          />
          <Input
            autoComplete="new-password"
            label="Nouveau mot de passe"
            onChange={(event) => setNewPassword(event.target.value)}
            required
            type="password"
            value={newPassword}
          />
          <Input
            autoComplete="new-password"
            label="Confirmer le mot de passe"
            onChange={(event) => setConfirmPassword(event.target.value)}
            required
            type="password"
            value={confirmPassword}
          />
          {error ? <p className="rounded-md bg-rose-50 px-3 py-2 text-sm text-rose-700">{error}</p> : null}
          <Button disabled={loading} icon={<KeyRound size={18} />} type="submit">
            {loading ? 'Enregistrement...' : 'Enregistrer'}
          </Button>
        </form>
      </Card>
    </main>
  );
}
