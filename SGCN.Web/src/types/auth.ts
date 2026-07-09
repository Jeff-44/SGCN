export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  nifOrCin?: string;
  phoneNumber?: string;
}

export interface AuthResponse {
  accessToken: string;
  expiresAt: string;
  userId: string;
  email: string;
  fullName: string;
  forcePasswordChange: boolean;
  roles: string[];
}

export interface AuthUser {
  userId: string;
  email: string;
  fullName: string;
  forcePasswordChange: boolean;
  roles: string[];
}
