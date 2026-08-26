import { createAsyncThunk, createSlice, type PayloadAction } from '@reduxjs/toolkit';
import {
  getDepartments,
  createDepartment,
  updateDepartment,
  deactivateDepartment,
  type DepartmentDto,
  type CreateDepartmentRequest,
  type UpdateDepartmentRequest,
} from '../api/departmentApi';

// ─── State ───────────────────────────────────────────────────────────────────

interface DepartmentState {
  departments: DepartmentDto[];
  loading: boolean;
  error: string | null;
  duplicateError: boolean;
}

const initialState: DepartmentState = {
  departments: [],
  loading: false,
  error: null,
  duplicateError: false,
};

// ─── Thunks ──────────────────────────────────────────────────────────────────

export const fetchDepartments = createAsyncThunk(
  'department/fetchAll',
  async (_, { rejectWithValue }) => {
    try {
      return await getDepartments();
    } catch {
      return rejectWithValue('Failed to load departments');
    }
  }
);

export const createDepartmentAsync = createAsyncThunk(
  'department/create',
  async (data: CreateDepartmentRequest, { rejectWithValue }) => {
    try {
      return await createDepartment(data);
    } catch (err: unknown) {
      const status = (err as { response?: { status?: number } }).response?.status;
      if (status === 409) return rejectWithValue('DUPLICATE_DEPARTMENT_NAME');
      return rejectWithValue('Failed to create department');
    }
  }
);

export const updateDepartmentAsync = createAsyncThunk(
  'department/update',
  async (
    { id, data }: { id: string; data: UpdateDepartmentRequest },
    { rejectWithValue }
  ) => {
    try {
      return await updateDepartment(id, data);
    } catch (err: unknown) {
      const status = (err as { response?: { status?: number } }).response?.status;
      if (status === 409) return rejectWithValue('DUPLICATE_DEPARTMENT_NAME');
      return rejectWithValue('Failed to update department');
    }
  }
);

export const deactivateDepartmentAsync = createAsyncThunk(
  'department/deactivate',
  async (id: string, { rejectWithValue }) => {
    try {
      await deactivateDepartment(id);
      return id;
    } catch {
      return rejectWithValue('Failed to deactivate department');
    }
  }
);

// ─── Slice ────────────────────────────────────────────────────────────────────

const departmentSlice = createSlice({
  name: 'department',
  initialState,
  reducers: {
    clearDepartmentError(state) {
      state.error = null;
      state.duplicateError = false;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchDepartments.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchDepartments.fulfilled, (state, action: PayloadAction<DepartmentDto[]>) => {
        state.loading = false;
        state.departments = action.payload;
      })
      .addCase(fetchDepartments.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      .addCase(createDepartmentAsync.pending, (state) => {
        state.loading = true;
        state.error = null;
        state.duplicateError = false;
      })
      .addCase(createDepartmentAsync.fulfilled, (state, action: PayloadAction<DepartmentDto>) => {
        state.loading = false;
        state.departments.push(action.payload);
      })
      .addCase(createDepartmentAsync.rejected, (state, action) => {
        state.loading = false;
        if (action.payload === 'DUPLICATE_DEPARTMENT_NAME') {
          state.duplicateError = true;
        } else {
          state.error = action.payload as string;
        }
      })
      .addCase(updateDepartmentAsync.pending, (state) => {
        state.loading = true;
        state.error = null;
        state.duplicateError = false;
      })
      .addCase(updateDepartmentAsync.fulfilled, (state, action: PayloadAction<DepartmentDto>) => {
        state.loading = false;
        const idx = state.departments.findIndex((d) => d.id === action.payload.id);
        if (idx !== -1) state.departments[idx] = action.payload;
      })
      .addCase(updateDepartmentAsync.rejected, (state, action) => {
        state.loading = false;
        if (action.payload === 'DUPLICATE_DEPARTMENT_NAME') {
          state.duplicateError = true;
        } else {
          state.error = action.payload as string;
        }
      })
      .addCase(deactivateDepartmentAsync.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(deactivateDepartmentAsync.fulfilled, (state, action: PayloadAction<string>) => {
        state.loading = false;
        const dept = state.departments.find((d) => d.id === action.payload);
        if (dept) dept.status = 'Inactive';
      })
      .addCase(deactivateDepartmentAsync.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });
  },
});

export const { clearDepartmentError } = departmentSlice.actions;
export const departmentReducer = departmentSlice.reducer;
