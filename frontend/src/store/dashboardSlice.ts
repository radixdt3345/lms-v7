import { createAsyncThunk, createSlice } from "@reduxjs/toolkit";
import { fetchEmployeeDashboard, fetchHrDashboard, EmployeeDashboardDto, HrDashboardDto } from "../api/dashboardApi";

interface DashboardState { employee: EmployeeDashboardDto | null; hr: HrDashboardDto | null; loading: boolean; error: string | null; }
const init: DashboardState = { employee: null, hr: null, loading: false, error: null };

export const fetchEmployeeDashboardThunk = createAsyncThunk("dashboard/fetchEmployee", async () =>
  (await fetchEmployeeDashboard()).data);
export const fetchHrDashboardThunk = createAsyncThunk("dashboard/fetchHr", async () =>
  (await fetchHrDashboard()).data);

const slice = createSlice({ name: "dashboard", initialState: init, reducers: {},
  extraReducers: (b) => {
    b.addCase(fetchEmployeeDashboardThunk.pending, (s) => { s.loading = true; s.error = null; });
    b.addCase(fetchEmployeeDashboardThunk.fulfilled, (s, a) => { s.loading = false; s.employee = a.payload ?? null; });
    b.addCase(fetchEmployeeDashboardThunk.rejected, (s, a) => { s.loading = false; s.error = a.error.message ?? "Error"; });
    b.addCase(fetchHrDashboardThunk.fulfilled, (s, a) => { s.hr = a.payload ?? null; });
  }
});
export const dashboardReducer = slice.reducer;
