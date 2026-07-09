import { FormEvent, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { UserPlus } from 'lucide-react';
import { register } from '../../api/authApi';
import Button from '../../components/ui/Button';
import Card from '../../components/ui/Card';
import Input from '../../components/ui/Input';

export default function RegisterPage() {
  const navigate = useNavigate();
  const [form, setForm] = useState({
    fullName: '',
    email: '',
    password: '',
    nifOrCin: '',
    phoneNumber: ''
  });
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  function update(name: string, value: string) {
    setForm((current) => ({ ...current, [name]: value }));
  }

  async function submit(event: FormEvent) {
    event.preventDefault();
    setError('');
    setMessage('');
    setLoading(true);

    try {
      const result = await register(form);
      if (!result.success) {
        setError(result.message);
        return;
      }

      setMessage('Compte créé avec succès.');
      setTimeout(() => navigate('/login'), 600);
    } catch {
      setError('Inscription impossible pour le moment.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="grid min-h-screen place-items-center bg-slate-50 px-4 py-10">
      <Card className="w-full max-w-xl p-6">
        <div className="mb-6">
          <p className="text-2xl font-bold text-slate-950">Créer un compte citoyen</p>
          <p className="mt-1 text-sm text-slate-500">SGCN</p>
        </div>
        <form className="grid gap-4 sm:grid-cols-2" onSubmit={submit}>
          <Input className="sm:col-span-2" label="Nom complet" onChange={(event) => update('fullName', event.target.value)} required value={form.fullName} />
          <Input label="Email" onChange={(event) => update('email', event.target.value)} required type="email" value={form.email} />
          <Input label="Mot de passe" onChange={(event) => update('password', event.target.value)} required type="password" value={form.password} />
          <Input label="NIF ou CIN" onChange={(event) => update('nifOrCin', event.target.value)} value={form.nifOrCin} />
          <Input label="Téléphone" onChange={(event) => update('phoneNumber', event.target.value)} value={form.phoneNumber} />
          {error ? <p className="rounded-md bg-rose-50 px-3 py-2 text-sm text-rose-700 sm:col-span-2">{error}</p> : null}
          {message ? <p className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700 sm:col-span-2">{message}</p> : null}
          <Button className="sm:col-span-2" disabled={loading} icon={<UserPlus size={18} />} type="submit">
            {loading ? 'Création...' : 'Créer le compte'}
          </Button>
        </form>
        <Link className="mt-5 inline-flex text-sm text-slate-600 hover:text-slate-950" to="/login">
          Retour à la connexion
        </Link>
      </Card>
    </main>
  );
}
