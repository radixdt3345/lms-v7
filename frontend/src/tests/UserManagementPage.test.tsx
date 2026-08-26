import React from 'react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { Provider } from 'react-redux';
import { MemoryRouter } from 'react-router-dom';
import { configureStore } from '@reduxjs/toolkit';
import authenticationReducer from '../store/authenticationSlice';
import UserManagementPage from '../pages/AuthenticationAndIdentity/UserManagementPage';
import * as authApi from '../api/authenticationApi';
import type { LockedUserDto } from '../api/authenticationApi';

// Mocks

vi.mock('../api/authenticationApi', () => ({
  getSsoLoginUrl: vi.fn(),
  loginWithCredentials: vi.fn(),
  logout: vi.fn(),
  getLockedAccounts: vi.fn(),
  unlockAccount: vi.fn(),
}));

// Mock MUI DataGrid with a functional test double
vi.mock('@mui/x-data-grid', () => ({
  DataGrid: ({
    rows,
    loading,
    columns,
  }: {
    rows: LockedUserDto[];
    loading: boolean;
    columns: {
      field: string;
      renderCell?: (p: { row: LockedUserDto }) => React.ReactNode;
    }[];
  }) => {
    if (loading) return <div role="progressbar" data-testid="grid-loading" />;
    if (!rows.length) return <div data-testid="empty-grid">No locked accounts</div>;
    return (
      <div data-testid="grid">
        {rows.map((row) => {
          const actionsCol = columns.find((c) => c.field === 'actions');
          return (
            <div key={row.id} data-testid={`grid-row-${row.id}`}>
              <span>{row.name}</span>
              <span>{row.email}</span>
              {actionsCol?.renderCell?.({ row })}
            </div>
          );
        })}
      </div>
    );
  },
}));

// Test data

const lockedUser: LockedUserDto = {
  id: 'user-abc-123',
  name: 'Locked Employee',
  email: 'locked@example.com',
  role: 'EMPLOYEE',
  failedAttempts: 3,
  lockedAt: new Date().toISOString(),
};

// Helpers

function makeStore() {
  return configureStore({
    reducer: { authentication: authenticationReducer },
    preloadedState: {
      authentication: {
        accessToken: 'tok.mock.value',
        isAuthenticated: true,
        role: 'HR_ADMIN',
        name: 'HR Admin',
        email: 'admin@example.com',
        isLoading: false,
        error: null,
      },
    },
  });
}

function renderPage() {
  return render(
    <Provider store={makeStore()}>
      <MemoryRouter>
        <UserManagementPage />
      </MemoryRouter>
    </Provider>
  );
}

// Tests

describe('UserManagementPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  // UT-FE-007: table container renders immediately (before data loads)
  it('UT-FE-007: renders table-locked-accounts container on mount', () => {
    vi.mocked(authApi.getLockedAccounts).mockReturnValue(new Promise(() => {}));
    renderPage();
    expect(screen.getByTestId('table-locked-accounts')).toBeInTheDocument();
  });

  // UT-FE-008: populated table renders locked accounts
  it('UT-FE-008: renders locked user row after data loads', async () => {
    vi.mocked(authApi.getLockedAccounts).mockResolvedValue([lockedUser]);
    renderPage();
    await waitFor(() =>
      expect(screen.getByTestId(`grid-row-${lockedUser.id}`)).toBeInTheDocument()
    );
    expect(screen.getByText('Locked Employee')).toBeInTheDocument();
    expect(screen.getByText('locked@example.com')).toBeInTheDocument();
  });

  // UT-FE-009: empty state when no locked accounts
  it('UT-FE-009: shows empty-grid when no locked accounts exist', async () => {
    vi.mocked(authApi.getLockedAccounts).mockResolvedValue([]);
    renderPage();
    await waitFor(() =>
      expect(screen.getByTestId('empty-grid')).toBeInTheDocument()
    );
  });

  // UT-FE-010: unlock button present for each row
  it('UT-FE-010: renders btn-unlock-account for each locked user', async () => {
    vi.mocked(authApi.getLockedAccounts).mockResolvedValue([lockedUser]);
    renderPage();
    await waitFor(() =>
      expect(screen.getByTestId('btn-unlock-account')).toBeInTheDocument()
    );
  });

  // UT-FE-011: unlock success refreshes the list
  it('UT-FE-011: clicking unlock removes the account from the list', async () => {
    vi.mocked(authApi.getLockedAccounts)
      .mockResolvedValueOnce([lockedUser])
      .mockResolvedValueOnce([]);
    vi.mocked(authApi.unlockAccount).mockResolvedValue(true);
    renderPage();
    await waitFor(() =>
      expect(screen.getByTestId('btn-unlock-account')).toBeInTheDocument()
    );
    fireEvent.click(screen.getByTestId('btn-unlock-account'));
    await waitFor(() =>
      expect(screen.getByTestId('empty-grid')).toBeInTheDocument()
    );
    expect(authApi.unlockAccount).toHaveBeenCalledWith(lockedUser.id);
  });

  // UT-FE-012: fetch error shows error alert
  it('UT-FE-012: shows error message when getLockedAccounts rejects', async () => {
    vi.mocked(authApi.getLockedAccounts).mockRejectedValue(new Error('network error'));
    renderPage();
    await waitFor(() =>
      expect(screen.getByText(/Failed to load locked accounts/)).toBeInTheDocument()
    );
  });
});
