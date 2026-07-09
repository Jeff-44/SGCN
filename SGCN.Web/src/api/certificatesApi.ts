import { axiosClient } from './axiosClient';
import type { ApiResponse } from '../types/apiResponse';
import type { Certificate, CertificatePreview } from '../types/certificate';

export async function getCertificates(): Promise<ApiResponse<Certificate[]>> {
  const response = await axiosClient.get<ApiResponse<Certificate[]>>('/certificates');
  return response.data;
}

export async function previewCertificateFromRequest(requestId: string): Promise<ApiResponse<CertificatePreview>> {
  const response = await axiosClient.get<ApiResponse<CertificatePreview>>(
    `/certificates/preview/from-request/${requestId}`
  );
  return response.data;
}

export async function previewCertificateFromBirthRecord(birthRecordId: string): Promise<ApiResponse<CertificatePreview>> {
  const response = await axiosClient.get<ApiResponse<CertificatePreview>>(
    `/certificates/preview/from-birth-record/${birthRecordId}`
  );
  return response.data;
}

export async function generateCertificateFromRequest(requestId: string): Promise<ApiResponse<Certificate>> {
  const response = await axiosClient.post<ApiResponse<Certificate>>(
    `/certificates/generate/from-request/${requestId}`
  );
  return response.data;
}

export async function generateCertificateFromBirthRecord(birthRecordId: string): Promise<ApiResponse<Certificate>> {
  const response = await axiosClient.post<ApiResponse<Certificate>>(
    `/certificates/generate/from-birth-record/${birthRecordId}`
  );
  return response.data;
}

export async function annulCertificate(id: string, annulledReason: string): Promise<ApiResponse<Certificate>> {
  const response = await axiosClient.patch<ApiResponse<Certificate>>(`/certificates/${id}/annul`, {
    annulledReason
  });
  return response.data;
}
