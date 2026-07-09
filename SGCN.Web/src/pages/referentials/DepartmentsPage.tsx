import { FormEvent, useEffect, useState } from 'react';
import { Edit, Plus, RotateCw, Search } from 'lucide-react';
import {
  activateDepartment,
  createDepartment,
  deactivateDepartment,
  getDepartments,
  updateDepartment
} from '../../api/referentialsApi';
import Badge from '../../components/ui/Badge';
import Button from '../../components/ui/Button';
import Card from '../../components/ui/Card';
import Input from '../../components/ui/Input';
import Loader from '../../components/ui/Loader';
import Modal from '../../components/ui/Modal';
import Table from '../../components/ui/Table';
import type { Department } from '../../types/referentials';

export default function DepartmentsPage() {
  const [items, setItems] = useState<Department[]>([]);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<Department | null>(null);
  const [form, setForm] = useState({ name: '', code: '' });

  async function load() {
    setLoading(true);
    setError('');
    try {
      const result = await getDepartments({ search: search || undefined });
      result.success ? setItems(result.data ?? []) : setError(result.message);
    } catch {
      setError('Impossible de charger les départements.');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  function openCreate() {
    setEditing(null);
    setForm({ name: '', code: '' });
    setModalOpen(true);
  }

  function openEdit(item: Department) {
    setEditing(item);
    setForm({ name: item.name, code: item.code ?? '' });
    setModalOpen(true);
  }

  async function submit(event: FormEvent) {
    event.preventDefault();
    const result = editing
      ? await updateDepartment(editing.id, form)
      : await createDepartment(form);
    if (!result.success) {
      setError(result.message);
      return;
    }

    setModalOpen(false);
    await load();
  }

  async function toggle(item: Department) {
    const result = item.isActive ? await deactivateDepartment(item.id) : await activateDepartment(item.id);
    result.success ? await load() : setError(result.message);
  }

  return (
    <div className="grid gap-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-slate-950">Departments</h1>
          <p className="mt-1 text-sm text-slate-500">Référentiel départemental</p>
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
      <Modal isOpen={modalOpen} onClose={() => setModalOpen(false)} title={editing ? 'Modifier le département' : 'Créer un département'}>
        <form className="grid gap-4" onSubmit={submit}>
          <Input label="Nom" onChange={(event) => setForm((current) => ({ ...current, name: event.target.value }))} required value={form.name} />
          <Input label="Code" onChange={(event) => setForm((current) => ({ ...current, code: event.target.value }))} value={form.code} />
          <Button type="submit">{editing ? 'Enregistrer' : 'Créer'}</Button>
        </form>
      </Modal>
    </div>
  );
}
