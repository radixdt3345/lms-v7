import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { compOffApi, CompOffRequestDto } from '../api/compOffApi';
interface State { requests: CompOffRequestDto[]; loading: boolean; error: string | null; }
export const fetchMyCompOffRequests = createAsyncThunk('compOff/fetchMy', async () => (await compOffApi.getMyRequests()).data.data);
export const fetchAllCompOffRequests = createAsyncThunk('compOff/fetchAll', async () => (await compOffApi.getAllRequests()).data.data);
const slice = createSlice({ name: 'compOff', initialState: { requests: [], loading: false, error: null } as State, reducers: {},
  extraReducers: (b) => {
    b.addCase(fetchMyCompOffRequests.pending, (s) => { s.loading = true; })
     .addCase(fetchMyCompOffRequests.fulfilled, (s, a) => { s.loading = false; s.requests = a.payload; })
     .addCase(fetchMyCompOffRequests.rejected, (s, a) => { s.loading = false; s.error = a.error.message ?? 'Failed'; })
     .addCase(fetchAllCompOffRequests.pending, (s) => { s.loading = true; })
     .addCase(fetchAllCompOffRequests.fulfilled, (s, a) => { s.loading = false; s.requests = a.payload; })
     .addCase(fetchAllCompOffRequests.rejected, (s, a) => { s.loading = false; s.error = a.error.message ?? 'Failed'; });
  }
});
export const compOffReducer = slice.reducer;
