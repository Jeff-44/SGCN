import { axiosClient } from './axiosClient';
import type { ApiResponse } from '../types/apiResponse';
import type {
  Commune,
  CreateCommuneRequest,
  CreateDepartmentRequest,
  CreateHospitalRequest,
  Department,
  Hospital
} from '../types/referentials';

export async function getDepartments(params?: {
  search?: string;
  isActive?: boolean;
}): Promise<ApiResponse<Department[]>> {
  const response = await axiosClient.get<ApiResponse<Department[]>>('/departments', { params });
  return response.data;
}

export async function createDepartment(request: CreateDepartmentRequest): Promise<ApiResponse<Department>> {
  const response = await axiosClient.post<ApiResponse<Department>>('/departments', request);
  return response.data;
}

export async function updateDepartment(id: string, request: Partial<CreateDepartmentRequest>): Promise<ApiResponse<Department>> {
  const response = await axiosClient.patch<ApiResponse<Department>>(`/departments/${id}`, request);
  return response.data;
}

export async function activateDepartment(id: string): Promise<ApiResponse<Department>> {
  const response = await axiosClient.patch<ApiResponse<Department>>(`/departments/${id}/activate`);
  return response.data;
}

export async function deactivateDepartment(id: string): Promise<ApiResponse<Department>> {
  const response = await axiosClient.patch<ApiResponse<Department>>(`/departments/${id}/deactivate`);
  return response.data;
}

export async function getCommunes(params?: {
  search?: string;
  departmentId?: string;
  isActive?: boolean;
}): Promise<ApiResponse<Commune[]>> {
  const response = await axiosClient.get<ApiResponse<Commune[]>>('/communes', { params });
  return response.data;
}

export async function createCommune(request: CreateCommuneRequest): Promise<ApiResponse<Commune>> {
  const response = await axiosClient.post<ApiResponse<Commune>>('/communes', request);
  return response.data;
}

export async function updateCommune(id: string, request: Partial<CreateCommuneRequest>): Promise<ApiResponse<Commune>> {
  const response = await axiosClient.patch<ApiResponse<Commune>>(`/communes/${id}`, request);
  return response.data;
}

export async function activateCommune(id: string): Promise<ApiResponse<Commune>> {
  const response = await axiosClient.patch<ApiResponse<Commune>>(`/communes/${id}/activate`);
  return response.data;
}

export async function deactivateCommune(id: string): Promise<ApiResponse<Commune>> {
  const response = await axiosClient.patch<ApiResponse<Commune>>(`/communes/${id}/deactivate`);
  return response.data;
}

export async function getHospitals(params?: {
  search?: string;
  communeId?: string;
  departmentId?: string;
  isActive?: boolean;
}): Promise<ApiResponse<Hospital[]>> {
  const response = await axiosClient.get<ApiResponse<Hospital[]>>('/hospitals', { params });
  return response.data;
}

export async function createHospital(request: CreateHospitalRequest): Promise<ApiResponse<Hospital>> {
  const response = await axiosClient.post<ApiResponse<Hospital>>('/hospitals', request);
  return response.data;
}

export async function updateHospital(id: string, request: Partial<CreateHospitalRequest>): Promise<ApiResponse<Hospital>> {
  const response = await axiosClient.patch<ApiResponse<Hospital>>(`/hospitals/${id}`, request);
  return response.data;
}

export async function activateHospital(id: string): Promise<ApiResponse<Hospital>> {
  const response = await axiosClient.patch<ApiResponse<Hospital>>(`/hospitals/${id}/activate`);
  return response.data;
}

export async function deactivateHospital(id: string): Promise<ApiResponse<Hospital>> {
  const response = await axiosClient.patch<ApiResponse<Hospital>>(`/hospitals/${id}/deactivate`);
  return response.data;
}
