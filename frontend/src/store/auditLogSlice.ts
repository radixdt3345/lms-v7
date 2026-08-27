import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { AuditLogDto, PagedResult, AuditLogSearchParams } from '../api/auditLogApi';
import { searchAuditLogs } from '../api/auditLogApi';

interface AuditLogState {
  result: PagedResult<AuditLogDto> | null;
  loading: boolean;
  error: string | null;
}

const initialState: AuditLogState = {
  result: null,
  loading: false,
  error: null,
};

export const fetchAuditLogs = createAsyncThunk(
  'auditLog/search',
  async (params: AuditLogSearchParams) => searchAuditLogs(params)
);

const auditLogSlice = createSlice({
  name: 'auditLog',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchAuditLogs.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchAuditLogs.fulfilled, (state, action) => {
        state.loading = false;
        state.result = action.payload;
      })
      .addCase(fetchAuditLogs.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message ?? 'Failed to load audit logs';
      });
  },
});

export const auditLogReducer = auditLogSlice.reducer;
