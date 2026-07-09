import { FormEvent, useEffect, useState } from 'react';
import { Edit, Plus, RotateCw, Search } from 'lucide-react';
import {
  activateCommune,
  createCommune,
  deactivateCommune,
  getCommunes,
  getDepartments,
  updateCommune
} from '../../api/referentialsApi';
import Badge from '../../components/ui/Badge';
import Button from '../../components/ui/Button';
import Card from '../../components/ui/Card';
import Input from '../../components/ui/Input';
import Loader from '../../components/ui/Loader';
import Modal from '../../components/ui/Modal';
import Select from '../../components/ui/Select';
import Table from '../../components/ui/Table';
import type { Commune, Department } from '../../types/referentials';

export default function CommunesPage() {
  const [items, setItems] = useState<Commune[]>([]);
  const [departments, setDepartments] = useState<Department[]>([]);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<Commune | null>(null);
  const [form, setForm] = useState({ name: '', code: '', departmentId: '' });

  async function load() {
    setLoading(true);
    setError('');
    try {
      const [communesResult, departmentsResult] = await Promise.all([getCommunes({ search: search || undefined }), getDepartments()]);
      communesResult.success ? setItems(communesResult.data ?? []) : setError(communesResult.message);
      if (departmentsResult.success) {
        setDepartments(departmentsResult.data ?? []);
      }
    } catch {
      setError('Impossible de charger les communes.');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  function openCreate() {
    setEditing(null);
    setForm({ name: '', code: '', departmentId: departments[0]?.id ?? '' });
    setModalOpen(true);
  }

  function openEdit(item: Commune) {
    setEditing(item);
    setForm({ name: item.name, code: item.code ?? '', departmentId: item.departmentId });
    setModalOpen(true);
  }

  async function submit(event: FormEvent) {
    event.preventDefault();
    const result = editing ? await updateCommune(editing.id, form) : await createCommune(form);
    if (!result.success) {
      setError(result.message);
      return;
    }

    setModalOpen(false);
    await load();
  }

  async function toggle(item: Commune) {
    const result = item.isActive ? await deactivateCommune(item.id) : await activateCommune(item.id);
    result.success ? await load() : setError(result.message);
  }

  return (
    <div className="grid gap-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-slate-950">Communes</h1>
          <p className="mt-1 text-sm text-slate-500">Référentiel communal</p>
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
            { header: 'Département', cell: (item) => item.departmentName },
            { header: 'Code', cell: (item) => item.code ?? '-' },
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
      <Modal isOpen={modalOpen} onClose={() => setModalOpen(false)} title={editing ? 'Modifier la commune' : 'Créer une commune'}>
        <form className="grid gap-4" onSubmit={submit}>
          <Input label="Nom" onChange={(event) => setForm((current) => ({ ...current, name: event.target.value }))} required value={form.name} />
          <Input label="Code" onChange={(event) => setForm((current) => ({ ...current, code: event.target.value }))} value={form.code} />
          <Select label="Département" onChange={(event) => setForm((current) => ({ ...current, departmentId: event.target.value }))} required value={form.departmentId}>
            <option value="">Choisir</option>
            {departments.map((department) => <option key={department.id} value={department.id}>{department.name}</option>)}
          </Select>
          <Button type="submit">{editing ? 'Enregistrer' : 'Créer'}</Button>
        </form>
      </Modal>
    </div>
  );
}
