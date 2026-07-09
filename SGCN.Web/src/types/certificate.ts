import type { Gender } from './birthRecord';

export type CertificateStatus = 'Active' | 'Annulled';

export interface CertificatePreview {
  childFirstName: string;
  childLastName: string;
  gender: Gender;
  birthDate: string;
  birthTime: string;
  birthPlace: string;
  hospitalName: string;
  communeName: string;
  departmentName: string;
  motherFullName: string;
  fatherFullName?: string | null;
  sgcnId: string;
  requestNumber?: string | null;
}

export interface Certificate {
  id: string;
  certificateNumber: string;
  certificateRequestId?: string | null;
  requestNumber?: string | null;
  birthRecordId: string;
  sgcnId: string;
  childFirstName: string;
  childLastName: string;
  gender: Gender;
  birthDate: string;
  birthTime: string;
  birthPlace: string;
  hospitalName: string;
  communeName: string;
  departmentName: string;
  motherFullName: string;
  fatherFullName?: string | null;
  createdByUserId: string;
  createdByFullName: string;
  status: CertificateStatus;
  verificationCode: string;
  pdfPath?: string | null;
  qrCodePath?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  annulledAt?: string | null;
  annulledReason?: string | null;
}
