import { FormEvent, useMemo, useState, useEffect } from 'react';
import { Eye, Link2, Plus, Search, XCircle } from 'lucide-react';
import { Link } from 'react-router-dom';
import {
  cancelCertificateRequest,
  createCertificateRequest,
  getCertificateRequests,
  getMatchingBirthRecords,
  linkBirthRecord,
  rejectCertificateRequest
} from '../../api/certificateRequestsApi';
import Badge from '../../components/ui/Badge';
import Button from '../../components/ui/Button';
import Card from '../../components/ui/Card';
import Input from '../../components/ui/Input';
import Loader from '../../components/ui/Loader';
import Modal from '../../components/ui/Modal';
import Select from '../../components/ui/Select';
import Table from '../../components/ui/Table';
import { useAuth } from '../../store/authStore';
import type { BirthRecord, Gender } from '../../types/birthRecord';
import type {
  CertificateRequest,
  CertificateRequestStatus,
  CreateCertificateRequestRequest
} from '../../types/certificateRequest';
import { formatDate, formatDateTime } from '../../utils/date';
import { hasAnyRole, hasRole, Roles } from '../../utils/roles';

const initialForm: CreateCertificateRequestRequest = {
  targetFirstName: '',
  targetLastName: '',
  targetGender: 'Male',
  targetBirthDate: '',
  motherFullName: '',
  fatherFullName: '',
  birthPlace: '',
  hospitalFileNumber: '',
  relationshipToTarget: ''
};

function statusTone(status: CertificateRequestStatus) {
  if (status === 'CertificateCreated') {
    return 'success' as const;
  }

  if (status === 'Rejected' || status === 'Cancelled') {
    return 'danger' as const;
  }

  if (status === 'InProgress') {
    return 'warning' as const;
  }

  return 'default' as const;
}

