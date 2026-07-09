import axios from 'axios';

type ApiErrorResponse = {
  success?: boolean;
  message?: string;
  errors?: string[] | Record<string, string[]>;
};

export function getApiErrorMessage(error: unknown, fallbackMessage: string): string {
  if (!axios.isAxiosError(error)) {
    return fallbackMessage;
  }

  let data = error.response?.data as ApiErrorResponse | string | undefined;

  if (typeof data === 'string') {
    try {
      data = JSON.parse(data) as ApiErrorResponse;
    } catch {
      return typeof data === 'string' && data.trim() ? data : fallbackMessage;
    }
  }

  if (data?.message) {
    return data.message;
  }

  if (Array.isArray(data?.errors) && data.errors.length > 0) {
    return data.errors.join('\n');
  }

  if (data?.errors && typeof data.errors === 'object') {
    return Object.values(data.errors).flat().join('\n');
  }

  return fallbackMessage;
}