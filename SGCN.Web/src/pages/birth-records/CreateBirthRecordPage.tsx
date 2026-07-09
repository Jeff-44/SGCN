import { FormEvent, useEffect, useState } from 'react';
import { Save } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { createBirthRecord } from '../../api/birthRecordsApi';
import { getHospitals } from '../../api/referentialsApi';
import Button from '../../components/ui/Button';
import Card from '../../components/ui/Card';
import Input from '../../components/ui/Input';
import Loader from '../../components/ui/Loader';
import Select from '../../components/ui/Select';
import type { CreateBirthRecordRequest, Gender } from '../../types/birthRecord';
import type { Hospital } from '../../types/referentials';

const initialForm: CreateBirthRecordRequest = {
  childFirstName: '',
  childLastName: '',
  gender: 'Male',
  birthDate: '',
  birthTime: '',
  birthPlace: '',
  hospitalId: '',
  motherFullName: '',
  fatherFullName: '',
  hospitalFileNumber: ''
};

export default function CreateBirthRecordPage() {
  const navigate = useNavigate();
  const [form, setForm] = useState<CreateBirthRecordRequest>(initialForm);
  const [hospitals, setHospitals] = useState<Hospital[]>([]);
  const [loadingHospitals, setLoadingHospitals] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    async function loadHospitals() {
      setLoadingHospitals(true);
      try {
        const result = await getHospitals({ isActive: true });
        if (result.success) {
          const data = result.data ?? [];
          setHospitals(data);
          setForm((current) => ({ ...current, hospitalId: current.hospitalId || data[0]?.id || '' }));
          return;
        }

        setError(result.message);
      } catch {
        setError('Impossible de charger les hôpitaux.');
      } finally {
        setLoadingHospitals(false);
      }
    }

    void loadHospitals();
  }, []);

  function updateField<K extends keyof CreateBirthRecordRequest>(key: K, value: CreateBirthRecordRequest[K]) {
    setForm((current) => ({ ...current, [key]: value }));
  }

  async function submit(event: FormEvent) {
    event.preventDefault();
    setSaving(true);
    setError('');

    try {
      const result = await createBirthRecord(form);
      if (!result.success) {
        setError(result.message);
        return;
      }

      navigate('/birth-records');
    } catch {
      setError("Impossible d'enregistrer l'acte de naissance.");
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="grid gap-5">
      <div>
        <h1 className="text-2xl font-bold text-slate-950">Créer un acte de naissance</h1>
        <p className="mt-1 text-sm text-slate-500">Saisie initiale par le médecin</p>
      </div>

      {error ? <p className="rounded-md bg-rose-50 px-3 py-2 text-sm text-rose-700">{error}</p> : null}

      <Card className="p-5">
        {loadingHospitals ? (
          <Loader />
        ) : (
          <form className="grid gap-5" onSubmit={submit}>
            <div className="grid gap-4 md:grid-cols-2">
              <Input
                label="Prénom de l'enfant"
                onChange={(event) => updateField('childFirstName', event.target.value)}
                required
                value={form.childFirstName}
              />
              <Input
                label="Nom de l'enfant"
                onChange={(event) => updateField('childLastName', event.target.value)}
                required
                value={form.childLastName}
              />
              <Select
                label="Sexe"
                onChange={(event) => updateField('gender', event.target.value as Gender)}
                required
                value={form.gender}
              >
                <option value="Male">Masculin</option>
                <option value="Female">Féminin</option>
              </Select>
              <Input
                label="Date de naissance"
                onChange={(event) => updateField('birthDate', event.target.value)}
                required
                type="date"
                value={form.birthDate}
              />
              <Input
                label="Heure de naissance"
                onChange={(event) => updateField('birthTime', event.target.value)}
                required
                type="time"
                value={form.birthTime}
              />
              <Input
                label="Lieu de naissance"
                onChange={(event) => updateField('birthPlace', event.target.value)}
                required
                value={form.birthPlace}
              />
              <Select
                label="Hôpital"
                onChange={(event) => updateField('hospitalId', event.target.value)}
                required
                value={form.hospitalId}
              >
                <option value="">Choisir</option>
                {hospitals.map((hospital) => (
                  <option key={hospital.id} value={hospital.id}>
                    {hospital.name} - {hospital.communeName}
                  </option>
                ))}
              </Select>
              <Input
                label="Numéro de dossier hospitalier"
                onChange={(event) => updateField('hospitalFileNumber', event.target.value)}
                value={form.hospitalFileNumber}
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
            </div>

            <div className="flex justify-end">
              <Button disabled={saving} icon={<Save size={18} />} type="submit">
                {saving ? 'Enregistrement...' : 'Enregistrer'}
              </Button>
            </div>
          </form>
        )}
      </Card>
    </div>
  );
}
