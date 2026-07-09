import { FormEvent, useEffect, useMemo, useState } from 'react';
import { Ban, Eye, FilePlus2, Search } from 'lucide-react';
import { Link } from 'react-router-dom';
import {
  annulCertificate,
  generateCertificateFromBirthRecord,
  generateCertificateFromRequest,
  getCertificates
} from '../../api/certificatesApi';
import Badge from '../../components/ui/Badge';
import Button from '../../components/ui/Button';
import Card from '../../components/ui/Card';
import Input from '../../components/ui/Input';
import Loader from '../../components/ui/Loader';
import Modal from '../../components/ui/Modal';
import Select from '../../components/ui/Select';
import Table from '../../components/ui/Table';
import { useAuth } from '../../store/authStore';
import type { Certificate } from '../../types/certificate';
import { formatDate, formatDateTime } from '../../utils/date';
import { hasAnyRole, hasRole, Roles } from '../../utils/roles';

type GenerateMode = 'request' | 'birth-record';

export default function CertificatesPage() {
  const { user } = useAuth();
  const [items, setItems] = useState<Certificate[]>([]);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [generateMode, setGenerateMode] = useState<GenerateMode>('request');
  const [generateId, setGenerateId] = useState('');
  const [generateOpen, setGenerateOpen] = useState(false);
  const [annulling, setAnnulling] = useState<Certificate | null>(null);
  const [annulReason, setAnnulReason] = useState('');

  const canGenerate = hasAnyRole(user, [Roles.Administrateur, Roles.AgentEtatCivil]);
  const canAnnul = hasRole(user, Roles.Administrateur);

  async function load() {
    setLoading(true);
    setError('');

    try {
      const result = await getCertificates();
      result.success ? setItems(result.data ?? []) : setError(result.message);
    } catch {
      setError('Impossible de charger les certificats.');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  const visibleItems = useMemo(() => {
    const normalized = search.trim().toLowerCase();
    if (!normalized) {
      return items;
    }

    return items.filter((item) =>
      [
        item.certificateNumber,
        item.sgcnId,
        item.childFirstName,
        item.childLastName,
        item.motherFullName,
        item.verificationCode
      ].some((value) => String(value).toLowerCase().includes(normalized))
    );
  }, [items, search]);

  async function submitGenerate(event: FormEvent) {
    event.preventDefault();
    setSaving(true);
    setError('');

    try {
      const result =
        generateMode === 'request'
          ? await generateCertificateFromRequest(generateId)
          : await generateCertificateFromBirthRecord(generateId);

      if (!result.success) {
        setError(result.message);
        return;
      }

      setGenerateOpen(false);
      setGenerateId('');
      await load();
    } catch {
      setError('Impossible de générer le certificat.');
    } finally {
      setSaving(false);
    }
  }

  async function submitAnnul(event: FormEvent) {
    event.preventDefault();
    if (!annulling) {
      return;
    }

    setSaving(true);
    setError('');

    try {
      const result = await annulCertificate(annulling.id, annulReason);
      if (!result.success) {
        setError(result.message);
        return;
      }

      setAnnulling(null);
      setAnnulReason('');
      await load();
    } catch {
      setError("Impossible d'annuler le certificat.");
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="grid gap-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-slate-950">Certificats</h1>
          <p className="mt-1 text-sm text-slate-500">Liste des certificats générés</p>
        </div>
        {canGenerate ? (
          <Button
            icon={<FilePlus2 size={18} />}
            onClick={() => {
              setGenerateMode('request');
              setGenerateId('');
              setGenerateOpen(true);
            }}
          >
            Générer
          </Button>
        ) : null}
      </div>

      <Card className="p-4">
        <div className="grid gap-3 md:grid-cols-[1fr_auto]">
          <Input label="Recherche" onChange={(event) => setSearch(event.target.value)} value={search} />
          <div className="flex items-end">
            <Button icon={<Search size={18} />} variant="secondary">
              Filtrer
            </Button>
          </div>
        </div>
      </Card>

      {error ? <p className="rounded-md bg-rose-50 px-3 py-2 text-sm text-rose-700">{error}</p> : null}

      <Card>
        {loading ? (
          <div className="p-5">
            <Loader />
          </div>
        ) : (
          <Table
            columns={[
              {
                header: 'Certificat',
                cell: (item) => (
                  <div>
                    <p className="font-medium text-slate-900">{item.certificateNumber}</p>
                    <p className="text-xs text-slate-500">{item.sgcnId}</p>
                  </div>
                )
              },
              {
                header: 'Enfant',
                cell: (item) => (
                  <div>
                    <p>
                      {item.childFirstName} {item.childLastName}
                    </p>
                    <p className="text-xs text-slate-500">{formatDate(item.birthDate)}</p>
                  </div>
                )
              },
              {
                header: 'Lieu',
                cell: (item) => (
                  <div>
                    <p>{item.hospitalName}</p>
                    <p className="text-xs text-slate-500">
                      {item.communeName}, {item.departmentName}
                    </p>
                  </div>
                )
              },
              {
                header: 'Statut',
                cell: (item) => (
                  <Badge tone={item.status === 'Active' ? 'success' : 'danger'}>{item.status}</Badge>
                )
              },
              { header: 'Vérification', cell: (item) => item.verificationCode },
              { header: 'Créé', cell: (item) => formatDateTime(item.createdAt) },
              {
                header: 'Actions',
                cell: (item) => (
                  <div className="flex flex-wrap gap-2">
                    <Link
                      className="inline-flex min-h-10 items-center justify-center gap-2 rounded-md bg-white px-4 py-2 text-sm font-medium text-slate-800 ring-1 ring-slate-200 transition hover:bg-slate-50"
                      to={
                        item.certificateRequestId
                          ? `/certificates/preview/from-request/${item.certificateRequestId}`
                          : `/certificates/preview/from-birth-record/${item.birthRecordId}`
                      }
                    >
                      <Eye size={16} />
                      Aperçu
                    </Link>
                    {canAnnul && item.status === 'Active' ? (
                      <Button
                        icon={<Ban size={16} />}
                        onClick={() => {
                          setAnnulling(item);
                          setAnnulReason('');
                        }}
                        variant="danger"
                      >
                        Annuler
                      </Button>
                    ) : null}
                  </div>
                )
              }
            ]}
            data={visibleItems}
            emptyText="Aucun certificat trouvé."
            keyExtractor={(item) => item.id}
          />
        )}
      </Card>

      <Modal isOpen={generateOpen} onClose={() => setGenerateOpen(false)} title="Générer un certificat">
        <form className="grid gap-4" onSubmit={submitGenerate}>
          <Select
            label="Source"
            onChange={(event) => setGenerateMode(event.target.value as GenerateMode)}
            value={generateMode}
          >
            <option value="request">Demande de certificat</option>
            <option value="birth-record">Acte de naissance</option>
          </Select>
          <Input
            label={generateMode === 'request' ? 'ID de la demande' : "ID de l'acte de naissance"}
            onChange={(event) => setGenerateId(event.target.value)}
            required
            value={generateId}
          />
          <Button disabled={saving} icon={<FilePlus2 size={18} />} type="submit">
            {saving ? 'Génération...' : 'Générer'}
          </Button>
        </form>
      </Modal>

      <Modal isOpen={Boolean(annulling)} onClose={() => setAnnulling(null)} title="Annuler le certificat">
        <form className="grid gap-4" onSubmit={submitAnnul}>
          <Input
            label="Motif"
            onChange={(event) => setAnnulReason(event.target.value)}
            required
            value={annulReason}
          />
          <Button disabled={saving} type="submit" variant="danger">
            Annuler le certificat
          </Button>
        </form>
      </Modal>
    </div>
  );
}
