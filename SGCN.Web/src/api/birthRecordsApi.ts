import { axiosClient } from './axiosClient';
import type { ApiResponse } from '../types/apiResponse';
import type { BirthRecord, CreateBirthRecordRequest } from '../types/birthRecord';

export async function getBirthRecords(params?: {
  search?: string;
  hospitalId?: string;
  isLocked?: boolean;
  isActive?: boolean;
  birthDateFrom?: string;
  birthDateTo?: string;
}): Promise<ApiResponse<BirthRecord[]>> {
  const response = await axiosClient.get<ApiResponse<BirthRecord[]>>('/birth-records', { params });
  return response.data;
}

export async function getBirthRecordsWithoutCertificate(params?: {
  search?: string;
  hospitalId?: string;
  birthDateFrom?: string;
  birthDateTo?: string;
}): Promise<ApiResponse<BirthRecord[]>> {
  // TODO: Rename in UI copy if the backend later exposes /birth-records/pending-certificate.
  const response = await axiosClient.get<ApiResponse<BirthRecord[]>>('/birth-records/without-certificate', { params });
  return response.data;
}

export async function createBirthRecord(request: CreateBirthRecordRequest): Promise<ApiResponse<BirthRecord>> {
  const response = await axiosClient.post<ApiResponse<BirthRecord>>('/birth-records', request);
  return response.data;
}

export async function activateBirthRecord(id: string): Promise<ApiResponse<BirthRecord>> {
  const response = await axiosClient.patch<ApiResponse<BirthRecord>>(`/birth-records/${id}/activate`);
  return response.data;
}

export async function deactivateBirthRecord(id: string): Promise<ApiResponse<BirthRecord>> {
  const response = await axiosClient.patch<ApiResponse<BirthRecord>>(`/birth-records/${id}/deactivate`);
  return response.data;
}
