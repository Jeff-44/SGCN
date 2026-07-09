import { FormEvent, useState } from 'react';
import { Link } from 'react-router-dom';
import { forgotPassword } from '../../api/authApi';
import Button from '../../components/ui/Button';
import Card from '../../components/ui/Card';
import Input from '../../components/ui/Input';

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState('');
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  async function submit(event: FormEvent) {
    event.preventDefault();
    setError('');
    setMessage('');
    setLoading(true);

    try {
      const result = await forgotPassword(email);
      result.success ? setMessage(result.message) : setError(result.message);
    } catch {
      setError('Demande impossible pour le moment.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="grid min-h-screen place-items-center bg-slate-50 px-4 py-10">
      <Card className="w-full max-w-md p-6">
        <p className="text-xl font-bold text-slate-950">Mot de passe oublié</p>
        <form className="mt-5 grid gap-4" onSubmit={submit}>
          <Input label="Email" onChange={(event) => setEmail(event.target.value)} required type="email" value={email} />
          {error ? <p className="rounded-md bg-rose-50 px-3 py-2 text-sm text-rose-700">{error}</p> : null}
          {message ? <p className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{message}</p> : null}
          <Button disabled={loading} type="submit">
            {loading ? 'Envoi...' : 'Envoyer'}
          </Button>
        </form>
        <Link className="mt-5 inline-flex text-sm text-slate-600 hover:text-slate-950" to="/login">
          Retour à la connexion
        </Link>
      </Card>
    </main>
  );
}
