import axios from 'axios';
import { ApiResponse } from '../types';

export interface PendingApprovalDto {
  id: string;
  entityType: string;
  employeeName: string;
  description: string;
  status: string;
  submittedAt: string;
}

export interface ApprovalHistoryDto {
  id: string;
  entityType: string;
  entityId: string;
  actorName: string;
  action: string;
  comments?: string;
  actedAt: string;
}

const BASE = "/api/v1/approvals";

export const fetchPendingApprovals = async (): Promise<ApiResponse<PendingApprovalDto[]>> => {
  const res = await axios.get<ApiResponse<PendingApprovalDto[]>>(`${BASE}/pending`);
  return res.data;
};

export const fetchApprovalHistory = async (entityType: string, entityId: string): Promise<ApiResponse<ApprovalHistoryDto[]>> => {
  const res = await axios.get<ApiResponse<ApprovalHistoryDto[]>>(`${BASE}/history/${entityType}/${entityId}`);
  return res.data;
};
