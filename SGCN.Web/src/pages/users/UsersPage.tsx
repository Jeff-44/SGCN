import { FormEvent, useEffect, useState } from 'react';
import { Copy, Edit, Plus, RotateCw, Search } from 'lucide-react';
import { activateUser, createUser, deactivateUser, getUsers, updateUser } from '../../api/usersApi';
import Badge from '../../components/ui/Badge';
import Button from '../../components/ui/Button';
import Card from '../../components/ui/Card';
import Input from '../../components/ui/Input';
import Loader from '../../components/ui/Loader';
import Modal from '../../components/ui/Modal';
import Select from '../../components/ui/Select';
import Table from '../../components/ui/Table';
import type { CreateUserRequest, UpdateUserRequest, User } from '../../types/user';
import { formatDate } from '../../utils/date';

const initialForm: CreateUserRequest = {
  fullName: '',
  email: '',
  userName: '',
  role: 'AgentEtatCivil',
  phoneNumber: '',
  nifOrCin: ''
};

export default function UsersPage() {
  const [users, setUsers] = useState<User[]>([]);
  const [search, setSearch] = useState('');
  const [role, setRole] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<User | null>(null);
  const [form, setForm] = useState<CreateUserRequest>(initialForm);
  const [createdPassword, setCreatedPassword] = useState<{
    fullName: string;
    temporaryPassword: string;
  } | null>(null);
  const [copyStatus, setCopyStatus] = useState('');

  async function load() {
    setLoading(true);
    setError('');
    try {
      const result = await getUsers({ search: search || undefined, role: role || undefined });
      result.success ? setUsers(result.data ?? []) : setError(result.message);
    } catch {
      setError('Impossible de charger les utilisateurs.');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  function updateForm(name: keyof CreateUserRequest, value: string) {
    setForm((current) => ({ ...current, [name]: value }));
  }

  function openCreate() {
    setEditing(null);
    setForm(initialForm);
    setError('');
    setSuccess('');
    setModalOpen(true);
  }

  function openEdit(user: User) {
    setEditing(user);
    setForm({
      fullName: user.fullName,
      email: user.email,
      userName: user.userName,
      role: user.roles[0] ?? 'AgentEtatCivil',
      phoneNumber: user.phoneNumber ?? '',
      nifOrCin: user.nifOrCin ?? ''
    });
    setError('');
    setSuccess('');
    setModalOpen(true);
  }

  async function submit(event: FormEvent) {
    event.preventDefault();
    setError('');
    setSuccess('');

    const updateRequest: UpdateUserRequest = {
      fullName: form.fullName,
      phoneNumber: form.phoneNumber,
      nifOrCin: form.nifOrCin,
      role: form.role
    };

    const result = editing
      ? await updateUser(editing.id, updateRequest)
      : await createUser(form);

    if (!result.success) {
      setError(result.message);
      return;
    }

    setModalOpen(false);
    setForm(initialForm);
    setEditing(null);

    if (!editing && result.data?.temporaryPassword) {
      setCreatedPassword({
        fullName: result.data.fullName,
        temporaryPassword: result.data.temporaryPassword
      });
    } else {
      setSuccess(editing ? 'Utilisateur modifié.' : 'Utilisateur créé.');
    }

    await load();
  }

  async function toggle(user: User) {
    const result = user.isActive ? await deactivateUser(user.id) : await activateUser(user.id);
    result.success ? await load() : setError(result.message);
  }

  async function copyTemporaryPassword() {
    if (!createdPassword) {
      return;
    }

    try {
      await navigator.clipboard.writeText(createdPassword.temporaryPassword);
      setCopyStatus('Mot de passe copié.');
    } catch {
      setCopyStatus('Copie impossible.');
    }
  }

  return (
    <div className="grid gap-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-slate-950">Users</h1>
          <p className="mt-1 text-sm text-slate-500">Gestion des comptes opérationnels</p>
        </div>
        <Button icon={<Plus size={18} />} onClick={openCreate}>
          Nouvel utilisateur
        </Button>
      </div>

      <Card className="p-4">
        <div className="grid gap-3 md:grid-cols-[1fr_220px_auto]">
          <Input label="Recherche" onChange={(event) => setSearch(event.target.value)} placeholder="Nom, email..." value={search} />
          <Select label="Rôle" onChange={(event) => setRole(event.target.value)} value={role}>
            <option value="">Tous</option>
            <option value="Administrateur">Administrateur</option>
            <option value="AgentEtatCivil">AgentEtatCivil</option>
            <option value="Medecin">Medecin</option>
          </Select>
          <div className="flex items-end">
            <Button className="w-full md:w-auto" icon={<Search size={18} />} onClick={load} variant="secondary">
              Rechercher
            </Button>
          </div>
        </div>
      </Card>

      {error ? <p className="rounded-md bg-rose-50 px-3 py-2 text-sm text-rose-700">{error}</p> : null}
      {success ? <p className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{success}</p> : null}
      <Card>{loading ? <div className="p-5"><Loader /></div> : (
        <Table
          columns={[
            { header: 'Nom', cell: (user) => <span className="font-medium text-slate-900">{user.fullName}</span> },
            { header: 'Email', cell: (user) => user.email },
            { header: 'Rôles', cell: (user) => user.roles.join(', ') || '-' },
            { header: 'Statut', cell: (user) => <Badge tone={user.isActive ? 'success' : 'muted'}>{user.isActive ? 'Actif' : 'Inactif'}</Badge> },
            { header: 'Créé', cell: (user) => formatDate(user.createdAt) },
            {
              header: 'Actions',
              cell: (user) => (
                <div className="flex flex-wrap gap-2">
                  <Button icon={<Edit size={16} />} onClick={() => openEdit(user)} variant="secondary">
                    Modifier
                  </Button>
                  <Button icon={<RotateCw size={16} />} onClick={() => toggle(user)} variant="secondary">
                    {user.isActive ? 'Désactiver' : 'Activer'}
                  </Button>
                </div>
              )
            }
          ]}
          data={users}
          keyExtractor={(user) => user.id}
        />
      )}</Card>

      <Modal isOpen={modalOpen} onClose={() => setModalOpen(false)} title={editing ? 'Modifier un utilisateur' : 'Créer un utilisateur'}>
        <form className="grid gap-4 sm:grid-cols-2" onSubmit={submit}>
          <Input label="Nom complet" onChange={(event) => updateForm('fullName', event.target.value)} required value={form.fullName} />
          <Input disabled={Boolean(editing)} label="Email" onChange={(event) => updateForm('email', event.target.value)} required type="email" value={form.email} />
          <Input disabled={Boolean(editing)} label="Nom utilisateur" onChange={(event) => updateForm('userName', event.target.value)} required value={form.userName} />
          <Select label="Rôle" onChange={(event) => updateForm('role', event.target.value)} value={form.role}>
            <option value="AgentEtatCivil">AgentEtatCivil</option>
            <option value="Medecin">Medecin</option>
            <option value="Administrateur">Administrateur</option>
          </Select>
          <Input label="Téléphone" onChange={(event) => updateForm('phoneNumber', event.target.value)} value={form.phoneNumber} />
          <Input label="NIF ou CIN" onChange={(event) => updateForm('nifOrCin', event.target.value)} value={form.nifOrCin} />
          <Button className="sm:col-span-2" type="submit">
            {editing ? 'Enregistrer' : 'Créer'}
          </Button>
        </form>
      </Modal>

      <Modal
        isOpen={Boolean(createdPassword)}
        onClose={() => {
          setCreatedPassword(null);
          setCopyStatus('');
        }}
        title="User created successfully"
      >
        {createdPassword ? (
          <div className="grid gap-4">
            <div>
              <p className="text-sm font-medium text-slate-900">{createdPassword.fullName}</p>
              <p className="mt-2 rounded-md bg-slate-100 px-3 py-2 font-mono text-sm text-slate-950">
                {createdPassword.temporaryPassword}
              </p>
            </div>
            <p className="rounded-md bg-amber-50 px-3 py-2 text-sm text-amber-800">
              This password will only be displayed once. Please communicate it securely to the user.
            </p>
            {copyStatus ? <p className="text-sm text-slate-600">{copyStatus}</p> : null}
            <Button icon={<Copy size={18} />} onClick={copyTemporaryPassword} type="button">
              Copy Password
            </Button>
          </div>
        ) : null}
      </Modal>
    </div>
  );
}
