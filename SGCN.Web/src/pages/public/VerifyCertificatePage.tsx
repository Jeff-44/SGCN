import { ArrowLeft, Search, ShieldCheck } from 'lucide-react';
import { Link } from 'react-router-dom';
import Button from '../../components/ui/Button';
import Card from '../../components/ui/Card';
import Input from '../../components/ui/Input';

export default function VerifyCertificatePage() {
  return (
    <main className="min-h-screen bg-slate-50 px-4 py-8">
      <div className="mx-auto grid max-w-2xl gap-5">
        <Link className="inline-flex items-center gap-2 text-sm font-medium text-slate-700 hover:text-slate-950" to="/login">
          <ArrowLeft size={16} />
          Retour
        </Link>

        <Card className="p-6">
          <div className="grid gap-5">
            <div className="flex items-start gap-3">
              <div className="grid h-11 w-11 place-items-center rounded-md bg-slate-900 text-white">
                <ShieldCheck size={22} />
              </div>
              <div>
                <p className="text-sm font-semibold text-slate-500">SGCN</p>
                <h1 className="text-2xl font-bold text-slate-950">Vérification de certificat</h1>
              </div>
            </div>

            <p className="text-sm leading-6 text-slate-600">
              La vérification publique sera activée quand l'API exposera un endpoint dédié au code de vérification.
            </p>

            {/* TODO: Connect this form to the future public certificate verification endpoint. */}
            <form className="grid gap-3 md:grid-cols-[1fr_auto]" onSubmit={(event) => event.preventDefault()}>
              <Input disabled label="Code de vérification" placeholder="Ex: SGCN-XXXX" />
              <div className="flex items-end">
                <Button disabled icon={<Search size={18} />} variant="secondary">
                  Vérifier
                </Button>
              </div>
            </form>
          </div>
        </Card>
      </div>
    </main>
  );
}
