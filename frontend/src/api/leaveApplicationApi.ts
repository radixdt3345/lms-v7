import axios from 'axios';

export interface LeaveApplicationDto {
  id: string; employeeId: string; employeeName: string;
  leaveTypeId: string; leaveTypeName: string;
  startDate: string; endDate: string; totalDays: number;
  reason: string; status: string;
  approvedById?: string; approvedByName?: string;
  approvedAt?: string; rejectionReason?: string; createdAt: string;
}

export interface SubmitLeaveApplicationRequest {
  leaveTypeId: string; startDate: string; endDate: string; reason: string;
}

const API = '/api/v1/leave-applications';

export const leaveApplicationApi = {
  getMyApplications: () => axios.get<{ data: LeaveApplicationDto[] }>(`${API}/me`),
  getAll: (status?: string) => axios.get<{ data: LeaveApplicationDto[] }>(`${API}${status ? `?status=${status}` : ''}`),
  getById: (id: string) => axios.get<{ data: LeaveApplicationDto }>(`${API}/${id}`),
  submit: (req: SubmitLeaveApplicationRequest) => axios.post<{ data: LeaveApplicationDto }>(API, req),
  approve: (id: string) => axios.put<{ data: LeaveApplicationDto }>(`${API}/${id}/approve`),
  reject: (id: string, rejectionReason: string) => axios.put<{ data: LeaveApplicationDto }>(`${API}/${id}/reject`, { rejectionReason }),
  cancel: (id: string) => axios.delete(`${API}/${id}/cancel`),
};
