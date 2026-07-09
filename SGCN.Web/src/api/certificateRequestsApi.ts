import { axiosClient } from './axiosClient';
import type { ApiResponse } from '../types/apiResponse';
import type { BirthRecord } from '../types/birthRecord';
import type { CertificateRequest, CreateCertificateRequestRequest } from '../types/certificateRequest';

export async function getCertificateRequests(): Promise<ApiResponse<CertificateRequest[]>> {
  const response = await axiosClient.get<ApiResponse<CertificateRequest[]>>('/certificate-requests');
  return response.data;
}

export async function createCertificateRequest(
  request: CreateCertificateRequestRequest
): Promise<ApiResponse<CertificateRequest>> {
  const response = await axiosClient.post<ApiResponse<CertificateRequest>>('/certificate-requests', request);
  return response.data;
}

export async function cancelCertificateRequest(id: string): Promise<ApiResponse<CertificateRequest>> {
  const response = await axiosClient.patch<ApiResponse<CertificateRequest>>(`/certificate-requests/${id}/cancel`);
  return response.data;
}

export async function rejectCertificateRequest(id: string, rejectionReason: string): Promise<ApiResponse<CertificateRequest>> {
  const response = await axiosClient.patch<ApiResponse<CertificateRequest>>(`/certificate-requests/${id}/reject`, {
    rejectionReason
  });
  return response.data;
}

export async function getMatchingBirthRecords(id: string): Promise<ApiResponse<BirthRecord[]>> {
  const response = await axiosClient.get<ApiResponse<BirthRecord[]>>(`/certificate-requests/${id}/matching-birth-records`);
  return response.data;
}

export async function linkBirthRecord(id: string, birthRecordId: string): Promise<ApiResponse<CertificateRequest>> {
  const response = await axiosClient.patch<ApiResponse<CertificateRequest>>(
    `/certificate-requests/${id}/link-birth-record/${birthRecordId}`
  );
  return response.data;
}
