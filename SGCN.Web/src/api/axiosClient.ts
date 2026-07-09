import axios, { AxiosError } from 'axios';
import { authStore } from '../store/authStore';
import { getToken } from '../utils/token';

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

if (!apiBaseUrl) {
  throw new Error('VITE_API_BASE_URL is not configured.');
}

export const axiosClient = axios.create({
  baseURL: apiBaseUrl,
  headers: {
    'Content-Type': 'application/json'
  },
  timeout: 30000
});

// Request interceptor
axiosClient.interceptors.request.use(
  (config) => {
    const token = getToken();

    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor
axiosClient.interceptors.response.use(
  (response) => response,

  (error: AxiosError) => {
    const status = error.response?.status;

    switch (status) {
      case 401:
        console.warn('401 Unauthorized');

        authStore.logout();

        if (window.location.pathname !== '/login') {
          window.location.replace('/login');
        }
        break;

      case 403:
        console.warn('403 Forbidden');
        break;

      case 500:
        console.error('Internal Server Error');
        break;

      default:
        break;
    }

    return Promise.reject(error);
  }
);