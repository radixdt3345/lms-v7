import axios from 'axios';
import type { ApiResponse } from './types';

export interface DepartmentDto {
  id: string;
  name: string;
  code: string;
  overlapLimit: number;
  status: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateDepartmentRequest {
  name: string;
  code: string;
  overlapLimit: number;
}

export interface UpdateDepartmentRequest {
  name?: string;
  code?: string;
  overlapLimit?: number;
  status?: string;
}

export const getDepartments = async (): Promise<DepartmentDto[]> => {
  const response = await axios.get<ApiResponse<DepartmentDto[]>>('/api/v1/departments');
  return response.data.data;
};

export const getDepartmentById = async (id: string): Promise<DepartmentDto> => {
  const response = await axios.get<ApiResponse<DepartmentDto>>(`/api/v1/departments/${id}`);
  return response.data.data;
};

export const createDepartment = async (
  data: CreateDepartmentRequest
): Promise<DepartmentDto> => {
  const response = await axios.post<ApiResponse<DepartmentDto>>('/api/v1/departments', data);
  return response.data.data;
};

export const updateDepartment = async (
  id: string,
  data: UpdateDepartmentRequest
): Promise<DepartmentDto> => {
  const response = await axios.put<ApiResponse<DepartmentDto>>(
    `/api/v1/departments/${id}`,
    data
  );
  return response.data.data;
};

export const deactivateDepartment = async (id: string): Promise<void> => {
  await axios.delete(`/api/v1/departments/${id}`);
};
