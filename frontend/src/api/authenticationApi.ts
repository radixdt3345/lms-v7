import axios from 'axios';
import type { ApiResponse } from './types';

// DTOs

export interface TokenResponse {
  accessToken: string;
  tokenType: string;
  expiresIn: number;
}

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface LockedUserDto {
  id: string;
  name: string;
  email: string;
  role: string;
  failedAttempts: number;
  lockedAt: string;
}

// Constants

const BASE = '/api/v1';

// Auth endpoints

/** Returns the SSO redirect URL - navigate to this to start Azure AD flow. */
export const getSsoLoginUrl = (): string => `${BASE}/auth/sso/login`;

/** POST /api/v1/auth/login - BCrypt email/password login. */
export const loginWithCredentials = async (
  credentials: LoginCredentials
): Promise<TokenResponse> => {
  const response = await axios.post<ApiResponse<TokenResponse>>(
    `${BASE}/auth/login`,
    credentials
  );
  return response.data.data; // ApiResponse<T>.data
};

/** POST /api/v1/auth/refresh - rotate refresh token (reads HttpOnly cookie). */
export const refreshToken = async (): Promise<TokenResponse> => {
  const response = await axios.post<ApiResponse<TokenResponse>>(
    `${BASE}/auth/refresh`
  );
  return response.data.data;
};

/** POST /api/v1/auth/logout - revoke refresh token. */
export const logout = async (): Promise<void> => {
  await axios.post(`${BASE}/auth/logout`);
};

// Account management endpoints

/** GET /api/v1/accounts/locked - list locked users (HR_ADMIN / SUPER_ADMIN). */
export const getLockedAccounts = async (): Promise<LockedUserDto[]> => {
  const response = await axios.get<ApiResponse<LockedUserDto[]>>(
    `${BASE}/accounts/locked`
  );
  return response.data.data;
};

/** POST /api/v1/accounts/:id/unlock - unlock a locked user. */
export const unlockAccount = async (userId: string): Promise<boolean> => {
  const response = await axios.post<ApiResponse<boolean>>(
    `${BASE}/accounts/${userId}/unlock`
  );
  return response.data.data;
};
