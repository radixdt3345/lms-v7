import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { leaveApplicationApi, LeaveApplicationDto } from '../api/leaveApplicationApi';

interface State { applications: LeaveApplicationDto[]; loading: boolean; error: string | null; }
const initial: State = { applications: [], loading: false, error: null };

export const fetchMyApplications = createAsyncThunk('leaveApp/fetchMy', async () => {
  const res = await leaveApplicationApi.getMyApplications();
  return res.data.data;
});

export const fetchAllApplications = createAsyncThunk('leaveApp/fetchAll', async (status?: string) => {
  const res = await leaveApplicationApi.getAll(status);
  return res.data.data;
});

const slice = createSlice({
  name: 'leaveApplication',
  initialState: initial,
  reducers: { clearError: (s) => { s.error = null; } },
  extraReducers: (b) => {
    b.addCase(fetchMyApplications.pending, (s) => { s.loading = true; })
     .addCase(fetchMyApplications.fulfilled, (s, a) => { s.loading = false; s.applications = a.payload; })
     .addCase(fetchMyApplications.rejected, (s, a) => { s.loading = false; s.error = a.error.message ?? 'Failed'; })
     .addCase(fetchAllApplications.pending, (s) => { s.loading = true; })
     .addCase(fetchAllApplications.fulfilled, (s, a) => { s.loading = false; s.applications = a.payload; })
     .addCase(fetchAllApplications.rejected, (s, a) => { s.loading = false; s.error = a.error.message ?? 'Failed'; });
  },
});

export const { clearError } = slice.actions;
export const leaveApplicationReducer = slice.reducer;