export default function CertificateRequestsPage() {
  const { user } = useAuth();
  const [items, setItems] = useState<CertificateRequest[]>([]);
  const [matches, setMatches] = useState<BirthRecord[]>([]);
  const [selectedRequest, setSelectedRequest] = useState<CertificateRequest | null>(null);
  const [rejectingRequest, setRejectingRequest] = useState<CertificateRequest | null>(null);
  const [rejectReason, setRejectReason] = useState('');
  const [search, setSearch] = useState('');
  const [createOpen, setCreateOpen] = useState(false);
  const [form, setForm] = useState<CreateCertificateRequestRequest>(initialForm);
  const [loading, setLoading] = useState(true);
  const [matching, setMatching] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const isCitizen = hasRole(user, Roles.Citoyen);
  const canProcess = hasAnyRole(user, [Roles.Administrateur, Roles.AgentEtatCivil]);

  async function load() {
    setLoading(true);
    setError('');

    try {
      const result = await getCertificateRequests();
      result.success ? setItems(result.data ?? []) : setError(result.message);
    } catch {
      setError('Impossible de charger les demandes de certificat.');
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
        item.requestNumber,
        item.requestedByFullName,
        item.targetFirstName,
        item.targetLastName,
        item.motherFullName,
        item.birthPlace,
        item.linkedBirthRecordSgcnId
      ]
        .filter(Boolean)
        .some((value) => String(value).toLowerCase().includes(normalized))
    );
  }, [items, search]);

  function updateField<K extends keyof CreateCertificateRequestRequest>(
    key: K,
    value: CreateCertificateRequestRequest[K]
  ) {
    setForm((current) => ({ ...current, [key]: value }));
  }

  async function submitCreate(event: FormEvent) {
    event.preventDefault();
    setSaving(true);
    setError('');

    try {
      const result = await createCertificateRequest(form);
      if (!result.success) {
        setError(result.message);
        return;
      }

      setForm(initialForm);
      setCreateOpen(false);
      await load();
    } catch {
      setError('Impossible de créer la demande.');
    } finally {
      setSaving(false);
    }
  }

  async function cancelRequest(item: CertificateRequest) {
    const result = await cancelCertificateRequest(item.id);
    result.success ? await load() : setError(result.message);
  }

  async function openMatches(item: CertificateRequest) {
    setSelectedRequest(item);
    setMatches([]);
    setMatching(true);
    setError('');

    try {
      const result = await getMatchingBirthRecords(item.id);
      result.success ? setMatches(result.data ?? []) : setError(result.message);
    } catch {
      setError('Impossible de charger les actes correspondants.');
    } finally {
      setMatching(false);
    }
  }

  async function submitLink(birthRecordId: string) {
    if (!selectedRequest) {
      return;
    }

    const result = await linkBirthRecord(selectedRequest.id, birthRecordId);
    if (!result.success) {
      setError(result.message);
      return;
    }

    setSelectedRequest(null);
    await load();
  }

  async function submitReject(event: FormEvent) {
    event.preventDefault();
    if (!rejectingRequest) {
      return;
    }

    setSaving(true);
    const result = await rejectCertificateRequest(rejectingRequest.id, rejectReason);
    if (!result.success) {
      setError(result.message);
      setSaving(false);
      return;
    }

    setRejectingRequest(null);
    setRejectReason('');
    setSaving(false);
    await load();
  }

  return (
    <div className="grid gap-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-slate-950">{isCitizen ? 'Mes demandes' : 'Demandes de certificat'}</h1>
          <p className="mt-1 text-sm text-slate-500">Suivi et traitement des demandes</p>
        </div>
        {isCitizen ? (
          <Button icon={<Plus size={18} />} onClick={() => setCreateOpen(true)}>
            Nouvelle demande
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
                header: 'Demande',
                cell: (item) => (
                  <div>
                    <p className="font-medium text-slate-900">{item.requestNumber}</p>
                    <p className="text-xs text-slate-500">{formatDateTime(item.createdAt)}</p>
                  </div>
                )
              },
              {
                header: 'Personne',
                cell: (item) => (
                  <div>
                    <p>
                      {item.targetFirstName} {item.targetLastName}
                    </p>
                    <p className="text-xs text-slate-500">{formatDate(item.targetBirthDate)}</p>
                  </div>
                )
              },
              { header: 'Demandeur', cell: (item) => item.requestedByFullName },
              { header: 'Mère', cell: (item) => item.motherFullName },
              { header: 'Statut', cell: (item) => <Badge tone={statusTone(item.status)}>{item.status}</Badge> },
              {
                header: 'Lien',
                cell: (item) => item.linkedBirthRecordSgcnId ?? '-'
              },
              {
                header: 'Actions',
                cell: (item) => (
                  <div className="flex flex-wrap gap-2">
                    {canProcess ? (
                      <Button icon={<Search size={16} />} onClick={() => openMatches(item)} variant="secondary">
                        Correspondances
                      </Button>
                    ) : null}
                    {canProcess && item.linkedBirthRecordId ? (
                      <Link
                        className="inline-flex min-h-10 items-center justify-center gap-2 rounded-md bg-white px-4 py-2 text-sm font-medium text-slate-800 ring-1 ring-slate-200 transition hover:bg-slate-50"
                        to={`/certificates/preview/from-request/${item.id}`}
                      >
                        <Eye size={16} />
                        Aperçu
                      </Link>
                    ) : null}
                    {canProcess && item.status !== 'CertificateCreated' && item.status !== 'Rejected' ? (
                      <Button
                        icon={<XCircle size={16} />}
                        onClick={() => {
                          setRejectingRequest(item);
                          setRejectReason('');
                        }}
                        variant="danger"
                      >
                        Rejeter
                      </Button>
                    ) : null}
                    {isCitizen && item.status === 'Pending' ? (
                      <Button icon={<XCircle size={16} />} onClick={() => cancelRequest(item)} variant="secondary">
                        Annuler
                      </Button>
                    ) : null}
                  </div>
                )
              }
            ]}
            data={visibleItems}
            emptyText="Aucune demande trouvée."
            keyExtractor={(item) => item.id}
          />
        )}
      </Card>

      <Modal isOpen={createOpen} onClose={() => setCreateOpen(false)} title="Créer une demande">
        <form className="grid gap-4" onSubmit={submitCreate}>
          <div className="grid gap-4 md:grid-cols-2">
            <Input
              label="Prénom"
              onChange={(event) => updateField('targetFirstName', event.target.value)}
              required
              value={form.targetFirstName}
            />
            <Input
              label="Nom"
              onChange={(event) => updateField('targetLastName', event.target.value)}
              required
              value={form.targetLastName}
            />
            <Select
              label="Sexe"
              onChange={(event) => updateField('targetGender', event.target.value as Gender)}
              required
              value={form.targetGender}
            >
              <option value="Male">Masculin</option>
              <option value="Female">Féminin</option>
            </Select>
            <Input
              label="Date de naissance"
              onChange={(event) => updateField('targetBirthDate', event.target.value)}
              required
              type="date"
              value={form.targetBirthDate}
            />
            <Input
              label="Nom complet de la mère"
              onChange={(event) => updateField('motherFullName', event.target.value)}
              required
              value={form.motherFullName}
            />
            <Input
              label="Nom complet du père"
              onChange={(event) => updateField('fatherFullName', event.target.value)}
              value={form.fatherFullName}
            />
            <Input
              label="Lieu de naissance"
              onChange={(event) => updateField('birthPlace', event.target.value)}
              required
              value={form.birthPlace}
            />
            <Input
              label="Numéro de dossier hospitalier"
              onChange={(event) => updateField('hospitalFileNumber', event.target.value)}
              value={form.hospitalFileNumber}
            />
            <Input
              label="Lien avec la personne"
              onChange={(event) => updateField('relationshipToTarget', event.target.value)}
              required
              value={form.relationshipToTarget}
            />
          </div>
          <Button disabled={saving} type="submit">
            {saving ? 'Création...' : 'Créer la demande'}
          </Button>
        </form>
      </Modal>

      <Modal
        isOpen={Boolean(selectedRequest)}
        onClose={() => setSelectedRequest(null)}
        title="Actes correspondants"
      >
        {matching ? (
          <Loader />
        ) : (
          <Table
            columns={[
              {
                header: 'Acte',
                cell: (item) => (
                  <div>
                    <p className="font-medium text-slate-900">{item.sgcnId}</p>
                    <p className="text-xs text-slate-500">
                      {item.childFirstName} {item.childLastName}
                    </p>
                  </div>
                )
              },
              { header: 'Naissance', cell: (item) => formatDate(item.birthDate) },
              { header: 'Mère', cell: (item) => item.motherFullName },
              {
                header: 'Action',
                cell: (item) => (
                  <Button icon={<Link2 size={16} />} onClick={() => submitLink(item.id)} variant="secondary">
                    Lier
                  </Button>
                )
              }
            ]}
            data={matches}
            emptyText="Aucune correspondance trouvée."
            keyExtractor={(item) => item.id}
          />
        )}
      </Modal>

      <Modal
        isOpen={Boolean(rejectingRequest)}
        onClose={() => setRejectingRequest(null)}
        title="Rejeter la demande"
      >
        <form className="grid gap-4" onSubmit={submitReject}>
          <Input
            label="Motif"
            onChange={(event) => setRejectReason(event.target.value)}
            required
            value={rejectReason}
          />
          <Button disabled={saving} type="submit" variant="danger">
            Rejeter
          </Button>
        </form>
      </Modal>
    </div>
  );
}
