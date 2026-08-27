import axios from 'axios';
import type { ApiResponse } from './types';

export interface LeaveTypeDto {
  id: string;
  name: string;
  code: string;
  description: string | null;
  annualDays: number;
  requiresAttachment: boolean;
  requiresHrApproval: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateLeaveTypeRequest {
  name: string;
  code: string;
  description?: string;
  annualDays: number;
  requiresAttachment: boolean;
  requiresHrApproval: boolean;
}

export interface UpdateLeaveTypeRequest {
  name?: string;
  code?: string;
  description?: string;
  annualDays?: number;
  requiresAttachment?: boolean;
  requiresHrApproval?: boolean;
  isActive?: boolean;
}

export const listLeaveTypes = async (): Promise<LeaveTypeDto[]> => {
  const response = await axios.get<ApiResponse<LeaveTypeDto[]>>('/api/v1/leave-types');
  return response.data.data;
};

export const getLeaveTypeById = async (id: string): Promise<LeaveTypeDto> => {
  const response = await axios.get<ApiResponse<LeaveTypeDto>>(`/api/v1/leave-types/${id}`);
  return response.data.data;
};

export const createLeaveType = async (data: CreateLeaveTypeRequest): Promise<LeaveTypeDto> => {
  const response = await axios.post<ApiResponse<LeaveTypeDto>>('/api/v1/leave-types', data);
  return response.data.data;
};

export const updateLeaveType = async (
  id: string,
  data: UpdateLeaveTypeRequest
): Promise<LeaveTypeDto> => {
  const response = await axios.put<ApiResponse<LeaveTypeDto>>(`/api/v1/leave-types/${id}`, data);
  return response.data.data;
};

export const deactivateLeaveType = async (id: string): Promise<void> => {
  await axios.delete(`/api/v1/leave-types/${id}`);
};
