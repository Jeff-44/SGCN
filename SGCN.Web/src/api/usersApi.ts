import { axiosClient } from './axiosClient';
import type { ApiResponse } from '../types/apiResponse';
import type { CreateUserRequest, UpdateUserRequest, User } from '../types/user';

export async function getUsers(params?: {
  search?: string;
  role?: string;
  isActive?: boolean;
}): Promise<ApiResponse<User[]>> {
  const response = await axiosClient.get<ApiResponse<User[]>>('/users', { params });
  return response.data;
}

export async function createUser(request: CreateUserRequest): Promise<ApiResponse<User>> {
  const response = await axiosClient.post<ApiResponse<User>>('/users', request);
  return response.data;
}

export async function updateUser(id: string, request: UpdateUserRequest): Promise<ApiResponse<User>> {
  const response = await axiosClient.patch<ApiResponse<User>>(`/users/${id}`, request);
  return response.data;
}

export async function activateUser(id: string): Promise<ApiResponse<User>> {
  const response = await axiosClient.patch<ApiResponse<User>>(`/users/${id}/activate`);
  return response.data;
}

export async function deactivateUser(id: string): Promise<ApiResponse<User>> {
  const response = await axiosClient.patch<ApiResponse<User>>(`/users/${id}/deactivate`);
  return response.data;
}
