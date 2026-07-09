import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis
} from 'recharts';
import Card from '../ui/Card';
import type { DashboardChart } from '../../types/dashboard';

type DashboardChartCardProps = {
  chart: DashboardChart;
};

const colors = ['#0284c7', '#059669', '#d97706', '#dc2626', '#0891b2', '#7c3aed'];

const numberFormatter = new Intl.NumberFormat('fr-HT');

export default function DashboardChartCard({ chart }: DashboardChartCardProps) {
  const total = chart.items.reduce((sum, item) => sum + item.value, 0);

  return (
    <Card className="p-5">
      <div className="mb-4 flex items-start justify-between gap-4">
        <h2 className="text-base font-semibold text-slate-950">{chart.title}</h2>
        <span className="rounded-md bg-slate-100 px-2 py-1 text-xs font-medium text-slate-600">
          {numberFormatter.format(total)}
        </span>
      </div>

      {total === 0 ? (
        <div className="grid h-64 place-items-center rounded-md border border-dashed border-slate-200 text-sm text-slate-500">
          Aucune donnée disponible.
        </div>
      ) : chart.type === 'donut' ? (
        <DonutChart chart={chart} />
      ) : chart.type === 'summary' ? (
        <CertificateSummary chart={chart} />
      ) : (
        <HorizontalBarChart chart={chart} />
      )}
    </Card>
  );
}

function HorizontalBarChart({ chart }: DashboardChartCardProps) {
  return (
    <div className="h-64">
      <ResponsiveContainer height="100%" width="100%">
        <BarChart data={chart.items} layout="vertical" margin={{ bottom: 4, left: 8, right: 16, top: 4 }}>
          <CartesianGrid horizontal={false} stroke="#e2e8f0" />
          <XAxis allowDecimals={false} axisLine={false} tickLine={false} type="number" />
          <YAxis
            axisLine={false}
            dataKey="label"
            tick={{ fill: '#475569', fontSize: 12 }}
            tickLine={false}
            type="category"
            width={130}
          />
          <Tooltip formatter={(value) => numberFormatter.format(Number(value))} />
          <Bar dataKey="value" fill="#0284c7" isAnimationActive={false} radius={[0, 4, 4, 0]} />
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}

function DonutChart({ chart }: DashboardChartCardProps) {
  return (
    <div className="grid gap-4 md:grid-cols-[minmax(0,1fr)_12rem] md:items-center">
      <div className="h-64">
        <ResponsiveContainer height="100%" width="100%">
          <PieChart>
            <Pie
              cx="50%"
              cy="50%"
              data={chart.items}
              dataKey="value"
              innerRadius="55%"
              isAnimationActive={false}
              nameKey="label"
              outerRadius="80%"
              paddingAngle={2}
            >
              {chart.items.map((item, index) => (
                <Cell fill={colors[index % colors.length]} key={item.label} />
              ))}
            </Pie>
            <Tooltip formatter={(value) => numberFormatter.format(Number(value))} />
          </PieChart>
        </ResponsiveContainer>
      </div>

      <ChartLegend chart={chart} />
    </div>
  );
}

function CertificateSummary({ chart }: DashboardChartCardProps) {
  const available = chart.items.find((item) => item.label === 'Available')?.value ?? 0;
  const total = chart.items.reduce((sum, item) => sum + item.value, 0);

  return (
    <div className="grid min-h-64 content-center gap-5">
      <div>
        <p className="text-sm text-slate-500">Available certificates</p>
        <p className="mt-2 text-4xl font-semibold text-slate-950">{numberFormatter.format(available)}</p>
      </div>

      <div className="grid gap-3">
        {chart.items.map((item, index) => {
          const percentage = total > 0 ? Math.round((item.value / total) * 100) : 0;

          return (
            <div className="grid gap-1" key={item.label}>
              <div className="flex items-center justify-between gap-3 text-sm">
                <span className="font-medium text-slate-700">{item.label}</span>
                <span className="text-slate-500">{numberFormatter.format(item.value)}</span>
              </div>
              <div className="h-2 overflow-hidden rounded-full bg-slate-100">
                <div
                  className="h-full rounded-full"
                  style={{ backgroundColor: colors[index % colors.length], width: `${percentage}%` }}
                />
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

function ChartLegend({ chart }: DashboardChartCardProps) {
  return (
    <div className="grid gap-3">
      {chart.items.map((item, index) => (
        <div className="flex items-center justify-between gap-3 text-sm" key={item.label}>
          <div className="flex min-w-0 items-center gap-2">
            <span
              className="h-3 w-3 shrink-0 rounded-sm"
              style={{ backgroundColor: colors[index % colors.length] }}
            />
            <span className="truncate text-slate-700">{item.label}</span>
          </div>
          <span className="font-medium text-slate-950">{numberFormatter.format(item.value)}</span>
        </div>
      ))}
    </div>
  );
}
