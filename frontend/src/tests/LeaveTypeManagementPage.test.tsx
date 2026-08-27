import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { Provider } from 'react-redux';
import { configureStore } from '@reduxjs/toolkit';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import LeaveTypeManagementPage from '../pages/LeaveTypeManagement/LeaveTypeManagementPage';
import leaveTypeReducer from '../store/leaveTypeSlice';
import * as leaveTypeApi from '../api/leaveTypeApi';

vi.mock('../api/leaveTypeApi');

const mockLeaveTypes: leaveTypeApi.LeaveTypeDto[] = [
  {
    id: '00000000-0000-0000-0000-000000000001',
    name: 'Casual Leave',
    code: 'CL',
    description: 'Casual leave for personal matters',
    annualDays: 12,
    requiresAttachment: false,
    requiresHrApproval: false,
    isActive: true,
    createdAt: '2026-08-27T10:00:00Z',
    updatedAt: '2026-08-27T10:00:00Z',
  },
  {
    id: '00000000-0000-0000-0000-000000000002',
    name: 'Sick Leave',
    code: 'SL',
    description: 'Medical leave',
    annualDays: 6,
    requiresAttachment: true,
    requiresHrApproval: true,
    isActive: false,
    createdAt: '2026-08-27T10:00:00Z',
    updatedAt: '2026-08-27T10:00:00Z',
  },
];

const makeStore = () =>
  configureStore({ reducer: { leaveType: leaveTypeReducer } });

const renderPage = () => {
  const store = makeStore();
  return render(
    <Provider store={store}>
      <LeaveTypeManagementPage />
    </Provider>
  );
};

describe('LeaveTypeManagementPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders the page with table and create button', async () => {
    vi.mocked(leaveTypeApi.listLeaveTypes).mockResolvedValue(mockLeaveTypes);
    renderPage();
    expect(screen.getByText('Leave Type Management')).toBeDefined();
    expect(screen.getByTestId('create-leave-type-btn')).toBeDefined();
    expect(screen.getByTestId('leave-type-table')).toBeDefined();
  });

  it('shows loading state and then displays leave types', async () => {
    vi.mocked(leaveTypeApi.listLeaveTypes).mockResolvedValue(mockLeaveTypes);
    renderPage();
    await waitFor(() => {
      expect(screen.getByText('Casual Leave')).toBeDefined();
    });
  });

  it('shows error alert when fetch fails', async () => {
    vi.mocked(leaveTypeApi.listLeaveTypes).mockRejectedValue(new Error('Network error'));
    renderPage();
    await waitFor(() => {
      expect(screen.getByText(/failed to load/i)).toBeDefined();
    });
  });

  it('opens create dialog when create button is clicked', async () => {
    vi.mocked(leaveTypeApi.listLeaveTypes).mockResolvedValue([]);
    renderPage();
    fireEvent.click(screen.getByTestId('create-leave-type-btn'));
    await waitFor(() => {
      expect(screen.getByTestId('leave-type-dialog')).toBeDefined();
      expect(screen.getByTestId('leave-type-name-input')).toBeDefined();
      expect(screen.getByTestId('leave-type-code-input')).toBeDefined();
      expect(screen.getByTestId('leave-type-annual-days-input')).toBeDefined();
    });
  });

  it('creates a leave type and shows success snackbar', async () => {
    vi.mocked(leaveTypeApi.listLeaveTypes).mockResolvedValue([]);
    vi.mocked(leaveTypeApi.createLeaveType).mockResolvedValue(mockLeaveTypes[0]);
    renderPage();

    fireEvent.click(screen.getByTestId('create-leave-type-btn'));
    await waitFor(() => screen.getByTestId('leave-type-name-input'));

    fireEvent.change(screen.getByTestId('leave-type-name-input'), {
      target: { value: 'Study Leave' },
    });
    fireEvent.change(screen.getByTestId('leave-type-code-input'), {
      target: { value: 'STL' },
    });

    fireEvent.click(screen.getByTestId('leave-type-submit-btn'));
    await waitFor(() => {
      expect(screen.getByTestId('success-snackbar')).toBeDefined();
    });
  });

  it('deactivate button calls deactivateLeaveType', async () => {
    vi.mocked(leaveTypeApi.listLeaveTypes).mockResolvedValue(mockLeaveTypes);
    vi.mocked(leaveTypeApi.deactivateLeaveType).mockResolvedValue();
    renderPage();
    await waitFor(() => screen.getAllByTestId('deactivate-leave-type-btn'));
    const btns = screen.getAllByTestId('deactivate-leave-type-btn');
    fireEvent.click(btns[0]);
    await waitFor(() => {
      expect(leaveTypeApi.deactivateLeaveType).toHaveBeenCalled();
    });
  });
});
