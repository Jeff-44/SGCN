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
    <div className="fixed inset-0 z-50 grid place-items-center bg-slate-950/35 px-4 py-6">
      <div className="max-h-[90vh] w-full max-w-2xl overflow-y-auto rounded-lg bg-white shadow-soft">
        <div className="flex items-center justify-between border-b border-slate-200 px-5 py-4">
          <h2 className="text-base font-semibold text-slate-900">{title}</h2>
          <Button aria-label="Fermer" className="h-9 w-9 px-0" icon={<X size={18} />} onClick={onClose} variant="ghost" />
        </div>
        <div className="p-5">{children}</div>
      </div>
    </div>
  );
}
