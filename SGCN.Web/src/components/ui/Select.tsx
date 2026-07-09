import type { SelectHTMLAttributes } from 'react';

type SelectProps = SelectHTMLAttributes<HTMLSelectElement> & {
  label?: string;
};

export default function Select({ children, className = '', label, id, ...props }: SelectProps) {
  const selectId = id ?? props.name;

  return (
    <label className="grid gap-1.5 text-sm text-slate-700" htmlFor={selectId}>
      {label ? <span className="font-medium">{label}</span> : null}
      <select
        className={`min-h-10 rounded-md border border-slate-200 bg-white px-3 py-2 text-sm outline-none ring-slate-900 transition focus:ring-2 ${className}`}
        id={selectId}
        {...props}
      >
        {children}
      </select>
    </label>
  );
}
