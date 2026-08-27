import axios from 'axios';
import { ApiResponse } from '../types';

const API = '/api/v1/jobs';

export interface JobLog {
  id: string;
  jobName: string;
  status: string;
  details?: string;
  startedAt: string;
  completedAt?: string;
}

export const triggerExpireCompOff = async (): Promise<string> => {
  const res = await axios.post<ApiResponse<string>>(`${API}/expire-comp-off`);
  return res.data.data;
};

export const triggerResetLeaveBalances = async (year?: number): Promise<string> => {
  const res = await axios.post<ApiResponse<string>>(`${API}/reset-leave-balances`, null, { params: year ? { year } : {} });
  return res.data.data;
};

export const triggerSendReminders = async (): Promise<string> => {
  const res = await axios.post<ApiResponse<string>>(`${API}/send-reminders`);
  return res.data.data;
};

export const fetchJobLogs = async (): Promise<JobLog[]> => {
  const res = await axios.get<ApiResponse<JobLog[]>>(`${API}/logs`);
  return res.data.data;
};
