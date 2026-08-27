import axios from 'axios';

export interface LeaveBalanceDto {
  id: string;
  employeeId: string;
  employeeName: string;
  leaveTypeId: string;
  leaveTypeName: string;
  year: number;
  totalDays: number;
  usedDays: number;
  pendingDays: number;
  remainingDays: number;
}

export interface AdjustBalanceRequest {
  employeeId: string;
  leaveTypeId: string;
  year: number;
  adjustmentDays: number;
  reason: string;
}

const API = '/api/v1/leave-balances';

export const leaveBalanceApi = {
  getMyBalances: (year: number) =>
    axios.get<{ data: LeaveBalanceDto[] }>(`${API}/me?year=${year}`),
  getEmployeeBalances: (employeeId: string, year: number) =>
    axios.get<{ data: LeaveBalanceDto[] }>(`${API}/${employeeId}?year=${year}`),
  getAllBalances: (year: number) =>
    axios.get<{ data: LeaveBalanceDto[] }>(`${API}?year=${year}`),
  creditAnnual: (year: number) =>
    axios.post(`${API}/credit`, { year }),
  adjustBalance: (req: AdjustBalanceRequest) =>
    axios.post(`${API}/adjust`, req),
};
