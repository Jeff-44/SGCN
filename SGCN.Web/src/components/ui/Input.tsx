import type { InputHTMLAttributes } from 'react';

type InputProps = InputHTMLAttributes<HTMLInputElement> & {
  label?: string;
};

export default function Input({ className = '', label, id, ...props }: InputProps) {
  const inputId = id ?? props.name;

  return (
    <label className="grid gap-1.5 text-sm text-slate-700" htmlFor={inputId}>
      {label ? <span className="font-medium">{label}</span> : null}
      <input
        className={`min-h-10 rounded-md border border-slate-200 bg-white px-3 py-2 text-sm outline-none ring-slate-900 transition placeholder:text-slate-400 focus:ring-2 ${className}`}
        id={inputId}
        {...props}
      />
    </label>
  );
}
