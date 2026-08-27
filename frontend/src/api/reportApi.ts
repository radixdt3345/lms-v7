import axios from 'axios';

const API = '/api/v1/reports';

export const downloadLeaveReport = async (year?: number): Promise<Blob> => {
  const res = await axios.get(`${API}/leave`, {
    params: year ? { year } : {},
    responseType: 'blob',
  });
  return res.data;
};

export const downloadCompOffReport = async (year?: number): Promise<Blob> => {
  const res = await axios.get(`${API}/comp-off`, {
    params: year ? { year } : {},
    responseType: 'blob',
  });
  return res.data;
};

export const downloadLeaveBalanceReport = async (year?: number): Promise<Blob> => {
  const res = await axios.get(`${API}/leave-balances`, {
    params: year ? { year } : {},
    responseType: 'blob',
  });
  return res.data;
};
