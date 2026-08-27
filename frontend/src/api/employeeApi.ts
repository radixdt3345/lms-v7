import axios from 'axios';
import type { ApiResponse } from './types';

// ─── DTOs ────────────────────────────────────────────────────────────────────

export interface EmployeeDto {
  id: string;
  name: string;
  email: string;
  phone: string | null;
  role: string;
  status: string;
  jobTitle: string | null;
  dateOfJoining: string | null;
  departmentId: string | null;
  departmentName: string | null;
  reportingManagerId: string | null;
  reportingManagerName: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateEmployeeRequest {
  name: string;
  email: string;
  phone?: string | null;
  jobTitle?: string | null;
  dateOfJoining?: string | null;
  departmentId?: string | null;
  reportingManagerId?: string | null;
}

export interface UpdateEmployeeRequest {
  name?: string | null;
  phone?: string | null;
  jobTitle?: string | null;
  dateOfJoining?: string | null;
  departmentId?: string | null;
  clearDepartment?: boolean;
  reportingManagerId?: string | null;
  clearReportingManager?: boolean;
  role?: string | null;
  status?: string | null;
}

export interface SelfEditRequest {
  name: string;
  phone?: string | null;
}

export interface DepartmentDto {
  id: string;
  name: string;
}

// ─── Constants ───────────────────────────────────────────────────────────────

const BASE = '/api/v1/employees';
const DEPT_BASE = '/api/v1/departments';

// ─── Employee endpoints ───────────────────────────────────────────────────────

/** GET /api/v1/employees — HR_ADMIN / SUPER_ADMIN */
export const listEmployees = async (): Promise<EmployeeDto[]> => {
  const response = await axios.get<ApiResponse<EmployeeDto[]>>(BASE);
  return response.data.data;
};

/** GET /api/v1/employees/:id */
export const getEmployee = async (id: string): Promise<EmployeeDto> => {
  const response = await axios.get<ApiResponse<EmployeeDto>>(`${BASE}/${id}`);
  return response.data.data;
};

/** POST /api/v1/employees — HR_ADMIN / SUPER_ADMIN */
export const createEmployee = async (
  req: CreateEmployeeRequest
): Promise<EmployeeDto> => {
  const response = await axios.post<ApiResponse<EmployeeDto>>(BASE, req);
  return response.data.data;
};

/** PUT /api/v1/employees/:id — HR_ADMIN / SUPER_ADMIN */
export const updateEmployee = async (
  id: string,
  req: UpdateEmployeeRequest
): Promise<EmployeeDto> => {
  const response = await axios.put<ApiResponse<EmployeeDto>>(
    `${BASE}/${id}`,
    req
  );
  return response.data.data;
};

/** DELETE /api/v1/employees/:id — soft deactivate */
export const deactivateEmployee = async (id: string): Promise<void> => {
  await axios.delete(`${BASE}/${id}`);
};

/** GET /api/v1/employees/me */
export const getMe = async (): Promise<EmployeeDto> => {
  const response = await axios.get<ApiResponse<EmployeeDto>>(`${BASE}/me`);
  return response.data.data;
};

/** PUT /api/v1/employees/me — self-edit name + phone */
export const selfEdit = async (req: SelfEditRequest): Promise<EmployeeDto> => {
  const response = await axios.put<ApiResponse<EmployeeDto>>(`${BASE}/me`, req);
  return response.data.data;
};

/** GET /api/v1/employees/team — Manager's direct reports */
export const getTeam = async (): Promise<EmployeeDto[]> => {
  const response = await axios.get<ApiResponse<EmployeeDto[]>>(`${BASE}/team`);
  return response.data.data;
};

/** POST /api/v1/employees/:id/anonymise — SUPER_ADMIN */
export const anonymiseEmployee = async (id: string): Promise<void> => {
  await axios.post(`${BASE}/${id}/anonymise`);
};

// ─── Department lookup (used by employee form dropdowns) ──────────────────────

/** GET /api/v1/departments — for dropdown population */
export const listDepartments = async (): Promise<DepartmentDto[]> => {
  const response = await axios.get<ApiResponse<DepartmentDto[]>>(DEPT_BASE);
  return response.data.data;
};
