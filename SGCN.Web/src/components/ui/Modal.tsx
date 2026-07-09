import type { ReactNode } from 'react';
import { X } from 'lucide-react';
import Button from './Button';

type ModalProps = {
  children: ReactNode;
  isOpen: boolean;
  onClose: () => void;
  title: string;
};

export default function Modal({ children, isOpen, onClose, title }: ModalProps) {
  if (!isOpen) {
    return null;
  }

  return (
<div
  className="fixed inset-0 z-50 grid place-items-center bg-slate-950/35 px-4 py-6"
  onClick={onClose}
>
  <div
    className="max-h-[90vh] w-full max-w-2xl overflow-y-auto rounded-lg bg-white shadow-soft"
    onClick={(event) => event.stopPropagation()}
  >
        <div className="flex items-center justify-between border-b border-slate-200 px-5 py-4">
          <h2 className="text-base font-semibold text-slate-900">{title}</h2>
          <Button
  aria-label="Fermer"
  className="h-12 w-12 bg-white text-slate-700 hover:bg-slate-100 hover:text-slate-950"
  icon={<X size={30} strokeWidth={3} />}
  onClick={onClose}
  variant="ghost"
/>
        </div>
        <div className="p-5">{children}</div>
      </div>
    </div>
  );
}
