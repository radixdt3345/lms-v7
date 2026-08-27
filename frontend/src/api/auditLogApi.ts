import axios from 'axios';
import type { ApiResponse } from './types';

export interface AuditLogDto {
  id: string;
  actorUserId: string | null;
  actorName: string | null;
  actionType: string;
  recordType: string;
  recordId: string;
  oldValue: string | null;
  newValue: string | null;
  ipAddress: string | null;
  timestamp: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface AuditLogSearchParams {
  userId?: string;
  actionType?: string;
  recordType?: string;
  fromDate?: string;
  toDate?: string;
  page?: number;
  pageSize?: number;
}

export const searchAuditLogs = async (
  params: AuditLogSearchParams = {}
): Promise<PagedResult<AuditLogDto>> => {
  const response = await axios.get<ApiResponse<PagedResult<AuditLogDto>>>(
    '/api/v1/audit-log',
    { params }
  );
  return response.data.data;
};
