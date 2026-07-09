import { useEffect, useState } from 'react';
import {
  BadgeCheck,
  ClipboardList,
  Clock3,
  FileCheck,
  FileText,
  HeartPulse,
  Hospital,
  ShieldCheck,
  Stethoscope,
  UserRound,
  Users,
  XCircle
} from 'lucide-react';
import { getDashboard } from '../../api/dashboardApi';
import DashboardChartCard from '../../components/dashboard/DashboardChartCard';
import Badge from '../../components/ui/Badge';
import Card from '../../components/ui/Card';
import Loader from '../../components/ui/Loader';
import { useAuth } from '../../store/authStore';
import type { Dashboard, DashboardRecentItem } from '../../types/dashboard';
import { formatDateTime } from '../../utils/date';
import { primaryRole } from '../../utils/roles';

const metricIcons = {
  totalUsers: Users,
  totalCitizens: UserRound,
  totalDoctors: Stethoscope,
  totalCivilRegistryAgents: ShieldCheck,
  totalHospitals: Hospital,
  totalBirthRecords: HeartPulse,
  pendingCertificateRequests: ClipboardList,
  totalCertificatesIssued: BadgeCheck,
  birthRecordsCreated: HeartPulse,
  approvedCertificateRequests: BadgeCheck,
  rejectedCertificateRequests: XCircle,
  certificatesIssued: FileCheck,
  pendingBirthRecords: Clock3,
  validatedBirthRecords: BadgeCheck,
  rejectedBirthRecords: XCircle,
  myCertificateRequests: ClipboardList,
  pendingRequests: Clock3,
  approvedRequests: BadgeCheck,
  rejectedRequests: XCircle,
  myCertificates: FileCheck
};

const accentClasses = [
  'bg-sky-50 text-sky-700',
  'bg-emerald-50 text-emerald-700',
  'bg-amber-50 text-amber-700',
  'bg-rose-50 text-rose-700',
  'bg-cyan-50 text-cyan-700',
  'bg-violet-50 text-violet-700'
];

const numberFormatter = new Intl.NumberFormat('fr-HT');

function iconForMetric(key: string) {
  return metricIcons[key as keyof typeof metricIcons] ?? FileText;
}

function toneForStatus(status?: string | null) {
  if (!status) {
    return 'default' as const;
  }

  if (['Active', 'Approved', 'CertificateCreated', 'Validated'].includes(status)) {
    return 'success' as const;
  }

  if (['Pending', 'InProgress'].includes(status)) {
    return 'warning' as const;
  }

  if (['Rejected', 'Annulled', 'Inactive'].includes(status)) {
    return 'danger' as const;
  }

  if (status === 'Cancelled') {
    return 'muted' as const;
  }

  return 'default' as const;
}

function itemDetails(item: DashboardRecentItem): string {
  return [item.reference, item.description].filter(Boolean).join(' - ');
}

export default function DashboardPage() {
  const { user } = useAuth();
  const [dashboard, setDashboard] = useState<Dashboard | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  async function load() {
    setLoading(true);
    setError('');

    try {
      const result = await getDashboard();
      result.success ? setDashboard(result.data ?? null) : setError(result.message);
    } catch {
      setError('Impossible de charger le tableau de bord.');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  const roleLabel = dashboard?.role ?? primaryRole(user);

  return (
    <div className="grid gap-6">
      <div>
        <h1 className="text-2xl font-bold text-slate-950">Dashboard</h1>
        <p className="mt-1 text-sm text-slate-500">{roleLabel}</p>
      </div>

      {error ? <p className="rounded-md bg-rose-50 px-3 py-2 text-sm text-rose-700">{error}</p> : null}

      {loading ? (
        <Card className="p-5">
          <Loader />
        </Card>
      ) : (
        <>
          <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
            {(dashboard?.metrics ?? []).map((metric, index) => {
              const Icon = iconForMetric(metric.key);
              const accent = accentClasses[index % accentClasses.length];

              return (
                <Card className="p-5" key={metric.key}>
                  <div className="flex min-h-24 items-start justify-between gap-4">
                    <div className="min-w-0">
                      <p className="break-words text-sm text-slate-500">{metric.label}</p>
                      <p className="mt-2 text-3xl font-semibold text-slate-950">
                        {numberFormatter.format(metric.value)}
                      </p>
                    </div>
                    <div className={`grid h-11 w-11 shrink-0 place-items-center rounded-md ${accent}`}>
                      <Icon size={22} />
                    </div>
                  </div>
                </Card>
              );
            })}
          </div>

          {(dashboard?.charts?.length ?? 0) > 0 ? (
            <div className="grid gap-4 lg:grid-cols-2 2xl:grid-cols-3">
              {dashboard?.charts.map((chart) => (
                <DashboardChartCard chart={chart} key={chart.key} />
              ))}
            </div>
          ) : null}

          {dashboard?.recentItemsTitle ? (
            <Card className="overflow-hidden">
              <div className="border-b border-slate-200 px-5 py-4">
                <h2 className="text-base font-semibold text-slate-950">{dashboard.recentItemsTitle}</h2>
              </div>
              {dashboard.recentItems.length === 0 ? (
                <p className="px-5 py-6 text-sm text-slate-500">Aucune activité récente.</p>
              ) : (
                <ul className="divide-y divide-slate-100">
                  {dashboard.recentItems.map((item) => (
                    <li
                      className="flex flex-col gap-3 px-5 py-4 sm:flex-row sm:items-center sm:justify-between"
                      key={`${item.type}-${item.reference ?? item.title}-${item.createdAt}`}
                    >
                      <div className="min-w-0">
                        <div className="flex flex-wrap items-center gap-2">
                          <p className="font-medium text-slate-900">{item.title}</p>
                          <Badge tone="muted">{item.type}</Badge>
                        </div>
                        <p className="mt-1 break-words text-sm text-slate-500">{itemDetails(item)}</p>
                      </div>
                      <div className="flex shrink-0 flex-wrap items-center gap-3 sm:justify-end">
                        {item.status ? <Badge tone={toneForStatus(item.status)}>{item.status}</Badge> : null}
                        <span className="text-sm text-slate-500">{formatDateTime(item.createdAt)}</span>
                      </div>
                    </li>
                  ))}
                </ul>
              )}
            </Card>
          ) : null}
        </>
      )}
    </div>
  );
}
