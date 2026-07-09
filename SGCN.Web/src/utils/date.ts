export function formatDate(value?: string | null): string {
  if (!value) {
    return '-';
  }

  const normalized = /^\d{4}-\d{2}-\d{2}$/.test(value) ? `${value}T00:00:00` : value;

  return new Intl.DateTimeFormat('fr-HT', {
    year: 'numeric',
    month: 'short',
    day: '2-digit'
  }).format(new Date(normalized));
}

export function formatDateTime(value?: string | null): string {
  if (!value) {
    return '-';
  }

  return new Intl.DateTimeFormat('fr-HT', {
    year: 'numeric',
    month: 'short',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  }).format(new Date(value));
}
