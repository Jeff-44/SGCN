export interface Department {
  id: string;
  name: string;
  code?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface Commune {
  id: string;
  name: string;
  code?: string | null;
  departmentId: string;
  departmentName: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface Hospital {
  id: string;
  name: string;
  code?: string | null;
  communeId: string;
  communeName: string;
  departmentId: string;
  departmentName: string;
  address?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateDepartmentRequest {
  name: string;
  code?: string;
}

export interface CreateCommuneRequest {
  name: string;
  code?: string;
  departmentId: string;
}

export interface CreateHospitalRequest {
  name: string;
  code?: string;
  communeId: string;
  address?: string;
}
