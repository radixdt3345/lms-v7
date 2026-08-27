import axios from 'axios';

export interface PublicHolidayDto {
  id: string;
  date: string;
  name: string;
  year: number;
  createdAt: string;
  updatedAt: string;
}

const BASE = '/api/v1/holidays';

export const listHolidays = async (year: number): Promise<PublicHolidayDto[]> => {
  const res = await axios.get<{ data: PublicHolidayDto[] }>(BASE, { params: { year } });
  return res.data.data;
};

export const createHoliday = async (data: { date: string; name: string }): Promise<PublicHolidayDto> => {
  const res = await axios.post<{ data: PublicHolidayDto }>(BASE, data);
  return res.data.data;
};

export const updateHoliday = async (id: string, data: { date?: string; name?: string }): Promise<PublicHolidayDto> => {
  const res = await axios.put<{ data: PublicHolidayDto }>(`${BASE}/${id}`, data);
  return res.data.data;
};

export const deleteHoliday = async (id: string): Promise<void> => {
  await axios.delete(`${BASE}/${id}`);
};

export const bulkImportHolidays = async (year: number, holidays: Array<{ date: string; name: string }>, confirm = false) => {
  const res = await axios.post<{ data: any }>(`${BASE}/bulk-import`, { year, holidays, confirm });
  return res.data.data;
};
