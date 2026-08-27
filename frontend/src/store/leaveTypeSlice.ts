import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';
import {
  listLeaveTypes,
  createLeaveType,
  updateLeaveType,
  deactivateLeaveType,
  type LeaveTypeDto,
  type CreateLeaveTypeRequest,
  type UpdateLeaveTypeRequest,
} from '../api/leaveTypeApi';

interface LeaveTypeState {
  items: LeaveTypeDto[];
  loading: boolean;
  error: string | null;
}

const initialState: LeaveTypeState = {
  items: [],
  loading: false,
  error: null,
};

export const fetchLeaveTypes = createAsyncThunk('leaveType/fetchAll', async () => {
  return await listLeaveTypes();
});

export const addLeaveType = createAsyncThunk(
  'leaveType/create',
  async (req: CreateLeaveTypeRequest) => {
    return await createLeaveType(req);
  }
);

export const editLeaveType = createAsyncThunk(
  'leaveType/update',
  async ({ id, req }: { id: string; req: UpdateLeaveTypeRequest }) => {
    return await updateLeaveType(id, req);
  }
);

export const removeLeaveType = createAsyncThunk(
  'leaveType/deactivate',
  async (id: string) => {
    await deactivateLeaveType(id);
    return id;
  }
);

const leaveTypeSlice = createSlice({
  name: 'leaveType',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchLeaveTypes.pending, (state) => { state.loading = true; state.error = null; })
      .addCase(fetchLeaveTypes.fulfilled, (state, action) => { state.loading = false; state.items = action.payload; })
      .addCase(fetchLeaveTypes.rejected, (state, action) => { state.loading = false; state.error = action.error.message ?? 'Failed to load leave types'; })
      .addCase(addLeaveType.fulfilled, (state, action) => { state.items.push(action.payload); })
      .addCase(editLeaveType.fulfilled, (state, action) => {
        const idx = state.items.findIndex((i) => i.id === action.payload.id);
        if (idx >= 0) state.items[idx] = action.payload;
      })
      .addCase(removeLeaveType.fulfilled, (state, action) => {
        const idx = state.items.findIndex((i) => i.id === action.payload);
        if (idx >= 0) state.items[idx] = { ...state.items[idx], isActive: false };
      });
  },
});

export default leaveTypeSlice.reducer;
