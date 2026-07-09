import type { Gender } from './birthRecord';

export type CertificateRequestStatus =
  | 'Pending'
  | 'InProgress'
  | 'CertificateCreated'
  | 'Rejected'
  | 'Cancelled';

export interface CertificateRequest {
  id: string;
  requestNumber: string;
  requestedByUserId: string;
  requestedByFullName: string;
  targetFirstName: string;
  targetLastName: string;
  targetGender: Gender;
  targetBirthDate: string;
  motherFullName: string;
  fatherFullName?: string | null;
  birthPlace: string;
  hospitalFileNumber?: string | null;
  relationshipToTarget: string;
  status: CertificateRequestStatus;
  rejectionReason?: string | null;
  linkedBirthRecordId?: string | null;
  linkedBirthRecordSgcnId?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateCertificateRequestRequest {
  targetFirstName: string;
  targetLastName: string;
  targetGender: Gender;
  targetBirthDate: string;
  motherFullName: string;
  fatherFullName?: string;
  birthPlace: string;
  hospitalFileNumber?: string;
  relationshipToTarget: string;
}
