export interface User {
  id: string;
  fullName: string;
  email: string;
  userName: string;
  phoneNumber?: string | null;
  nifOrCin?: string | null;
  isActive: boolean;
  forcePasswordChange: boolean;
  roles: string[];
  createdAt: string;
  updatedAt?: string | null;
  temporaryPassword?: string | null;
}

export interface CreateUserRequest {
  fullName: string;
  email: string;
  userName: string;
  phoneNumber?: string;
  nifOrCin?: string;
  role: string;
}

export interface UpdateUserRequest {
  fullName?: string;
  phoneNumber?: string;
  nifOrCin?: string;
  role?: string;
}
