import { useEffect, useMemo, useState } from 'react';
import { Eye, Plus, RotateCw, Search } from 'lucide-react';
import { Link } from 'react-router-dom';
import {
  activateBirthRecord,
  deactivateBirthRecord,
  getBirthRecords,
  getBirthRecordsWithoutCertificate
} from '../../api/birthRecordsApi';
import Badge from '../../components/ui/Badge';
import Button from '../../components/ui/Button';
import Card from '../../components/ui/Card';
import Input from '../../components/ui/Input';
import Loader from '../../components/ui/Loader';
import Table from '../../components/ui/Table';
import { useAuth } from '../../store/authStore';
import type { BirthRecord } from '../../types/birthRecord';
import { formatDate, formatDateTime } from '../../utils/date';
import { hasAnyRole, hasRole, Roles } from '../../utils/roles';

type BirthRecordsPageProps = {
  mode?: 'pending';
};

export default function BirthRecordsPage({ mode }: BirthRecordsPageProps) {
  const { user } = useAuth();
  const [items, setItems] = useState<BirthRecord[]>([]);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const isPendingMode = mode === 'pending';
  const canCreate = hasRole(user, Roles.Medecin);
  const canToggle = hasRole(user, Roles.Administrateur);
  const canPreview = hasAnyRole(user, [Roles.Administrateur, Roles.AgentEtatCivil]);

  async function load() {
    setLoading(true);
    setError('');

    try {
      const result = isPendingMode
        ? await getBirthRecordsWithoutCertificate({ search: search || undefined })
        : await getBirthRecords({ search: search || undefined });

      result.success ? setItems(result.data ?? []) : setError(result.message);
    } catch {
      setError('Impossible de charger les actes de naissance.');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, [isPendingMode]);

  const title = isPendingMode ? 'Actes en attente de certificat' : 'Actes de naissance';
  const subtitle = isPendingMode
    ? 'Actes disponibles pour la génération de certificat'
    : 'Registre des naissances enregistrées';

  const columns = useMemo(
    () => [
      {
        header: 'Enfant',
        cell: (item: BirthRecord) => (
          <div>
            <p className="font-medium text-slate-900">
              {item.childFirstName} {item.childLastName}
            </p>
            <p className="text-xs text-slate-500">{item.sgcnId}</p>
          </div>
        )
      },
      {
        header: 'Naissance',
        cell: (item: BirthRecord) => (
          <div>
            <p>{formatDate(item.birthDate)}</p>
            <p className="text-xs text-slate-500">
              {item.birthTime} - {item.birthPlace}
            </p>
          </div>
        )
      },
      {
        header: 'Lieu',
        cell: (item: BirthRecord) => (
          <div>
            <p>{item.hospitalName}</p>
            <p className="text-xs text-slate-500">
              {item.communeName}, {item.departmentName}
            </p>
          </div>
        )
      },
      { header: 'Mère', cell: (item: BirthRecord) => item.motherFullName },
      {
        header: 'Statut',
        cell: (item: BirthRecord) => (
          <div className="flex flex-wrap gap-2">
            <Badge tone={item.isActive ? 'success' : 'muted'}>{item.isActive ? 'Actif' : 'Inactif'}</Badge>
            <Badge tone={item.isLocked ? 'warning' : 'default'}>{item.isLocked ? 'Verrouillé' : 'Ouvert'}</Badge>
          </div>
        )
      },
      {
        header: 'Créé',
        cell: (item: BirthRecord) => (
          <div>
            <p>{formatDateTime(item.createdAt)}</p>
            <p className="text-xs text-slate-500">{item.createdByFullName}</p>
          </div>
        )
      },
      {
        header: 'Actions',
        cell: (item: BirthRecord) => (
          <div className="flex flex-wrap gap-2">
            {canPreview ? (
              <Link
                className="inline-flex min-h-10 items-center justify-center gap-2 rounded-md bg-white px-4 py-2 text-sm font-medium text-slate-800 ring-1 ring-slate-200 transition hover:bg-slate-50"
                to={`/certificates/preview/from-birth-record/${item.id}`}
              >
                <Eye size={16} />
                Aperçu
              </Link>
            ) : null}
            {canToggle ? (
              <Button icon={<RotateCw size={16} />} onClick={() => toggle(item)} variant="secondary">
                {item.isActive ? 'Désactiver' : 'Activer'}
              </Button>
            ) : null}
          </div>
        )
      }
    ],
    [canPreview, canToggle]
  );

  async function toggle(item: BirthRecord) {
    const result = item.isActive ? await deactivateBirthRecord(item.id) : await activateBirthRecord(item.id);
    result.success ? await load() : setError(result.message);
  }

  return (
    <div className="grid gap-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-slate-950">{title}</h1>
          <p className="mt-1 text-sm text-slate-500">{subtitle}</p>
        </div>
        {canCreate && !isPendingMode ? (
          <Link
            className="inline-flex min-h-10 items-center justify-center gap-2 rounded-md bg-slate-900 px-4 py-2 text-sm font-medium text-white transition hover:bg-slate-800"
            to="/birth-records/create"
          >
            <Plus size={18} />
            Créer
          </Link>
        ) : null}
      </div>

      <Card className="p-4">
        <form className="grid gap-3 md:grid-cols-[1fr_auto]" onSubmit={(event) => { event.preventDefault(); void load(); }}>
          <Input label="Recherche" onChange={(event) => setSearch(event.target.value)} value={search} />
          <div className="flex items-end">
            <Button icon={<Search size={18} />} type="submit" variant="secondary">
              Rechercher
            </Button>
          </div>
        </form>
      </Card>

      {error ? <p className="rounded-md bg-rose-50 px-3 py-2 text-sm text-rose-700">{error}</p> : null}

      <Card>
        {loading ? (
          <div className="p-5">
            <Loader />
          </div>
        ) : (
          <Table
            columns={columns}
            data={items}
            emptyText={isPendingMode ? 'Aucun acte en attente de certificat.' : 'Aucun acte trouvé.'}
            keyExtractor={(item) => item.id}
          />
        )}
      </Card>
    </div>
  );
}
