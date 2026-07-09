type BadgeTone = 'default' | 'success' | 'warning' | 'danger' | 'muted';

type BadgeProps = {
  children: string | number | boolean;
  tone?: BadgeTone;
};

const tones: Record<BadgeTone, string> = {
  default: 'bg-slate-100 text-slate-700',
  success: 'bg-emerald-50 text-emerald-700 ring-1 ring-emerald-100',
  warning: 'bg-amber-50 text-amber-700 ring-1 ring-amber-100',
  danger: 'bg-rose-50 text-rose-700 ring-1 ring-rose-100',
  muted: 'bg-zinc-100 text-zinc-600'
};

export default function Badge({ children, tone = 'default' }: BadgeProps) {
  return (
    <span className={`inline-flex min-h-6 items-center rounded-md px-2 py-1 text-xs font-medium ${tones[tone]}`}>
      {String(children)}
    </span>
  );
}
