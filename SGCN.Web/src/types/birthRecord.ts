export type Gender = 'Male' | 'Female';

export interface BirthRecord {
  id: string;
  sgcnId: string;
  childFirstName: string;
  childLastName: string;
  gender: Gender;
  birthDate: string;
  birthTime: string;
  birthPlace: string;
  hospitalId: string;
  hospitalName: string;
  communeName: string;
  departmentName: string;
  motherFullName: string;
  fatherFullName?: string | null;
  hospitalFileNumber?: string | null;
  createdByUserId: string;
  createdByFullName: string;
  isLocked: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateBirthRecordRequest {
  childFirstName: string;
  childLastName: string;
  gender: Gender;
  birthDate: string;
  birthTime: string;
  birthPlace: string;
  hospitalId: string;
  motherFullName: string;
  fatherFullName?: string;
  hospitalFileNumber?: string;
}
