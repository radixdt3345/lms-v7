import axios from 'axios';
export interface CompOffRequestDto { id: string; employeeId: string; employeeName: string; workedDate: string; creditDays: number; reason: string; status: string; approvedByName?: string; rejectionReason?: string; createdAt: string; }
export interface CompOffCreditDto { id: string; employeeId: string; employeeName: string; earnedDate: string; expiryDate: string; creditDays: number; status: string; }
const API = '/api/v1/comp-off';
export const compOffApi = {
  getMyRequests: () => axios.get<{ data: CompOffRequestDto[] }>(`${API}/requests/me`),
  getAllRequests: (status?: string) => axios.get<{ data: CompOffRequestDto[] }>(`${API}/requests${status ? `?status=${status}` : ''}`),
  submit: (req: { workedDate: string; creditDays: number; reason: string }) => axios.post<{ data: CompOffRequestDto }>(`${API}/requests`, req),
  approve: (id: string) => axios.put<{ data: CompOffRequestDto }>(`${API}/requests/${id}/approve`),
  reject: (id: string, rejectionReason: string) => axios.put<{ data: CompOffRequestDto }>(`${API}/requests/${id}/reject`, { rejectionReason }),
  getMyCredits: () => axios.get<{ data: CompOffCreditDto[] }>(`${API}/credits/me`),
};
