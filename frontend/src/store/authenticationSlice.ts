import { createAsyncThunk, createSlice, type PayloadAction } from '@reduxjs/toolkit';
import * as authApi from '../api/authenticationApi';

// State shape

interface AuthState {
  accessToken: string | null;
  isAuthenticated: boolean;
  role: string | null;
  name: string | null;
  email: string | null;
  isLoading: boolean;
  error: string | null;
}

const initialState: AuthState = {
  accessToken: null,
  isAuthenticated: false,
  role: null,
  name: null,
  email: null,
  isLoading: false,
  error: null,
};

// Helpers

/** Decode JWT payload without verifying signature (client-side display only). */
const decodeJwtPayload = (token: string): Record<string, unknown> => {
  try {
    const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
    return JSON.parse(atob(base64)) as Record<string, unknown>;
  } catch {
    return {};
  }
};

const applyToken = (state: AuthState, token: string): void => {
  state.accessToken = token;
  state.isAuthenticated = true;
  const payload = decodeJwtPayload(token);
  state.role = (payload['role'] as string) ?? null;
  state.name = (payload['name'] as string) ?? null;
  state.email = (payload['email'] as string) ?? null;
};

// Thunks

export const loginAsync = createAsyncThunk(
  'authentication/login',
  async (credentials: authApi.LoginCredentials, { rejectWithValue }) => {
    try {
      return await authApi.loginWithCredentials(credentials);
    } catch (err: unknown) {
      const detail =
        (err as { response?: { data?: { detail?: string } } }).response?.data?.detail ??
        'Login failed';
      return rejectWithValue(detail);
    }
  }
);

export const logoutAsync = createAsyncThunk('authentication/logout', async () => {
  await authApi.logout();
});

// Slice

const authenticationSlice = createSlice({
  name: 'authentication',
  initialState,
  reducers: {
    /** Called after SSO callback - token comes from URL / backend response. */
    setToken(state, action: PayloadAction<string>) {
      applyToken(state, action.payload);
      state.error = null;
    },
    clearAuth(state) {
      state.accessToken = null;
      state.isAuthenticated = false;
      state.role = null;
      state.name = null;
      state.email = null;
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(loginAsync.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(loginAsync.fulfilled, (state, action) => {
        state.isLoading = false;
        applyToken(state, action.payload.accessToken);
      })
      .addCase(loginAsync.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      .addCase(logoutAsync.fulfilled, () => {
        return { ...initialState };
      });
  },
});

export const { setToken, clearAuth } = authenticationSlice.actions;
export default authenticationSlice.reducer;
