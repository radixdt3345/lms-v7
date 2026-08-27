import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { leaveBalanceApi, LeaveBalanceDto } from '../api/leaveBalanceApi';

interface LeaveBalanceState {
  balances: LeaveBalanceDto[];
  loading: boolean;
  error: string | null;
}

const initialState: LeaveBalanceState = { balances: [], loading: false, error: null };

export const fetchMyBalances = createAsyncThunk(
  'leaveBalance/fetchMy',
  async (year: number) => {
    const res = await leaveBalanceApi.getMyBalances(year);
    return res.data.data;
  }
);

export const fetchAllBalances = createAsyncThunk(
  'leaveBalance/fetchAll',
  async (year: number) => {
    const res = await leaveBalanceApi.getAllBalances(year);
    return res.data.data;
  }
);

const leaveBalanceSlice = createSlice({
  name: 'leaveBalance',
  initialState,
  reducers: { clearError: (s) => { s.error = null; } },
  extraReducers: (b) => {
    b.addCase(fetchMyBalances.pending, (s) => { s.loading = true; s.error = null; })
     .addCase(fetchMyBalances.fulfilled, (s, a) => { s.loading = false; s.balances = a.payload; })
     .addCase(fetchMyBalances.rejected, (s, a) => { s.loading = false; s.error = a.error.message ?? 'Failed'; })
     .addCase(fetchAllBalances.pending, (s) => { s.loading = true; s.error = null; })
     .addCase(fetchAllBalances.fulfilled, (s, a) => { s.loading = false; s.balances = a.payload; })
     .addCase(fetchAllBalances.rejected, (s, a) => { s.loading = false; s.error = a.error.message ?? 'Failed'; });
  },
});

export const { clearError } = leaveBalanceSlice.actions;
export const leaveBalanceReducer = leaveBalanceSlice.reducer;
