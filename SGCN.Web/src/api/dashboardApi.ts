import { axiosClient } from './axiosClient';
import type { ApiResponse } from '../types/apiResponse';
import type { Dashboard } from '../types/dashboard';

export async function getDashboard(): Promise<ApiResponse<Dashboard>> {
  const response = await axiosClient.get<ApiResponse<Dashboard>>('/dashboard');
  return response.data;
}
