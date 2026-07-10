import axios from 'axios';
import { axiosClient } from './axiosClient';
import type { ApiResponse } from '../types/apiResponse';
import type { Certificate, CertificatePreview } from '../types/certificate';

export async function getCertificates(): Promise<ApiResponse<Certificate[]>> {
  const response = await axiosClient.get<ApiResponse<Certificate[]>>('/certificates');
  return response.data;
}

export type CertificatePdfDownload = {
  blob: Blob;
  fileName: string;
};

export async function downloadCertificatePdf(id: string): Promise<CertificatePdfDownload> {
  try {
    const response = await axiosClient.get<Blob>(`/certificates/${id}/pdf`, {
      responseType: 'blob'
    });
    const contentDisposition = response.headers['content-disposition'] as string | undefined;

    return {
      blob: response.data,
      fileName: getDownloadFileName(contentDisposition, `certificat-${id}.pdf`)
    };
  } catch (error) {
    if (axios.isAxiosError(error) && error.response?.data instanceof Blob) {
      try {
        error.response.data = JSON.parse(await error.response.data.text());
      } catch {
        // Keep the original response so the shared error helper can use its fallback message.
      }
    }

    throw error;
  }
}

function getDownloadFileName(contentDisposition: string | undefined, fallback: string): string {
  if (!contentDisposition) {
    return fallback;
  }

  const utf8Match = contentDisposition.match(/filename\*=UTF-8''([^;]+)/i);
  if (utf8Match?.[1]) {
    try {
      return decodeURIComponent(utf8Match[1].replace(/^"|"$/g, ''));
    } catch {
      return fallback;
    }
  }

  const fileNameMatch = contentDisposition.match(/filename="?([^";]+)"?/i);
  return fileNameMatch?.[1] ?? fallback;
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
