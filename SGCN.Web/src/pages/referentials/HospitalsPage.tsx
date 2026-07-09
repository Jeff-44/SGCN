import { FormEvent, useEffect, useState } from 'react';
import { Edit, Plus, RotateCw, Search } from 'lucide-react';
import {
  activateHospital,
  createHospital,
  deactivateHospital,
  getCommunes,
  getHospitals,
  updateHospital
} from '../../api/referentialsApi';
import Badge from '../../components/ui/Badge';
import Button from '../../components/ui/Button';
import Card from '../../components/ui/Card';
import Input from '../../components/ui/Input';
import Loader from '../../components/ui/Loader';
import Modal from '../../components/ui/Modal';
import Select from '../../components/ui/Select';
import Table from '../../components/ui/Table';
import type { Commune, Hospital } from '../../types/referentials';

export default function HospitalsPage() {
  const [items, setItems] = useState<Hospital[]>([]);
  const [communes, setCommunes] = useState<Commune[]>([]);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<Hospital | null>(null);
  const [form, setForm] = useState({ name: '', code: '', communeId: '', address: '' });

  async function load() {
    setLoading(true);
    setError('');
    try {
      const [hospitalsResult, communesResult] = await Promise.all([getHospitals({ search: search || undefined }), getCommunes()]);
      hospitalsResult.success ? setItems(hospitalsResult.data ?? []) : setError(hospitalsResult.message);
      if (communesResult.success) {
        setCommunes(communesResult.data ?? []);
      }
    } catch {
      setError('Impossible de charger les hôpitaux.');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  function openCreate() {
    setEditing(null);
    setForm({ name: '', code: '', communeId: communes[0]?.id ?? '', address: '' });
    setModalOpen(true);
  }

  function openEdit(item: Hospital) {
    setEditing(item);
    setForm({ name: item.name, code: item.code ?? '', communeId: item.communeId, address: item.address ?? '' });
    setModalOpen(true);
  }

  async function submit(event: FormEvent) {
    event.preventDefault();
    const result = editing ? await updateHospital(editing.id, form) : await createHospital(form);
    if (!result.success) {
      setError(result.message);
      return;
    }

    setModalOpen(false);
    await load();
  }

  async function toggle(item: Hospital) {
    const result = item.isActive ? await deactivateHospital(item.id) : await activateHospital(item.id);
    result.success ? await load() : setError(result.message);
  }

  return (
    <div className="grid gap-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-slate-950">Hospitals</h1>
          <p className="mt-1 text-sm text-slate-500">Référentiel hospitalier</p>
        </div>
        <Button icon={<Plus size={18} />} onClick={openCreate}>Ajouter</Button>
      </div>
      <Card className="p-4">
        <div className="grid gap-3 md:grid-cols-[1fr_auto]">
          <Input label="Recherche" onChange={(event) => setSearch(event.target.value)} value={search} />
          <div className="flex items-end">
            <Button icon={<Search size={18} />} onClick={load} variant="secondary">Rechercher</Button>
          </div>
        </div>
      </Card>
      {error ? <p className="rounded-md bg-rose-50 px-3 py-2 text-sm text-rose-700">{error}</p> : null}
      <Card>{loading ? <div className="p-5"><Loader /></div> : (
        <Table
          columns={[
            { header: 'Nom', cell: (item) => <span className="font-medium text-slate-900">{item.name}</span> },
            { header: 'Commune', cell: (item) => item.communeName },
            { header: 'Département', cell: (item) => item.departmentName },
            { header: 'Statut', cell: (item) => <Badge tone={item.isActive ? 'success' : 'muted'}>{item.isActive ? 'Actif' : 'Inactif'}</Badge> },
            {
              header: 'Actions',
              cell: (item) => (
                <div className="flex flex-wrap gap-2">
                  <Button icon={<Edit size={16} />} onClick={() => openEdit(item)} variant="secondary">Modifier</Button>
                  <Button icon={<RotateCw size={16} />} onClick={() => toggle(item)} variant="secondary">
                    {item.isActive ? 'Désactiver' : 'Activer'}
                  </Button>
                </div>
              )
            }
          ]}
          data={items}
          keyExtractor={(item) => item.id}
        />
      )}</Card>
      <Modal isOpen={modalOpen} onClose={() => setModalOpen(false)} title={editing ? "Modifier l'hôpital" : "Créer un hôpital"}>
        <form className="grid gap-4" onSubmit={submit}>
          <Input label="Nom" onChange={(event) => setForm((current) => ({ ...current, name: event.target.value }))} required value={form.name} />
          <Input label="Code" onChange={(event) => setForm((current) => ({ ...current, code: event.target.value }))} value={form.code} />
          <Select label="Commune" onChange={(event) => setForm((current) => ({ ...current, communeId: event.target.value }))} required value={form.communeId}>
            <option value="">Choisir</option>
            {communes.map((commune) => <option key={commune.id} value={commune.id}>{commune.name}</option>)}
          </Select>
          <Input label="Adresse" onChange={(event) => setForm((current) => ({ ...current, address: event.target.value }))} value={form.address} />
          <Button type="submit">{editing ? 'Enregistrer' : 'Créer'}</Button>
        </form>
      </Modal>
    </div>
  );
}
