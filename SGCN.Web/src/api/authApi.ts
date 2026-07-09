import { axiosClient } from './axiosClient';
import type { ApiResponse } from '../types/apiResponse';
import type { AuthResponse, LoginRequest, RegisterRequest } from '../types/auth';

export async function login(request: LoginRequest): Promise<ApiResponse<AuthResponse>> {
  const response = await axiosClient.post<ApiResponse<AuthResponse>>('/auth/login', request);
  return response.data;
}

export async function register(request: RegisterRequest): Promise<ApiResponse<AuthResponse>> {
  const response = await axiosClient.post<ApiResponse<AuthResponse>>('/auth/register', request);
  return response.data;
}

export async function changePassword(request: {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}): Promise<ApiResponse<AuthResponse>> {
  const response = await axiosClient.post<ApiResponse<AuthResponse>>('/auth/change-password', request);
  return response.data;
}

export async function forgotPassword(email: string): Promise<ApiResponse<string>> {
  const response = await axiosClient.post<ApiResponse<string>>('/auth/forgot-password', { email });
  return response.data;
}
