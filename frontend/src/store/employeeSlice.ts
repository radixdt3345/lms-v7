import { createSlice } from '@reduxjs/toolkit';
import type { EmployeeDto } from '../api/employeeApi';

// ─── State shape ─────────────────────────────────────────────────────────────

interface EmployeeState {
  /** Cached current user profile (from GET /me). Updated on self-edit success. */
  me: EmployeeDto | null;
}

const initialState: EmployeeState = {
  me: null,
};

// ─── Slice ───────────────────────────────────────────────────────────────────

const employeeSlice = createSlice({
  name: 'employee',
  initialState,
  reducers: {
    setMe(state, action: { payload: EmployeeDto }) {
      state.me = action.payload;
    },
    clearMe(state) {
      state.me = null;
    },
  },
});

export const { setMe, clearMe } = employeeSlice.actions;
export default employeeSlice.reducer;
