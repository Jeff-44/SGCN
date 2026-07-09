import { useEffect, useState } from 'react';
import { FilePlus2 } from 'lucide-react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  generateCertificateFromBirthRecord,
  generateCertificateFromRequest,
  previewCertificateFromBirthRecord,
  previewCertificateFromRequest
} from '../../api/certificatesApi';
import Button from '../../components/ui/Button';
import Card from '../../components/ui/Card';
import Loader from '../../components/ui/Loader';
import { useAuth } from '../../store/authStore';
import type { CertificatePreview } from '../../types/certificate';
import { formatDate } from '../../utils/date';
import { hasAnyRole, Roles } from '../../utils/roles';

type CertificatePreviewPageProps = {
  source: 'request' | 'birth-record';
};

export default function CertificatePreviewPage({ source }: CertificatePreviewPageProps) {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user } = useAuth();
  const [preview, setPreview] = useState<CertificatePreview | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const canGenerate = hasAnyRole(user, [Roles.Administrateur, Roles.AgentEtatCivil]);

  useEffect(() => {
    async function load() {
      if (!id) {
        setError('Identifiant introuvable.');
        setLoading(false);
        return;
      }

      setLoading(true);
      setError('');

      try {
        const result =
          source === 'request'
            ? await previewCertificateFromRequest(id)
            : await previewCertificateFromBirthRecord(id);

        result.success ? setPreview(result.data ?? null) : setError(result.message);
      } catch {
        setError("Impossible de charger l'aperçu du certificat.");
      } finally {
        setLoading(false);
      }
    }

    void load();
  }, [id, source]);

  async function generate() {
    if (!id) {
      return;
    }

    setSaving(true);
    setError('');

    try {
      const result =
        source === 'request'
          ? await generateCertificateFromRequest(id)
          : await generateCertificateFromBirthRecord(id);

      if (!result.success) {
        setError(result.message);
        return;
      }

      navigate('/certificates');
    } catch {
      setError('Impossible de générer le certificat.');
    } finally {
      setSaving(false);
    }
  }

  const sourceLabel = source === 'request' ? 'demande' : 'acte de naissance';

  return (
    <div className="grid gap-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-slate-950">Aperçu du certificat</h1>
          <p className="mt-1 text-sm text-slate-500">Source: {sourceLabel}</p>
        </div>
        {canGenerate && preview ? (
          <Button disabled={saving} icon={<FilePlus2 size={18} />} onClick={generate}>
            {saving ? 'Génération...' : 'Générer'}
          </Button>
        ) : null}
      </div>

      {error ? <p className="rounded-md bg-rose-50 px-3 py-2 text-sm text-rose-700">{error}</p> : null}

      {loading ? (
        <Card className="p-5">
          <Loader />
        </Card>
      ) : preview ? (
        <Card className="p-6">
          <div className="mx-auto grid max-w-3xl gap-6 rounded-md border border-slate-200 p-6">
            <div className="border-b border-slate-200 pb-4 text-center">
              <p className="text-xs font-semibold uppercase text-slate-500">Système de Gestion des Certificats de Naissance</p>
              <h2 className="mt-2 text-xl font-bold text-slate-950">Certificat de naissance</h2>
            </div>

            <div className="grid gap-4 md:grid-cols-2">
              <Detail label="Nom de l'enfant" value={`${preview.childFirstName} ${preview.childLastName}`} />
              <Detail label="Sexe" value={preview.gender === 'Male' ? 'Masculin' : 'Féminin'} />
              <Detail label="Date de naissance" value={formatDate(preview.birthDate)} />
              <Detail label="Heure de naissance" value={preview.birthTime} />
              <Detail label="Lieu de naissance" value={preview.birthPlace} />
              <Detail label="Hôpital" value={preview.hospitalName} />
              <Detail label="Commune" value={preview.communeName} />
              <Detail label="Département" value={preview.departmentName} />
              <Detail label="Mère" value={preview.motherFullName} />
              <Detail label="Père" value={preview.fatherFullName ?? '-'} />
              <Detail label="SGCN ID" value={preview.sgcnId} />
              <Detail label="Demande" value={preview.requestNumber ?? '-'} />
            </div>
          </div>
        </Card>
      ) : (
        <Card className="p-5 text-sm text-slate-500">Aucun aperçu disponible.</Card>
      )}
    </div>
  );
}

function Detail({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <p className="text-xs font-semibold uppercase text-slate-500">{label}</p>
      <p className="mt-1 text-sm text-slate-900">{value}</p>
    </div>
  );
}
