import { FormEvent, useState } from 'react';
import { Link, Navigate, useNavigate } from 'react-router-dom';
import { LogIn } from 'lucide-react';
import { login } from '../../api/authApi';
import Button from '../../components/ui/Button';
import Card from '../../components/ui/Card';
import Input from '../../components/ui/Input';
import { authStore, useAuth } from '../../store/authStore';

export default function LoginPage() {
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  if (isAuthenticated) {
    return <Navigate replace to="/" />;
  }

  async function submit(event: FormEvent) {
    event.preventDefault();
    setError('');
    setLoading(true);

    try {
      const result = await login({ email, password });
      if (!result.success || !result.data) {
        setError(result.message);
        return;
      }

      authStore.setSession(result.data);
      navigate(result.data.forcePasswordChange ? '/change-password' : '/', { replace: true });
    } catch {
      setError('Connexion impossible pour le moment.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="grid min-h-screen place-items-center bg-slate-50 px-4 py-10">
      <Card className="w-full max-w-md p-6">
        <div className="mb-6">
          <p className="text-2xl font-bold text-slate-950">SGCN</p>
          <p className="mt-1 text-sm text-slate-500">Système de Gestion des Certificats de Naissance</p>
        </div>
        <form className="grid gap-4" onSubmit={submit}>
          <Input autoComplete="email" label="Email" onChange={(event) => setEmail(event.target.value)} required type="email" value={email} />
          <Input
            autoComplete="current-password"
            label="Mot de passe"
            onChange={(event) => setPassword(event.target.value)}
            required
            type="password"
            value={password}
          />
          {error ? <p className="rounded-md bg-rose-50 px-3 py-2 text-sm text-rose-700">{error}</p> : null}
          <Button disabled={loading} icon={<LogIn size={18} />} type="submit">
            {loading ? 'Connexion...' : 'Se connecter'}
          </Button>
        </form>
        <div className="mt-5 flex flex-wrap items-center justify-between gap-3 text-sm">
          <Link className="text-slate-600 hover:text-slate-950" to="/register">
            Créer un compte
          </Link>
          <Link className="text-slate-600 hover:text-slate-950" to="/forgot-password">
            Mot de passe oublié
          </Link>
        </div>
      </Card>
    </main>
  );
}
