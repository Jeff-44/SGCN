import { BadgeCheck, ClipboardList, FileText, HeartPulse } from 'lucide-react';
import Card from '../../components/ui/Card';

const cards = [
  { label: 'Birth records', value: '-', icon: HeartPulse },
  { label: 'Certificate requests', value: '-', icon: ClipboardList },
  { label: 'Certificates', value: '-', icon: BadgeCheck },
  { label: 'Active workflow', value: '-', icon: FileText }
];

export default function DashboardPage() {
  return (
    <div className="grid gap-6">
      <div>
        <h1 className="text-2xl font-bold text-slate-950">Dashboard</h1>
        <p className="mt-1 text-sm text-slate-500">Vue opérationnelle SGCN</p>
      </div>
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {cards.map((card) => {
          const Icon = card.icon;
          return (
            <Card className="p-5" key={card.label}>
              <div className="flex items-center justify-between gap-4">
                <div>
                  <p className="text-sm text-slate-500">{card.label}</p>
                  <p className="mt-2 text-3xl font-semibold text-slate-950">{card.value}</p>
                </div>
                <div className="grid h-11 w-11 place-items-center rounded-md bg-slate-100 text-slate-700">
                  <Icon size={22} />
                </div>
              </div>
            </Card>
          );
        })}
      </div>
      <Card className="p-5">
        <h2 className="text-base font-semibold text-slate-950">Activité</h2>
        <p className="mt-2 text-sm text-slate-500">Les indicateurs seront connectés quand les endpoints statistiques seront disponibles.</p>
      </Card>
    </div>
  );
}
